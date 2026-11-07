using System.Collections;
using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    public enum AppMode { Setup, Running, Paused, Extinct }

    /// <summary>
    /// The small state machine that owns the Run: Setup, Running, Paused, Extinct (PLAN 14.5).
    /// The views (World, UI) read this object and call its methods; none of them owns state.
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
        public float EffectiveSpeed { get { return runner.EffectiveSpeed; } }

        public WorldView WorldView { get; private set; }
        public CameraController Cam { get; private set; }
        public UiRoot Ui { get; private set; }
        public InputRouter Input { get; private set; }

        readonly SimulationRunner runner = new SimulationRunner();
        int fitFrames;

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
        }

        void Awake() { Instance = this; }

        public bool HasRun { get { return Sim != null; } }
        public bool Paused { get { return Mode == AppMode.Paused; } }
        public int SpeedMultiplier { get { return SimulationRunner.Speeds[SpeedIndex]; } }

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
            WorldView.TintOn = TintOn;
            if (Sim != null)
            {
                Ui.SyncViewport();
                if (fitFrames > 0) { Cam.Fit(); fitFrames--; }
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
            if (Sim != null) StartRun(Sim.Setup);
        }

        void Begin(Simulation sim, AppMode mode)
        {
            Sim = sim;
            Mode = mode;
            Armed = null;
            runner.Reset();
            WorldView.SetSimulation(sim);
            WorldView.TintOn = TintOn;
            Cam.SetWorld(sim.World.Width, sim.World.Height);
            fitFrames = 4;
            Ui.OnRunStarted();
        }

        public void GoToSetup()
        {
            SetupDraft = Sim != null ? Sim.Setup.Clone() : SetupDraft;
            Sim = null;
            Mode = AppMode.Setup;
            Armed = null;
            WorldView.SetSimulation(null);
            Ui.OnModeChanged();
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

        public void TogglePanel() { PanelOpen = !PanelOpen; fitFrames = 4; Ui.OnPanelToggled(); }

        public void FitView() { Cam.Fit(); }

        // ------------------------------------------------------------ selection and highlight
        public void SelectAt(Vector2 world)
        {
            if (Sim == null) return;
            Cell c = Sim.PickCell(world.x, world.y, 1f);
            WorldView.SelectedCellId = c != null ? c.Id : WorldView.NoCell;
        }

        public void ClearSelection() { WorldView.SelectedCellId = WorldView.NoCell; }

        public void ToggleHighlight(int speciesId)
        {
            WorldView.HighlightSpeciesId = WorldView.HighlightSpeciesId == speciesId ? 0 : speciesId;
        }

        // ------------------------------------------------------------ Events
        public void BeginPlacement(EventSpec spec)
        {
            Armed = spec;
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
            Armed.X = world.x;
            Armed.Y = world.y;
            Sim.PlaceEvent(Armed);
            CancelPlacement();
        }

        public void ApplyWholeWorld(EventSpec spec)
        {
            if (Sim == null) return;
            spec.WholeWorld = true;
            Sim.PlaceEvent(spec);
        }

        // ------------------------------------------------------------ files
        public void SaveRun()
        {
            if (Sim == null) return;
            RunFiles.DownloadText(RunFiles.RunFileName(Sim), "application/json", RunFiles.ToJson(Sim));
            Ui.Toast("status-good", "Run saved as " + RunFiles.RunFileName(Sim));
        }

        public void ExportCsv()
        {
            if (Sim == null) return;
            RunFiles.ExportCsv(Sim);
            Ui.Toast("status-good", "Files exported: traits-over-time.csv and species.csv");
        }

        public void LoadRunFromPicker()
        {
            FileBridge.PickText(".json,application/json", OnLoadText);
        }

        void OnLoadText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            string error;
            Simulation sim = RunFiles.FromJson(text, out error);
            if (sim == null)
            {
                Ui.Toast("status-serious", error, 8f);
                return;
            }
            Begin(sim, sim.Extinct ? AppMode.Extinct : AppMode.Paused);
            Ui.Toast("status-good", "Run loaded");
        }

        public void TakeScreenshot()
        {
            if (Sim == null) return;
            StartCoroutine(ScreenshotRoutine());
        }

        IEnumerator ScreenshotRoutine()
        {
            Tooltips.Hide();
            yield return new WaitForEndOfFrame();
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex == null) yield break;
            byte[] png = tex.EncodeToPNG();
            Destroy(tex);
            string name = RunFiles.ScreenshotFileName(Sim);
            FileBridge.Download(name, "image/png", png);
            Ui.Toast("status-good", "Screenshot saved as " + name);
        }
    }
}
