using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class UiRoot
    {
        readonly AppController app;
        readonly VisualElement root;
        readonly SetupScreen setup;
        readonly VisualElement runLayer, worldRegion, toastHost, placementCard, legend, eventDockWrap, fieldNotesCard, notesHistory;
        readonly TopBar topBar;
        readonly SidePanel sidePanel;
        readonly CellInspector inspector;
        readonly EventDialog eventDialog;
        readonly HelpScreen help;
        readonly ExtinctionOverlay extinction;
        readonly RunActionDialog confirm = new RunActionDialog();
        readonly VisualElement loading;
        readonly VisualElement tooSmall;
        readonly Label placementText, latestNoteTitle, latestNoteBody, latestNoteTime;
        readonly VisualElement latestNoteDot;
        readonly IconElement placementIcon, notesChevron;
        bool notesExpanded;
        int shownNoteVersion = -1;
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

            fieldNotesCard = BuildFieldNotes(out latestNoteDot, out latestNoteTitle, out latestNoteTime, out latestNoteBody, out notesHistory, out notesChevron);
            worldRegion.Add(fieldNotesCard);

            placementCard = Ui.Row();
            placementCard.style.position = Position.Absolute;
            placementCard.style.top = 10;
            placementCard.style.left = 0;
            placementCard.style.right = 0;
            placementCard.style.justifyContent = Justify.Center;
            placementCard.pickingMode = PickingMode.Ignore;
            VisualElement pc = Ui.Row();
            pc.pickingMode = PickingMode.Position;
            LiquidGlass.Attach(pc, DesignTokens.RadiusCard, GlassTone.Pill);
            pc.Pad(DesignTokens.Space2, DesignTokens.Space4);
            placementIcon = new IconElement("event", 18f) { Tint = DesignTokens.Text1 };
            placementIcon.style.marginRight = DesignTokens.Space2;
            pc.Add(placementIcon);
            placementText = Ui.LabelText("", DesignTokens.Text1);
            pc.Add(placementText);
            var cancelArm = new EvoButton(ButtonKind.Secondary, "Cancel", null, "event-cancel");
            cancelArm.style.height = 28;
            cancelArm.Pad(0, 10);
            cancelArm.style.marginLeft = DesignTokens.Space3;
            cancelArm.Clicked += app.CancelPlacement;
            pc.Add(cancelArm);
            placementCard.Add(pc);
            placementCard.Show(false);
            worldRegion.Add(placementCard);

            legend = BuildTemperatureLegend();
            worldRegion.Add(legend);

            eventDockWrap = BuildEventDock();
            worldRegion.Add(eventDockWrap);

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
            LiquidGlass.Attach(loadingCard, DesignTokens.RadiusPanel, GlassTone.Modal);
            loadingCard.Pad(DesignTokens.Space5);
            loadingCard.Add(Ui.Body("Loading snapshot. Select a JSON file, or cancel."));
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

        VisualElement BuildEventDock()
        {
            VisualElement wrap = Ui.Row();
            wrap.style.position = Position.Absolute;
            wrap.style.left = 0;
            wrap.style.right = 0;
            wrap.style.bottom = 12;
            wrap.style.justifyContent = Justify.Center;
            wrap.pickingMode = PickingMode.Ignore;

            VisualElement dock = Ui.Row();
            dock.pickingMode = PickingMode.Position;
            LiquidGlass.Attach(dock, DesignTokens.RadiusPanel, GlassTone.Pill);
            dock.Pad(5, 8);

            Label prompt = Ui.Caption("Events", DesignTokens.Text3);
            prompt.style.marginLeft = DesignTokens.Space2;
            prompt.style.marginRight = 10;
            prompt.pickingMode = PickingMode.Position;
            Tooltips.Attach(prompt, "events");
            dock.Add(prompt);

            for (int i = 0; i < EventInfo.Ids.Length; i++)
            {
                EventKind kind = (EventKind)i;
                var btn = new EvoButton(ButtonKind.Secondary, EventInfo.Names[i], EventInfo.Ids[i], "event-" + EventInfo.Ids[i]);
                btn.style.height = 34;
                btn.Pad(0, 11);
                btn.AccentOverride = EventsMenu.AccentFor(kind);
                if (i > 0) btn.style.marginLeft = 5;
                btn.Clicked += () => OpenEventDialog(kind);
                dock.Add(btn);
            }

            wrap.Add(dock);
            return wrap;
        }

        VisualElement BuildFieldNotes(
            out VisualElement dot,
            out Label title,
            out Label time,
            out Label body,
            out VisualElement historyBox,
            out IconElement chevron)
        {
            VisualElement card = Ui.Column();
            card.style.position = Position.Absolute;
            card.style.right = 14;
            card.style.top = 10;
            card.style.width = 312;
            card.pickingMode = PickingMode.Position;
            LiquidGlass.Attach(card, DesignTokens.RadiusCard, GlassTone.Pill);
            card.Pad(9, 12);

            VisualElement header = Ui.Row();
            Tooltips.Attach(header, "field-notes");
            dot = new VisualElement();
            dot.Size(7, 7);
            dot.Round(4);
            dot.style.backgroundColor = DesignTokens.AccentText;
            dot.style.marginRight = 7;
            dot.style.flexShrink = 0;
            dot.pickingMode = PickingMode.Ignore;
            header.Add(dot);

            title = Ui.CaptionBold("Field log", DesignTokens.Text1);
            title.style.flexGrow = 1;
            header.Add(title);

            time = Ui.MonoCaption("00:00", DesignTokens.Text3);
            time.style.marginRight = 6;
            header.Add(time);

            chevron = new IconElement("chevron-down", 15f) { Tint = DesignTokens.Text3 };
            header.Add(chevron);
            card.Add(header);

            body = Ui.Caption("", DesignTokens.Text2);
            body.style.marginTop = 3;
            card.Add(body);

            historyBox = Ui.Column();
            historyBox.style.marginTop = DesignTokens.Space2;
            historyBox.Show(false);
            card.Add(historyBox);

            card.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button != 0) return;
                notesExpanded = !notesExpanded;
                shownNoteVersion = -1;
                RefreshFieldNotes();
                e.StopPropagation();
            });

            return card;
        }

        void RefreshFieldNotes()
        {
            var notes = app.Narrator.Notes;
            bool blockingModal = help.IsOpen || eventDialog.IsOpen || confirm.IsOpen || app.Mode == AppMode.Extinct;
            if (notes.Count == 0 || blockingModal || app.Armed != null)
            {
                fieldNotesCard.Show(false);
                return;
            }
            fieldNotesCard.Show(true);
            if (shownNoteVersion == app.Narrator.Version) return;
            shownNoteVersion = app.Narrator.Version;

            FieldNote latest = notes[0];
            latestNoteDot.style.backgroundColor = latest.Dot;
            latestNoteTitle.text = latest.Title;
            latestNoteTime.text = Ui.Mmss(latest.Time);
            latestNoteBody.text = latest.Body;

            bool hasMore = notes.Count > 1 && notesExpanded;
            notesHistory.Show(hasMore);
            notesChevron.Show(notes.Count > 1);
            notesHistory.Clear();
            if (hasMore)
            {
                int limit = Mathf.Min(notes.Count, 4);
                for (int i = 1; i < limit; i++)
                {
                    FieldNote n = notes[i];
                    VisualElement div = Ui.Divider(false);
                    div.style.marginTop = 7;
                    div.style.marginBottom = 7;
                    notesHistory.Add(div);

                    VisualElement r = Ui.Row();
                    var d = new VisualElement();
                    d.Size(6, 6);
                    d.Round(3);
                    d.style.backgroundColor = n.Dot;
                    d.style.marginRight = 6;
                    r.Add(d);
                    Label nt = Ui.CaptionBold(n.Title, DesignTokens.Text2);
                    nt.style.flexGrow = 1;
                    r.Add(nt);
                    r.Add(Ui.MonoCaption(Ui.Mmss(n.Time), DesignTokens.Text3));
                    notesHistory.Add(r);

                    Label nb = Ui.Caption(n.Body, DesignTokens.Text3);
                    nb.style.marginTop = 2;
                    notesHistory.Add(nb);
                }
            }
        }

        VisualElement BuildTemperatureLegend()
        {
            VisualElement box = Ui.Column();
            box.style.position = Position.Absolute;
            box.style.left = 12;
            box.style.bottom = 12;
            box.style.width = 160;
            box.pickingMode = PickingMode.Position;
            LiquidGlass.Attach(box, DesignTokens.RadiusCard, GlassTone.Pill);
            box.Pad(7, 11);
            Tooltips.Attach(box, "temperature-legend");

            var bar = new VisualElement();
            bar.style.height = 6;
            bar.Round(3);
            bar.style.overflow = Overflow.Hidden;
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
            labels.style.marginTop = 3;
            labels.pickingMode = PickingMode.Ignore;
            labels.Add(Ui.Caption("Cold", DesignTokens.Text2));
            labels.Add(Ui.Spacer());
            labels.Add(Ui.Caption("Warm", DesignTokens.Text2));
            box.Add(labels);
            box.Show(false);
            return box;
        }

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
            notesExpanded = false;
            shownNoteVersion = -1;
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
            if (a == null)
            {
                placementCard.Show(false);
                RefreshFieldNotes();
                return;
            }
            int k = (int)a.Kind;
            placementIcon.Icon = EventInfo.Ids[k];
            placementIcon.Tint = EventsMenu.AccentFor(a.Kind);
            placementText.text = "Click in the water to place " + EventInfo.Names[k].ToLowerInvariant() + " (Esc cancels)";
            placementCard.Show(true);
            fieldNotesCard.Show(false);
        }

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

        public void Toast(string status, string text, float seconds = 4f)
        {
            if (toast != null) toast.RemoveFromHierarchy();
            toast = new Message(status, text);
            toast.style.maxWidth = 720;
            toastHost.Add(toast);
            toastUntil = Time.unscaledTime + seconds;
        }

        public bool IsOverUi(Vector2 screen)
        {
            if (root.panel == null) return false;
            Vector2 p = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screen.x, Screen.height - screen.y));
            VisualElement hit = root.panel.Pick(p);
            return hit != null && hit != root && hit != worldRegion;
        }

        public void SyncViewport()
        {
            Rect pb = root.layout;
            Rect wb = worldRegion.worldBound;
            if (pb.width < 10f || pb.height < 10f || wb.width < 10f || float.IsNaN(wb.width)) return;
            float topReserve = 22f;
            float dockReserve = 52f;
            float stageY = wb.yMin + topReserve;
            float stageH = Mathf.Max(60f, wb.height - topReserve - dockReserve);
            var r = new Rect(wb.x / pb.width, 1f - (stageY + stageH) / pb.height, wb.width / pb.width, stageH / pb.height);
            if (r == lastViewport) return;
            lastViewport = r;
            app.Cam.SetViewport(r);
        }

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

            bool dimBehindModal = help.IsOpen || eventDialog.IsOpen || confirm.IsOpen || app.Mode == AppMode.Extinct;
            runLayer.style.opacity = dimBehindModal ? 0.30f : 1f;

            legend.Show(app.TintOn);
            eventDockWrap.Show(app.Mode != AppMode.Extinct);
            RefreshFieldNotes();
            topBar.Tick(realDt);
            sidePanel.Tick(realDt);
            inspector.Tick(realDt);
        }
    }
}
