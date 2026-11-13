using System;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Liquid Glass popover menu of the five Events, plus the Event configurator modal.</summary>
    public static class EventsMenu
    {
        static VisualElement catcher;

        public static bool IsOpen { get { return catcher != null; } }

        public static void Close()
        {
            if (catcher != null && catcher.parent != null) catcher.parent.Remove(catcher);
            catcher = null;
        }

        public static Color AccentFor(EventKind kind)
        {
            switch (kind)
            {
                case EventKind.HeatWave: return DesignTokens.TempHot;
                case EventKind.IceAge: return DesignTokens.TempCold;
                case EventKind.Famine: return DesignTokens.StatusWarning;
                case EventKind.FoodBloom: return DesignTokens.WorldFood;
                default: return DesignTokens.WorldToxin;
            }
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
            card.style.width = 400;
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Modal);
            card.Pad(DesignTokens.Space2);
            Rect a = new Rect(overlay.WorldToLocal(anchor.worldBound.position), anchor.worldBound.size);
            card.style.left = Mathf.Max(DesignTokens.Space2, a.xMax - 400f);
            card.style.top = a.yMax + 6;
            card.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            catcher.Add(card);

            for (int i = 0; i < EventInfo.Ids.Length; i++)
            {
                EventKind kind = (EventKind)i;
                Color ec = AccentFor(kind);
                VisualElement row = Ui.Row();
                row.style.minHeight = 54;
                row.Pad(DesignTokens.Space2, DesignTokens.Space3);
                row.Round(DesignTokens.Radius);
                row.style.backgroundColor = Color.clear;

                var badge = new VisualElement();
                badge.Size(34, 34);
                badge.Round(10);
                badge.style.backgroundColor = DesignTokens.WithAlpha(ec, 0.18f);
                badge.style.alignItems = Align.Center;
                badge.style.justifyContent = Justify.Center;
                badge.style.marginRight = DesignTokens.Space3;
                badge.style.flexShrink = 0;
                badge.Add(new IconElement(EventInfo.Ids[i], 18f) { Tint = ec });
                row.Add(badge);

                VisualElement text = Ui.Column();
                text.style.flexShrink = 1;
                text.Add(Ui.LabelText(EventInfo.Names[i]));
                text.Add(Ui.Caption(UiText.Body("event-" + EventInfo.Ids[i]), DesignTokens.Text2));
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
    }

    /// <summary>Liquid Glass Event dialog: radius, intensity/duration, Whole World checkbox, Cancel and Place/Apply.</summary>
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
            Color ec = EventsMenu.AccentFor(kind);
            var spec = new EventSpec
            {
                Kind = kind,
                Radius = 20f,
                Intensity = EventInfo.DefaultIntensity(kind),
                Duration = EventInfo.DefaultDuration(kind),
            };

            var dialog = new VisualElement();
            dialog.style.width = DesignTokens.DialogWidth;
            LiquidGlass.Attach(dialog, DesignTokens.RadiusPanel, GlassTone.Modal);
            dialog.Pad(DesignTokens.Space5);
            Add(dialog);

            VisualElement head = Ui.Row();
            var badge = new VisualElement();
            badge.Size(40, 40);
            badge.Round(12);
            badge.style.backgroundColor = DesignTokens.WithAlpha(ec, 0.20f);
            badge.Border(DesignTokens.WithAlpha(ec, 0.45f), 1);
            badge.style.alignItems = Align.Center;
            badge.style.justifyContent = Justify.Center;
            badge.style.marginRight = DesignTokens.Space3;
            badge.Add(new IconElement(EventInfo.Ids[k], 22f) { Tint = ec });
            head.Add(badge);

            Label title = Ui.Heading(EventInfo.Names[k], DesignTokens.Text1);
            title.pickingMode = PickingMode.Position;
            Tooltips.Attach(title, "event-" + EventInfo.Ids[k]);
            head.Add(title);
            head.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "close");
            close.Clicked += Close;
            head.Add(close);
            dialog.Add(head);

            Label desc = Ui.Body(UiText.Body("event-" + EventInfo.Ids[k]), DesignTokens.Text2);
            desc.style.marginTop = DesignTokens.Space3;
            desc.style.marginBottom = DesignTokens.Space4;
            dialog.Add(desc);

            VisualElement well = new VisualElement();
            LiquidGlass.StyleWell(well, DesignTokens.RadiusCard);
            well.Pad(DesignTokens.Space4);
            dialog.Add(well);

            var radius = new EvoSlider("Radius", "event-radius", 5f, 60f, 20f, 1f, v => Mathf.RoundToInt(v) + " units");
            radius.Changed += v => spec.Radius = v;
            well.Add(radius);
            if (kind == EventKind.ToxinSpill)
            {
                Label amount = Ui.Caption("", DesignTokens.Text2);
                amount.style.marginTop = DesignTokens.Space1;
                amount.pickingMode = PickingMode.Position;
                Tooltips.Attach(amount, "event-toxin-amount");
                Action updateAmount = () => amount.text = Mathf.RoundToInt(app.Sim.P.SpillParticlesPer10Radius * spec.Radius / 10f) + " Toxin particles requested (capacity may limit this).";
                radius.Changed += v => updateAmount();
                updateAmount();
                well.Add(amount);
            }

            if (EventInfo.HasIntensity(kind))
            {
                var intensity = new EvoSlider("Intensity", "event-intensity", EventInfo.MinIntensity, EventInfo.MaxIntensity,
                    EventInfo.DefaultIntensity(kind), 0.05f, v => (kind == EventKind.IceAge ? "-" : "+") + Ui.F(v, 2));
                intensity.Changed += v => spec.Intensity = v;
                intensity.style.marginTop = DesignTokens.Space3;
                well.Add(intensity);
            }
            if (EventInfo.HasDuration(kind))
            {
                var duration = new EvoSlider("Duration", "event-duration", EventInfo.MinDuration(kind), EventInfo.MaxDuration(kind),
                    EventInfo.DefaultDuration(kind), 5f, v => Mathf.RoundToInt(v) + " s");
                duration.Changed += v => spec.Duration = v;
                duration.style.marginTop = DesignTokens.Space3;
                well.Add(duration);
            }

            var whole = new EvoCheckbox("Apply to the Whole World at once", "event-whole-world");
            whole.style.marginTop = DesignTokens.Space3;
            dialog.Add(whole);

            var msg = new Message("status-warning", "");
            msg.style.marginTop = DesignTokens.Space3;
            dialog.Add(msg);

            VisualElement buttons = Ui.Row();
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = DesignTokens.Space4;
            var cancel = new EvoButton(ButtonKind.Secondary, "Cancel", null, "event-cancel");
            cancel.Clicked += Close;
            var place = new EvoButton(ButtonKind.Primary, "Choose Spot in World", null, "event-place");
            place.style.marginLeft = DesignTokens.Space2;
            whole.Changed += v =>
            {
                radius.Enabled = !v || kind == EventKind.ToxinSpill;
                place.SetText(v ? "Apply Now" : "Choose Spot in World");
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
