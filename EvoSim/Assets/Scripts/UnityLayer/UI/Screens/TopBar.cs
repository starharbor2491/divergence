using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Floating Liquid Glass top bar: labeled Pause/Resume button, readable 1x–10x speed pills, tabular live telemetry
    /// (Time, Generation, Cells, Species), Thermal map & Fit View controls, file exports, How it works, Charts toggle, and Menu.
    /// </summary>
    public sealed class TopBar : VisualElement
    {
        readonly AppController app;
        readonly UiRoot ui;
        readonly EvoButton play, tint, panelToggle;
        readonly EvoButton[] speeds = new EvoButton[4];
        readonly Label timeValue, generationValue, cellsValue, speciesValue, capNote;
        float sinceReadout = 99f;
        AppMode shownMode = (AppMode)(-1);

        public TopBar(AppController app, UiRoot ui)
        {
            this.app = app;
            this.ui = ui;
            style.height = DesignTokens.TopBarHeight;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            this.Margin(DesignTokens.Space3, DesignTokens.Space3, 0, DesignTokens.Space3);
            LiquidGlass.Attach(this, DesignTokens.RadiusPanel, GlassTone.Pill);
            this.Pad(0, DesignTokens.Space3);

            // Transport: Play/Pause with clear label + 1x/2x/5x/10x segmented pill
            play = new EvoButton(ButtonKind.Secondary, "Pause", "pause", "pause");
            play.style.minWidth = 98;
            play.Round(DesignTokens.RadiusPill);
            play.Clicked += app.TogglePause;
            Add(play);

            var group = Ui.Row();
            group.style.marginLeft = DesignTokens.Space2;
            group.style.height = DesignTokens.ControlHeight;
            group.style.backgroundColor = DesignTokens.GlassInset;
            group.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            group.Round(DesignTokens.Radius);
            group.Pad(2);
            string[] speedLabels = { "1x", "2x", "5x", "10x" };
            string[] speedTips = { "speed-1", "speed-2", "speed-3", "speed-4" };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                speeds[i] = new EvoButton(ButtonKind.Tab, speedLabels[i], null, speedTips[i]);
                speeds[i].style.height = DesignTokens.ControlHeight - 4;
                speeds[i].style.minWidth = 44;
                speeds[i].Pad(0, 10);
                speeds[i].Round(DesignTokens.Radius - 3);
                speeds[i].Clicked += () => app.SetSpeed(index);
                group.Add(speeds[i]);
            }
            Add(group);

            // Live Telemetry Well
            VisualElement readout = Ui.Row();
            readout.style.marginLeft = DesignTokens.Space3;
            readout.style.height = DesignTokens.ControlHeight;
            LiquidGlass.StyleWell(readout, DesignTokens.Radius);
            readout.Pad(0, DesignTokens.Space4);
            readout.pickingMode = PickingMode.Position;
            Tooltips.Attach(readout, "stat-readout");
            timeValue = Metric(readout, "Time", false);
            generationValue = Metric(readout, "Gen", true);
            cellsValue = Metric(readout, "Cells", true);
            speciesValue = Metric(readout, "Species", true);
            capNote = Ui.CaptionBold(UiText.PopulationCap, DesignTokens.StatusWarning);
            capNote.style.marginLeft = DesignTokens.Space3;
            capNote.Show(false);
            readout.Add(capNote);
            Add(readout);

            Add(Ui.Spacer());

            // View tools
            tint = new EvoButton(ButtonKind.Secondary, "Thermal", "temperature", "temperature-tint");
            tint.Clicked += app.ToggleTint;
            Add(tint);

            var fit = new EvoButton(ButtonKind.Icon, null, "fit", "fit-view");
            fit.style.marginLeft = DesignTokens.Space1;
            fit.Clicked += app.FitView;
            Add(fit);

            Add(Divider());

            // File actions
            AddIcon("save", "save", app.SaveRun);
            var load = AddIcon("load", "load", () => { });
            FileBridge.BindPicker(load, app.LoadRunFromPicker, () => !ui.ModalOpen && !ui.WindowTooSmall);
            AddIcon("csv", "csv", app.ExportCsv);
            AddIcon("screenshot", "screenshot", app.TakeScreenshot);

            Add(Divider());

            // Guide, Charts panel toggle, Menu
            var guide = new EvoButton(ButtonKind.Secondary, "Guide", "help", "help");
            guide.Clicked += ui.OpenHelp;
            Add(guide);

            panelToggle = new EvoButton(ButtonKind.Secondary, "Charts", "panel", "panel");
            panelToggle.style.marginLeft = DesignTokens.Space2;
            panelToggle.Clicked += app.TogglePanel;
            Add(panelToggle);

            var home = new EvoButton(ButtonKind.Icon, null, "home", "home");
            home.style.marginLeft = DesignTokens.Space1;
            home.Clicked += app.GoToSetup;
            Add(home);
        }

        VisualElement Divider()
        {
            VisualElement d = Ui.Divider(true, 22);
            d.style.marginLeft = d.style.marginRight = DesignTokens.Space2;
            return d;
        }

        EvoButton AddIcon(string icon, string tip, System.Action click)
        {
            var b = new EvoButton(ButtonKind.Icon, null, icon, tip);
            b.Clicked += click;
            Add(b);
            return b;
        }

        static Label Metric(VisualElement parent, string name, bool gap)
        {
            Label n = Ui.Caption(name, DesignTokens.Text3);
            if (gap) n.style.marginLeft = DesignTokens.Space4;
            parent.Add(n);
            Label v = Ui.Mono("", DesignTokens.Text1, DesignTokens.TypeLabelSize);
            v.style.marginLeft = 6;
            parent.Add(v);
            return v;
        }

        public void Tick(float realDt)
        {
            Simulation sim = app.Sim;
            if (sim == null) return;

            bool running = app.Mode == AppMode.Running;
            if (shownMode != app.Mode)
            {
                shownMode = app.Mode;
                play.Icon.Icon = running ? "pause" : "play";
                play.SetText(running ? "Pause" : "Resume");
                play.SetTip(running ? "pause" : "play");
                play.Toggled = !running && app.Mode == AppMode.Paused;
                play.Disabled = app.Mode == AppMode.Extinct;
            }
            for (int i = 0; i < 4; i++) speeds[i].Selected = i == app.SpeedIndex;
            tint.Toggled = app.TintOn;
            panelToggle.Toggled = app.PanelOpen;
            panelToggle.Icon.On = app.PanelOpen;

            sinceReadout += realDt;
            if (sinceReadout < 0.25f) return;
            sinceReadout = 0f;
            CellPool pool = sim.Cells;
            float gen = 0f;
            for (int i = 0; i < pool.AliveCount; i++) gen += pool.Alive[i].Generation;
            if (pool.AliveCount > 0) gen /= pool.AliveCount;
            timeValue.text = Ui.Mmss(sim.Time);
            generationValue.text = Ui.F(gen, 1);
            cellsValue.text = Ui.Count(pool.AliveCount);
            speciesValue.text = Ui.Count(sim.Species.LivingCount);
            capNote.Show(pool.AliveCount >= sim.P.MaxCells);
        }
    }
}
