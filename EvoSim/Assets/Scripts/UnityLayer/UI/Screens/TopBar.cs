using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// The 56 px top bar of the Run screen (DESIGN 6.2): transport and speeds, the readout, then Events, Temperature tint,
    /// file actions, Help, panel toggle and Menu.
    /// </summary>
    public sealed class TopBar : VisualElement
    {
        readonly AppController app;
        readonly UiRoot ui;
        readonly EvoButton play, events, tint, panelToggle;
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
            style.backgroundColor = DesignTokens.Bg1;
            style.borderBottomWidth = DesignTokens.BorderWidth;
            style.borderBottomColor = DesignTokens.Border;
            this.Pad(0, DesignTokens.Space2);

            play = new EvoButton(ButtonKind.Icon, null, "pause", "pause");
            play.Clicked += app.TogglePause;
            Add(play);

            var group = Ui.Row();
            group.style.marginLeft = DesignTokens.Space2;
            group.style.height = DesignTokens.ControlHeight;
            group.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            group.Round(DesignTokens.Radius);
            string[] icons = { "speed-1", "speed-2", "speed-3", "speed-4" };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                speeds[i] = new EvoButton(ButtonKind.Icon, null, icons[i], icons[i]);
                speeds[i].style.height = DesignTokens.ControlHeight - 2;
                speeds[i].RoundEnds(i == 0, i == 3);
                speeds[i].Clicked += () => app.SetSpeed(index);
                group.Add(speeds[i]);
            }
            Add(group);

            VisualElement readout = Ui.Row();
            readout.style.marginLeft = DesignTokens.Space5;
            readout.pickingMode = PickingMode.Position;
            Tooltips.Attach(readout, "stat-readout");
            timeValue = Pair(readout, "time", false);
            generationValue = Pair(readout, "generation", true);
            cellsValue = Pair(readout, "cells", true);
            speciesValue = Pair(readout, "species", true);
            capNote = Ui.Caption(UiText.PopulationCap);
            capNote.style.marginLeft = DesignTokens.Space5;
            capNote.Show(false);
            readout.Add(capNote);
            Add(readout);

            Add(Ui.Spacer());

            events = new EvoButton(ButtonKind.Icon, null, "event", "events");
            events.style.width = 56;
            var chevron = new IconElement("chevron-down", 16f) { Tint = DesignTokens.Text2 };
            events.Add(chevron);
            events.Clicked += () => EventsMenu.Open(events, kind => ui.OpenEventDialog(kind));
            Add(events);

            tint = new EvoButton(ButtonKind.Icon, null, "temperature", "temperature-tint");
            tint.Clicked += app.ToggleTint;
            Add(tint);

            Add(Divider());
            AddIcon("save", "save", app.SaveRun);
            var load = AddIcon("load", "load", () => { });
            FileBridge.BindPicker(load, app.LoadRunFromPicker, () => !ui.ModalOpen && !ui.WindowTooSmall);
            AddIcon("csv", "csv", app.ExportCsv);
            AddIcon("screenshot", "screenshot", app.TakeScreenshot);
            Add(Divider());

            AddIcon("help", "help", ui.OpenHelp);
            panelToggle = AddIcon("panel", "panel", app.TogglePanel);
            AddIcon("home", "home", app.GoToSetup);
        }

        VisualElement Divider()
        {
            VisualElement d = Ui.Divider(true, 24);
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

        Label Pair(VisualElement parent, string name, bool gap)
        {
            Label n = Ui.Caption(name);
            if (gap) n.style.marginLeft = DesignTokens.Space5;
            parent.Add(n);
            Label v = Ui.Body("");
            v.style.marginLeft = DesignTokens.Space2;
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
                play.SetTip(running ? "pause" : "play");
                play.Disabled = app.Mode == AppMode.Extinct;
            }
            for (int i = 0; i < 4; i++) speeds[i].Selected = i == app.SpeedIndex;
            tint.Toggled = app.TintOn;
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
