using System.Collections;
using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    public enum AppMode { Setup, Running, Paused, Extinct }

    /// <summary>
    /// The small state machine that owns the Run: Setup, Running, Paused, Extinct (PLAN 14.5).
    /// Also runs a lightweight ambient preview World behind the Liquid Glass Setup screen while keeping Sim == null in Setup.
    /// </summary>
    public sealed class AppController : MonoBehaviour
    {
        public static AppController Instance { get; private set; }

        public Simulation Sim { get; private set; }
        public AppMode Mode { get; private set; } = AppMode.Setup;
        public int SpeedIndex { get; private set; }
        public bool TintOn { get; private set; }
        public bool PanelOpen { get; private set; } = true;
        public SetupSettings SetupDraft = Presets.Get("Mild world");
        public EventSpec Armed { get; private set; }
        public bool Loading { get; private set; }
        public float EffectiveSpeed { get { return runner.EffectiveSpeed; } }
        public readonly LiveNarrator Narrator = new LiveNarrator();

        public WorldView WorldView { get; private set; }
        public CameraController Cam { get; private set; }
        public UiRoot Ui { get; private set; }
        public InputRouter Input { get; private set; }

        readonly SimulationRunner runner = new SimulationRunner();
        readonly SimulationRunner previewRunner = new SimulationRunner();
        Simulation previewSim;
        string previewSignature;
        int fitFrames;
        int loadVersion;
        AppMode beforeLoad;

        public static AppController Create(GameObject host, WorldView world, CameraController cam)
        {
            var app = host.AddComponent<AppController>();
            app.WorldView = world;
            app.Cam = cam;
            return app;
        }

        public void Attach(UiRoot ui, InputRouter input)
        {
            Ui = ui;
            Input = input;
            RefreshSetupPreview();
        }

        void Awake() { Instance = this; }

        public bool HasRun { get { return Sim != null; } }
        public bool Paused { get { return Mode == AppMode.Paused; } }
        public int SpeedMultiplier { get { return SimulationRunner.Speeds[SpeedIndex]; } }

        // ------------------------------------------------------------ ambient setup preview
        static string DraftSignature(SetupSettings s)
        {
            if (s == null) return "";
            return (s.SeedText ?? "mild") + "|" + s.FoodAbundance.ToString("0.00") + "|" + s.TemperatureRange.ToString("0.00")
                + "|" + s.ZoneScale.ToString("0.00") + "|" + s.ToxinLevel.ToString("0.00") + "|" + s.MutationRate.ToString("0.00");
        }

        public void RefreshSetupPreview()
        {
            if (Mode != AppMode.Setup || Sim != null) return;
            string sig = DraftSignature(SetupDraft);
            if (previewSim != null && sig == previewSignature && !previewSim.Extinct) return;
            previewSignature = sig;
            SetupSettings copy = SetupDraft != null ? SetupDraft.Clone() : Presets.Get("Mild world");
            if (string.IsNullOrWhiteSpace(copy.SeedText)) copy.SeedText = "mild";
            previewSim = Simulation.Create(copy, new SimParameters());
            // Warm up 4 seconds so Cells are already swimming gracefully when Setup opens
            for (int i = 0; i < 80 && !previewSim.Extinct; i++) previewSim.Step();
            previewRunner.Reset();
            WorldView.SetSimulation(previewSim);
            WorldView.TintOn = false;
            Cam.SetWorld(previewSim.World.Width, previewSim.World.Height);
            Cam.FitCentered(0.88f);
        }

        // ------------------------------------------------------------ frame
        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (Input != null) Input.Tick();
            if (Sim != null && Mode == AppMode.Running && !Ui.WindowTooSmall)
            {
                runner.Advance(Sim, dt, SpeedMultiplier);
                if (Sim.Extinct)
                {
                    Mode = AppMode.Extinct;
                    Armed = null;
                    Ui.OnModeChanged();
                }
            }
            if (Sim != null)
            {
                WorldView.TintOn = TintOn;
                if (Armed != null) WorldView.PreviewKind = Armed.Kind;
                ValidateHighlight();
                Narrator.Tick(dt);
                Ui.SyncViewport();
                if (fitFrames > 0) { Cam.Fit(); fitFrames--; }
            }
            else if (Mode == AppMode.Setup)
            {
                if (previewSim == null || previewSim.Extinct) RefreshSetupPreview();
                else if (!Ui.WindowTooSmall && !Ui.ModalOpen) previewRunner.Advance(previewSim, dt, 1);
                Cam.FitCentered(0.88f);
            }
            WorldView.Render(Cam, dt);
            Ui.Tick(dt);
        }

        // ------------------------------------------------------------ runs
        public void StartRun(SetupSettings setup)
        {
            Begin(Simulation.Create(setup.Clone(), new SimParameters()), AppMode.Running);
        }

        public void Restart()
        {
            if (Sim != null)
            {
                SetupSettings settings = Sim.Setup.Clone();
                GuardRunChange("Restart this Run?", "Restart", () => StartRun(settings));
            }
        }

        void Begin(Simulation sim, AppMode mode)
        {
            CancelLoad();
            previewSim = null;
            Sim = sim;
            FileBridge.ProtectRun(true);
            Mode = mode;
            Armed = null;
            runner.Reset();
            Narrator.Bind(sim);
            WorldView.SetSimulation(sim);
            WorldView.TintOn = TintOn;
            Cam.SetWorld(sim.World.Width, sim.World.Height);
            fitFrames = 4;
            Ui.OnRunStarted();
        }

        public void GoToSetup()
        {
            GuardRunChange("Return to the menu?", "Discard", DiscardToSetup);
        }

        void DiscardToSetup()
        {
            CancelLoad();
            SetupDraft = Sim != null ? Sim.Setup.Clone() : SetupDraft;
            Sim = null;
            Narrator.Bind(null);
            FileBridge.ProtectRun(false);
            Mode = AppMode.Setup;
            Armed = null;
            WorldView.SetSimulation(null);
            previewSignature = null;
            RefreshSetupPreview();
            Ui.OnModeChanged();
        }

        void GuardRunChange(string title, string action, System.Action proceed)
        {
            if (Sim == null) { proceed(); return; }
            Simulation expected = Sim;
            AppMode previous = Mode;
            if (Mode == AppMode.Running) Mode = AppMode.Paused;
            Ui.ConfirmRunAction(title, action,
                () => { if (Sim == expected) proceed(); },
                () => { if (Sim == expected) { Mode = previous; Ui.OnModeChanged(); } });
        }

        // ------------------------------------------------------------ transport
        public void TogglePause()
        {
            if (Sim == null || Mode == AppMode.Extinct) return;
            Mode = Mode == AppMode.Running ? AppMode.Paused : AppMode.Running;
            Ui.OnModeChanged();
        }

        public void SetSpeed(int index)
        {
            SpeedIndex = Mathf.Clamp(index, 0, SimulationRunner.Speeds.Length - 1);
        }

        public void ToggleTint() { TintOn = !TintOn; }

        public void TogglePanel() { PanelOpen = !PanelOpen; Ui.OnPanelToggled(); }

        public void FitView() { Cam.Fit(); }

        // ------------------------------------------------------------ selection and highlight
        public void SelectAt(Vector2 world)
        {
            if (Sim == null) return;
            Cell c = Sim.PickCell(world.x, world.y, 1.25f);
            WorldView.SelectedCellId = c != null ? c.Id : WorldView.NoCell;
        }

        public void ClearSelection() { WorldView.SelectedCellId = WorldView.NoCell; }

        public void ToggleHighlight(int speciesId)
        {
            WorldView.HighlightSpeciesId = WorldView.HighlightSpeciesId == speciesId ? 0 : speciesId;
            ValidateHighlight();
        }

        void ValidateHighlight()
        {
            if (Sim == null || WorldView.HighlightSpeciesId == 0) return;
            for (int i = 0; i < Sim.Cells.AliveCount; i++)
                if (Sim.Cells.Alive[i].SpeciesId == WorldView.HighlightSpeciesId) return;
            WorldView.HighlightSpeciesId = 0;
        }

        // ------------------------------------------------------------ Events
        public void BeginPlacement(EventSpec spec)
        {
            Armed = spec;
            WorldView.PreviewKind = spec.Kind;
            Ui.OnPlacementChanged();
        }

        public void CancelPlacement()
        {
            if (Armed == null) return;
            Armed = null;
            WorldView.PreviewActive = false;
            Ui.OnPlacementChanged();
        }

        public void PlaceArmedAt(Vector2 world)
        {
            if (Armed == null || Sim == null) return;
            if (!WorldContains(world))
            {
                Ui.Toast("status-warning", "Click inside the World boundary to place the Event.");
                return;
            }
            Armed.X = world.x;
            Armed.Y = world.y;
            if (Sim.PlaceEvent(Armed)) CancelPlacement();
            else Ui.Toast("status-warning", "Too many Events are active. The Event is still ready to place.");
        }

        public bool WorldContains(Vector2 point)
        {
            return Sim != null && point.x >= 0f && point.y >= 0f && point.x <= Sim.World.Width && point.y <= Sim.World.Height;
        }

        public void ApplyWholeWorld(EventSpec spec)
        {
            if (Sim == null) return;
            spec.WholeWorld = true;
            if (Sim.PlaceEvent(spec)) CancelPlacement();
        }

        // ------------------------------------------------------------ files
        public void SaveRun()
        {
            if (Sim == null) return;
            RunFiles.DownloadText(RunFiles.RunFileName(Sim), "application/json", RunFiles.ToJson(Sim));
            Ui.Toast("status-good", "Download requested: " + RunFiles.RunFileName(Sim));
        }

        public void ExportCsv()
        {
            if (Sim == null) return;
            RunFiles.ExportCsv(Sim);
            Ui.Toast("status-good", "Two CSV downloads requested. Your browser may ask permission for multiple files.", 8f);
        }

        public void LoadRunFromPicker()
        {
            if (Loading) return;
            int request = ++loadVersion;
            Simulation expected = Sim;
            beforeLoad = Mode;
            Loading = true;
            if (Mode == AppMode.Running) Mode = AppMode.Paused;
            Ui.CloseAll();
            ChartElement.HideReadouts();
            Ui.OnLoadingChanged();
            FileBridge.PickText(".json,application/json", result =>
            {
                if (!Loading || request != loadVersion || Sim != expected) return;
                Loading = false;
                Mode = beforeLoad;
                Ui.OnLoadingChanged();
                if (result.Status == FilePickStatus.Cancelled) return;
                if (result.Status == FilePickStatus.Error)
                {
                    Ui.Toast("status-serious", "This file could not be read. Choose a readable Run file.", 8f);
                    return;
                }
                OnLoadText(result.Text);
            });
        }

        public void CancelLoad()
        {
            loadVersion++;
            FileBridge.CancelPick();
            if (!Loading) return;
            Loading = false;
            Mode = beforeLoad;
            if (Ui != null) Ui.OnLoadingChanged();
        }

        void OnLoadText(string text)
        {
            string error;
            Simulation sim = RunFiles.FromJson(text, out error);
            if (sim == null)
            {
                Ui.Toast("status-serious", error, 8f);
                return;
            }
            GuardRunChange("Replace the current Run?", "Replace", () =>
            {
                Begin(sim, sim.Extinct ? AppMode.Extinct : AppMode.Paused);
                Ui.Toast("status-good", "Run loaded");
            });
        }

        public void TakeScreenshot()
        {
            if (Sim == null) return;
            StartCoroutine(ScreenshotRoutine());
        }

        IEnumerator ScreenshotRoutine()
        {
            Simulation captured = Sim;
            string name = RunFiles.ScreenshotFileName(captured);
            Tooltips.Hide();
            yield return new WaitForEndOfFrame();
            if (Sim != captured) yield break;
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex == null) yield break;
            byte[] png = tex.EncodeToPNG();
            Destroy(tex);
            FileBridge.Download(name, "image/png", png);
            Ui.Toast("status-good", "Download requested: " + name);
        }
    }
}
