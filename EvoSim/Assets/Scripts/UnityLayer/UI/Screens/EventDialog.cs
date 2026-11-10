using System;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The menu under the Events button: five Events, each with an icon, a name and a one-line description (DESIGN 6.3).</summary>
    public static class EventsMenu
    {
        static VisualElement catcher;

        public static bool IsOpen { get { return catcher != null; } }

        public static void Close()
        {
            if (catcher != null && catcher.parent != null) catcher.parent.Remove(catcher);
            catcher = null;
        }

        public static void Open(VisualElement anchor, Action<EventKind> chosen)
        {
            if (catcher != null || Ui.Overlay == null) return;
            Tooltips.Hide();
            VisualElement overlay = Ui.Overlay;
            catcher = new VisualElement();
            catcher.Fill();
            catcher.RegisterCallback<PointerDownEvent>(e => { Close(); e.StopPropagation(); });
            overlay.Add(catcher);

            var card = new VisualElement();
            card.style.position = Position.Absolute;
            card.style.width = 420;
            card.style.backgroundColor = DesignTokens.Bg2;
            card.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            card.Round(DesignTokens.Radius);
            Rect a = new Rect(overlay.WorldToLocal(anchor.worldBound.position), anchor.worldBound.size);
            card.style.left = Mathf.Max(DesignTokens.Space2, a.xMax - 420f);
            card.style.top = a.yMax + 2;
            card.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            catcher.Add(card);

            for (int i = 0; i < EventInfo.Ids.Length; i++)
            {
                EventKind kind = (EventKind)i;
                VisualElement row = Ui.Row();
                row.style.minHeight = 56;
                row.Pad(DesignTokens.Space2, DesignTokens.Space3);
                row.style.backgroundColor = Color.clear;
                var icon = new IconElement(EventInfo.Ids[i], 20f) { Tint = DesignTokens.Text2 };
                icon.style.marginRight = DesignTokens.Space3;
                icon.style.flexShrink = 0;
                row.Add(icon);
                VisualElement text = Ui.Column();
                text.style.flexShrink = 1;
                text.Add(Ui.LabelText(EventInfo.Names[i]));
                text.Add(Ui.Caption(Short(UiText.Body("event-" + EventInfo.Ids[i]))));
                row.Add(text);
                row.RegisterCallback<PointerEnterEvent>(e => row.style.backgroundColor = DesignTokens.Bg3);
                row.RegisterCallback<PointerLeaveEvent>(e => row.style.backgroundColor = Color.clear);
                row.RegisterCallback<PointerDownEvent>(e =>
                {
                    Close();
                    if (chosen != null) chosen(kind);
                    e.StopPropagation();
                });
                card.Add(row);
            }
        }

        static string Short(string s) { return s; }
    }

    /// <summary>
    /// Event dialog (DESIGN 4.9, 6.3): radius, intensity and duration where they apply, a Whole World checkbox, Cancel and Place.
    /// Place closes the dialog and arms placement; with Whole World the button reads Apply and acts at once.
    /// </summary>
    public sealed class EventDialog : VisualElement
    {
        readonly AppController app;
        Action refreshState;
        public bool IsOpen { get; private set; }

        public EventDialog(AppController app)
        {
            this.app = app;
            this.Fill();
            style.backgroundColor = DesignTokens.Scrim;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            this.Show(false);
        }

        public void Open(EventKind kind)
        {
            Clear();
            Tooltips.Hide();
            IsOpen = true;
            this.Show(true);
            int k = (int)kind;
            var spec = new EventSpec
            {
                Kind = kind,
                Radius = 20f,
                Intensity = EventInfo.DefaultIntensity(kind),
                Duration = EventInfo.DefaultDuration(kind),
            };

            var dialog = new VisualElement();
            dialog.style.width = DesignTokens.DialogWidth;
            dialog.style.backgroundColor = DesignTokens.Bg1;
            dialog.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            dialog.Round(DesignTokens.Radius);
            dialog.Pad(DesignTokens.Space5);
            Add(dialog);

            VisualElement head = Ui.Row();
            var icon = new IconElement(EventInfo.Ids[k], 24f) { Tint = DesignTokens.AccentText };
            icon.style.marginRight = DesignTokens.Space3;
            head.Add(icon);
            Label title = Ui.Title(EventInfo.Names[k]);
            title.pickingMode = PickingMode.Position;
            Tooltips.Attach(title, "event-" + EventInfo.Ids[k]);
            head.Add(title);
            head.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "close");
            close.Clicked += Close;
            head.Add(close);
            dialog.Add(head);

            Label desc = Ui.Body(UiText.Body("event-" + EventInfo.Ids[k]), DesignTokens.Text2);
            desc.style.marginTop = DesignTokens.Space2;
            desc.style.marginBottom = DesignTokens.Space4;
            dialog.Add(desc);

            var radius = new EvoSlider("Radius", "event-radius", 5f, 60f, 20f, 1f, v => Mathf.RoundToInt(v) + " units");
            radius.Changed += v => spec.Radius = v;
            dialog.Add(radius);
            if (kind == EventKind.ToxinSpill)
            {
                Label amount = Ui.Caption("");
                amount.pickingMode = PickingMode.Position;
                Tooltips.Attach(amount, "event-toxin-amount");
                Action updateAmount = () => amount.text = Mathf.RoundToInt(app.Sim.P.SpillParticlesPer10Radius * spec.Radius / 10f) + " Toxin particles requested (capacity may limit this).";
                radius.Changed += v => updateAmount();
                updateAmount();
                dialog.Add(amount);
            }

            if (EventInfo.HasIntensity(kind))
            {
                var intensity = new EvoSlider("Intensity", "event-intensity", EventInfo.MinIntensity, EventInfo.MaxIntensity,
                    EventInfo.DefaultIntensity(kind), 0.05f, v => (kind == EventKind.IceAge ? "-" : "+") + Ui.F(v, 2));
                intensity.Changed += v => spec.Intensity = v;
                intensity.style.marginTop = DesignTokens.Space4;
                dialog.Add(intensity);
            }
            if (EventInfo.HasDuration(kind))
            {
                var duration = new EvoSlider("Duration", "event-duration", EventInfo.MinDuration(kind), EventInfo.MaxDuration(kind),
                    EventInfo.DefaultDuration(kind), 5f, v => Mathf.RoundToInt(v) + " s");
                duration.Changed += v => spec.Duration = v;
                duration.style.marginTop = DesignTokens.Space4;
                dialog.Add(duration);
            }

            var whole = new EvoCheckbox("Whole World", "event-whole-world");
            whole.style.marginTop = DesignTokens.Space4;
            dialog.Add(whole);

            var msg = new Message("status-warning", "");
            msg.style.marginTop = DesignTokens.Space3;
            dialog.Add(msg);

            VisualElement buttons = Ui.Row();
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = DesignTokens.Space5;
            var cancel = new EvoButton(ButtonKind.Secondary, "Cancel", null, "event-cancel");
            cancel.Clicked += Close;
            var place = new EvoButton(ButtonKind.Primary, "Place", null, "event-place");
            place.style.marginLeft = DesignTokens.Space2;
            whole.Changed += v =>
            {
                radius.Enabled = !v || kind == EventKind.ToxinSpill;
                place.SetText(v ? "Apply" : "Place");
                place.SetTip(v ? "event-apply" : "event-place");
            };
            place.Clicked += () =>
            {
                if (app.Sim == null || !app.Sim.CanPlaceEvent(kind)) { Tick(); return; }
                Close();
                if (whole.Value) app.ApplyWholeWorld(spec);
                else app.BeginPlacement(spec);
            };
            buttons.Add(cancel);
            buttons.Add(place);
            dialog.Add(buttons);
            refreshState = () =>
            {
                bool canPlace = app.Sim != null && app.Sim.CanPlaceEvent(kind);
                place.Disabled = !canPlace;
                msg.Show(!canPlace);
                msg.Q<Label>().text = app.Mode == AppMode.Paused
                    ? "Too many Events are active. Close this dialog and resume the Run so they can finish."
                    : "Too many Events are active. Wait for one to finish.";
            };
            Tick();
        }

        public void Tick() { if (IsOpen && refreshState != null) refreshState(); }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            refreshState = null;
            Tooltips.Hide();
            this.Show(false);
            Clear();
        }
    }
}
