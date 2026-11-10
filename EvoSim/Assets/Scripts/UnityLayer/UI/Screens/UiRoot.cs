using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Builds every screen from code and owns the layers: setup, run (top bar, World region, side panel), toast, dialogs,
    /// help, extinction, window-too-small and the overlay for tooltips and popups (DESIGN 3, 9).
    /// </summary>
    public sealed class UiRoot
    {
        readonly AppController app;
        readonly VisualElement root;
        readonly SetupScreen setup;
        readonly VisualElement runLayer, worldRegion, toastHost, placementCard, legend;
        readonly TopBar topBar;
        readonly SidePanel sidePanel;
        readonly CellInspector inspector;
        readonly EventDialog eventDialog;
        readonly HelpScreen help;
        readonly ExtinctionOverlay extinction;
        readonly RunActionDialog confirm = new RunActionDialog();
        readonly VisualElement loading;
        readonly VisualElement tooSmall;
        readonly Label placementText;
        readonly IconElement placementIcon;
        Message toast;
        float toastUntil;
        Rect lastViewport;

        public VisualElement WorldRegion { get { return worldRegion; } }
        public bool WindowTooSmall { get; private set; }
        public bool ModalOpen { get { return confirm.IsOpen || app.Loading || EvoDropdown.IsOpen || eventDialog.IsOpen || help.IsOpen || EventsMenu.IsOpen || app.Mode == AppMode.Extinct; } }

        public UiRoot(AppController app, VisualElement docRoot)
        {
            this.app = app;
            root = docRoot;
            root.style.flexGrow = 1;
            root.pickingMode = PickingMode.Ignore;
            root.style.backgroundColor = Color.clear;

            Ui.Overlay = new VisualElement();
            Ui.Overlay.Fill();
            Ui.Overlay.pickingMode = PickingMode.Ignore;

            // Run layer: top bar over [World region | side panel]
            runLayer = new VisualElement();
            runLayer.Fill();
            runLayer.pickingMode = PickingMode.Ignore;
            runLayer.style.flexDirection = FlexDirection.Column;
            topBar = new TopBar(app, this);
            runLayer.Add(topBar);
            VisualElement body = Ui.Row(Align.Stretch);
            body.style.flexGrow = 1;
            body.pickingMode = PickingMode.Ignore;
            worldRegion = new VisualElement();
            worldRegion.style.flexGrow = 1;
            worldRegion.pickingMode = PickingMode.Ignore;
            body.Add(worldRegion);
            sidePanel = new SidePanel(app);
            body.Add(sidePanel);
            runLayer.Add(body);

            inspector = new CellInspector(app);
            worldRegion.Add(inspector);

            placementCard = Ui.Row();
            placementCard.style.position = Position.Absolute;
            placementCard.style.top = DesignTokens.Space4;
            placementCard.style.alignSelf = Align.Center;
            placementCard.style.left = 0;
            placementCard.style.right = 0;
            placementCard.style.justifyContent = Justify.Center;
            placementCard.pickingMode = PickingMode.Ignore;
            VisualElement pc = Ui.Row();
            pc.style.backgroundColor = DesignTokens.Bg2;
            pc.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            pc.style.borderLeftWidth = 3;
            pc.style.borderLeftColor = DesignTokens.Accent;
            pc.Round(DesignTokens.Radius);
            pc.Pad(DesignTokens.Space3, DesignTokens.Space4);
            placementIcon = new IconElement("event", 20f) { Tint = DesignTokens.AccentText };
            placementIcon.style.marginRight = DesignTokens.Space3;
            pc.Add(placementIcon);
            placementText = Ui.Body("");
            pc.Add(placementText);
            placementCard.Add(pc);
            placementCard.Show(false);
            worldRegion.Add(placementCard);

            legend = BuildTemperatureLegend();
            worldRegion.Add(legend);

            setup = new SetupScreen(app, this);
            eventDialog = new EventDialog(app);
            help = new HelpScreen();
            extinction = new ExtinctionOverlay(app);

            toastHost = Ui.Column();
            toastHost.style.position = Position.Absolute;
            toastHost.style.left = 0;
            toastHost.style.right = 0;
            toastHost.style.top = DesignTokens.TopBarHeight + DesignTokens.Space4;
            toastHost.style.alignItems = Align.Center;
            toastHost.pickingMode = PickingMode.Ignore;

            tooSmall = new VisualElement();
            tooSmall.Fill();
            tooSmall.style.backgroundColor = DesignTokens.Bg0;
            tooSmall.style.alignItems = Align.Center;
            tooSmall.style.justifyContent = Justify.Center;
            var tsMessage = new Message("status-warning", UiText.WindowTooSmall);
            tsMessage.style.maxWidth = 640;
            tooSmall.Add(tsMessage);
            tooSmall.Show(false);
            loading = new VisualElement();
            loading.Fill();
            loading.style.backgroundColor = DesignTokens.Scrim;
            loading.style.alignItems = Align.Center;
            loading.style.justifyContent = Justify.Center;
            var loadingCard = Ui.Column();
            loadingCard.Pad(DesignTokens.Space5);
            loadingCard.style.backgroundColor = DesignTokens.Bg1;
            loadingCard.Add(Ui.Body("Loading Run. Choose a file, or cancel."));
            var cancelLoad = new EvoButton(ButtonKind.Secondary, "Cancel", null, "confirm-cancel");
            cancelLoad.style.marginTop = DesignTokens.Space4;
            cancelLoad.Clicked += app.CancelLoad;
            loadingCard.Add(cancelLoad);
            loading.Add(loadingCard);
            loading.Show(false);

            root.Add(setup);
            root.Add(runLayer);
            root.Add(toastHost);
            root.Add(eventDialog);
            root.Add(help);
            root.Add(extinction);
            root.Add(confirm);
            root.Add(loading);
            root.Add(tooSmall);
            root.Add(Ui.Overlay);
            Tooltips.Init(Ui.Overlay);

            ShowSetup();
        }

        VisualElement BuildTemperatureLegend()
        {
            VisualElement box = Ui.Column();
            box.style.position = Position.Absolute;
            box.style.right = DesignTokens.Space2;
            box.style.bottom = DesignTokens.Space2;
            box.style.width = 160;
            box.pickingMode = PickingMode.Position;
            Tooltips.Attach(box, "temperature-legend");
            var bar = new VisualElement();
            bar.style.height = 8;
            bar.pickingMode = PickingMode.Ignore;
            bar.generateVisualContent += ctx =>
            {
                Painter2D p = ctx.painter2D;
                const int slices = 40;
                float w = bar.contentRect.width, h = bar.contentRect.height;
                for (int i = 0; i < slices; i++)
                {
                    Color32 c = TemperatureColors.At(i / (float)(slices - 1));
                    p.fillColor = c;
                    p.BeginPath();
                    p.MoveTo(new Vector2(w * i / slices, 0));
                    p.LineTo(new Vector2(w * (i + 1) / slices + 0.5f, 0));
                    p.LineTo(new Vector2(w * (i + 1) / slices + 0.5f, h));
                    p.LineTo(new Vector2(w * i / slices, h));
                    p.ClosePath();
                    p.Fill();
                }
            };
            box.Add(bar);
            VisualElement labels = Ui.Row();
            labels.pickingMode = PickingMode.Ignore;
            labels.Add(Ui.Caption("cold"));
            labels.Add(Ui.Spacer());
            labels.Add(Ui.Caption("hot"));
            box.Add(labels);
            box.Show(false);
            return box;
        }

        // ------------------------------------------------------------ mode changes
        void ShowSetup()
        {
            CloseAll();
            setup.LoadFromDraft();
            setup.Show(true);
            runLayer.Show(false);
            extinction.Close();
            eventDialog.Close();
        }

        public void OnRunStarted()
        {
            CloseAll();
            setup.Show(false);
            runLayer.Show(true);
            sidePanel.BindRun(app.Sim);
            sidePanel.Select(0);
            sidePanel.Show(app.PanelOpen);
            eventDialog.Close();
            extinction.Close();
            OnPlacementChanged();
            OnModeChanged();
        }

        public void OnModeChanged()
        {
            ChartElement.HideReadouts();
            if (app.Mode == AppMode.Setup) { ShowSetup(); return; }
            setup.Show(false);
            runLayer.Show(true);
            if (app.Mode == AppMode.Extinct)
            {
                EvoDropdown.CloseOpen();
                EventsMenu.Close();
                Tooltips.Hide();
                extinction.Open(app.Sim);
            }
            else extinction.Close();
        }

        public void OnPanelToggled() { EvoDropdown.CloseOpen(); ChartElement.HideReadouts(); sidePanel.Show(app.PanelOpen); }

        public void OnPlacementChanged()
        {
            EventSpec a = app.Armed;
            if (a == null) { placementCard.Show(false); return; }
            int k = (int)a.Kind;
            placementIcon.Icon = EventInfo.Ids[k];
            placementText.text = "Click the World to place " + EventInfo.Names[k].ToLowerInvariant() + ". Press Escape to cancel.";
            placementCard.Show(true);
        }

        // ------------------------------------------------------------ dialogs
        public void OpenHelp() { CloseAll(); ChartElement.HideReadouts(); help.Open(); }

        public void SelectTab(int index) { sidePanel.Select(index); }

        public void CloseAll()
        {
            EvoDropdown.CloseOpen();
            EventsMenu.Close();
            eventDialog.Close();
            help.Close();
            confirm.Close();
        }

        public void OpenEventDialog(EventKind kind)
        {
            if (app.Sim == null) return;
            eventDialog.Open(kind);
            EvoDropdown.CloseOpen();
            ChartElement.HideReadouts();
        }

        /// <summary>Escape closes the topmost popup. Returns true when something was closed.</summary>
        public bool HandleEscape()
        {
            if (app.Loading) { app.CancelLoad(); return true; }
            if (confirm.IsOpen) { confirm.Close(); return true; }
            if (EvoDropdown.CloseOpen()) return true;
            if (EventsMenu.IsOpen) { EventsMenu.Close(); return true; }
            if (eventDialog.IsOpen) { eventDialog.Close(); return true; }
            if (help.IsOpen) { help.Close(); return true; }
            return false;
        }

        public void ConfirmRunAction(string title, string action, System.Action discard, System.Action cancelled)
        {
            CloseAll();
            ChartElement.HideReadouts();
            confirm.Open(title, action, discard, app.SaveRun, cancelled);
        }

        public void OnLoadingChanged() { loading.Show(app.Loading); }

        // ------------------------------------------------------------ toast
        public void Toast(string status, string text, float seconds = 4f)
        {
            if (toast != null) toast.RemoveFromHierarchy();
            toast = new Message(status, text);
            toast.style.maxWidth = 760;
            toastHost.Add(toast);
            toastUntil = Time.unscaledTime + seconds;
        }

        // ------------------------------------------------------------ pointer
        /// <summary>True when the pointer is over a UI element that takes pointer events (not the empty World region).</summary>
        public bool IsOverUi(Vector2 screen)
        {
            if (root.panel == null) return false;
            Vector2 p = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screen.x, Screen.height - screen.y));
            VisualElement hit = root.panel.Pick(p);
            return hit != null && hit != root && hit != worldRegion;
        }

        /// <summary>Gives the camera the part of the window that the World region covers.</summary>
        public void SyncViewport()
        {
            Rect pb = root.layout;
            Rect wb = worldRegion.worldBound;
            if (pb.width < 10f || pb.height < 10f || wb.width < 10f || float.IsNaN(wb.width)) return;
            var r = new Rect(wb.x / pb.width, 1f - wb.yMax / pb.height, wb.width / pb.width, wb.height / pb.height);
            if (r == lastViewport) return;
            lastViewport = r;
            app.Cam.SetViewport(r);
        }

        // ------------------------------------------------------------ frame
        public void Tick(float realDt)
        {
            float ratio = FileBridge.DevicePixelRatio;
            bool small = Screen.width / ratio < DesignTokens.MinWindowWidth || Screen.height / ratio < DesignTokens.MinWindowHeight;
            if (small != WindowTooSmall)
            {
                WindowTooSmall = small;
                tooSmall.Show(small);
                if (small) { EvoDropdown.CloseOpen(); EventsMenu.Close(); Tooltips.Hide(); ChartElement.HideReadouts(); }
            }
            eventDialog.Tick();
            sidePanel.SetWidthFor(root.layout.width);
            if (toast != null && Time.unscaledTime > toastUntil) { toast.RemoveFromHierarchy(); toast = null; }
            if (app.Mode == AppMode.Setup) return;
            legend.Show(app.TintOn);
            topBar.Tick(realDt);
            sidePanel.Tick(realDt);
            inspector.Tick(realDt);
        }
    }
}
