using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class TooltipCard : VisualElement
    {
        readonly VisualElement body = new VisualElement();
        readonly ScrollView scroll = UiScroll.Create();
        readonly bool interactive;

        public TooltipCard(bool interactive = false)
        {
            this.interactive = interactive;
            pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;
            style.position = Position.Absolute;
            LiquidGlass.Attach(this, DesignTokens.RadiusCard, GlassTone.Modal);
            this.Pad(10, 12);
            style.maxWidth = DesignTokens.TooltipMaxWidth;
            style.visibility = Visibility.Hidden;
            body.pickingMode = PickingMode.Ignore;
            scroll.pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 0;
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.Add(body);
            Add(scroll);
            if (!interactive) scroll.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
        }

        public void SetTip(TipContent tip)
        {
            body.Clear();
            scroll.scrollOffset = Vector2.zero;
            body.Add(Ui.Subhead(tip.Name, DesignTokens.Text1));
            if (!string.IsNullOrEmpty(tip.Body))
            {
                Label l = Ui.Caption(tip.Body, DesignTokens.Text2);
                l.style.marginTop = DesignTokens.Space1;
                body.Add(l);
            }
            if (!string.IsNullOrEmpty(tip.Key))
            {
                Label l = Ui.MonoCaption(tip.Key, DesignTokens.Text3);
                l.style.marginTop = 6;
                body.Add(l);
            }
        }

        public void SetRows(string title, List<ReadoutRow> rows)
        {
            body.Clear();
            scroll.scrollOffset = Vector2.zero;
            body.Add(Ui.CaptionBold(title, DesignTokens.Text1));
            foreach (ReadoutRow r in rows)
            {
                VisualElement row = Ui.Row();
                row.pickingMode = PickingMode.Ignore;
                row.style.marginTop = 3;
                if (r.Swatch.HasValue)
                {
                    var sw = new VisualElement();
                    sw.pickingMode = PickingMode.Ignore;
                    sw.Size(8, 8);
                    sw.Round(4);
                    sw.style.backgroundColor = r.Swatch.Value;
                    sw.style.marginRight = DesignTokens.Space2;
                    sw.style.flexShrink = 0;
                    row.Add(sw);
                }
                Label name = Ui.Caption(r.Name, DesignTokens.Text2);
                name.style.flexGrow = 1;
                row.Add(name);
                Label val = Ui.MonoCaption(r.Value, DesignTokens.Text1);
                val.style.marginLeft = DesignTokens.Space4;
                val.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(val);
                body.Add(row);
            }
        }

        public void PlaceNear(Vector2 point, Rect? confine = null)
        {
            style.left = point.x + 14;
            style.top = point.y + 18;
            style.visibility = Visibility.Hidden;
            if (parent != null)
            {
                float boundH = confine.HasValue && confine.Value.height > 80f ? confine.Value.height - 8f : Mathf.Min(280f, parent.layout.height - DesignTokens.Space4);
                float available = Mathf.Max(DesignTokens.ControlHeight, boundH);
                style.maxHeight = available;
                scroll.style.maxHeight = available - 20f;
                style.maxWidth = Mathf.Min(DesignTokens.TooltipMaxWidth, parent.layout.width - DesignTokens.Space2);
            }
            RegisterCallbackOnce<GeometryChangedEvent>(e => Reposition(point, confine));
            schedule.Execute(() => Reposition(point, confine)).StartingIn(0);
        }

        void Reposition(Vector2 point, Rect? confine)
        {
            if (parent == null) return;
            float pw = parent.layout.width, ph = parent.layout.height;
            float w = layout.width, h = layout.height;
            if (float.IsNaN(w) || w <= 0f) return;

            if (confine.HasValue && confine.Value.width > 40f && confine.Value.height > 40f)
            {
                Rect c = confine.Value;
                float cx = point.x - 14 - w;
                if (cx < c.xMin + 4f) cx = point.x + 14;
                cx = Mathf.Clamp(cx, Mathf.Max(DesignTokens.Space1, c.xMin + 4f), Mathf.Min(pw - w - DesignTokens.Space1, Mathf.Max(c.xMin + 4f, c.xMax - w - 4f)));
                float cy = Mathf.Clamp(point.y - h * 0.5f, Mathf.Max(DesignTokens.Space1, c.yMin + 4f), Mathf.Min(ph - h - DesignTokens.Space1, Mathf.Max(c.yMin + 4f, c.yMax - h - 4f)));
                style.left = cx;
                style.top = cy;
                style.visibility = Visibility.Visible;
                return;
            }

            float x = point.x > pw * 0.62f ? point.x - 16 - w : point.x + 14;
            float y = point.y + 16;
            if (x < DesignTokens.Space2) x = point.x + 14;
            if (x + w > pw - 6) x = pw - w - 6;
            if (y + h > ph - 6) y = point.y - 14 - h;
            style.left = Mathf.Clamp(x, DesignTokens.Space1, Mathf.Max(DesignTokens.Space1, pw - w - DesignTokens.Space1));
            style.top = Mathf.Clamp(y, DesignTokens.Space1, Mathf.Max(DesignTokens.Space1, ph - h - DesignTokens.Space1));
            style.visibility = Visibility.Visible;
        }
    }

    public struct ReadoutRow
    {
        public Color? Swatch;
        public string Name;
        public string Value;
    }

    public static class Tooltips
    {
        public static readonly HashSet<string> AttachedIds = new HashSet<string>();

        static TooltipCard card;
        static TooltipCard readout;
        static IVisualElementScheduledItem pending;
        static VisualElement owner;
        static bool interactive;
        static Vector2 lastPoint;

        public static void Init(VisualElement overlay)
        {
            card = new TooltipCard();
            overlay.Add(card);
            card.Show(false);
            readout = new TooltipCard(true);
            overlay.Add(readout);
            readout.Show(false);
            readout.RegisterCallback<PointerEnterEvent>(e => Cancel());
            readout.RegisterCallback<PointerLeaveEvent>(e =>
            {
                if (owner == null || !owner.worldBound.Contains(e.position)) Hide();
            });
        }

        public static void Attach(VisualElement element, string id)
        {
            AttachedIds.Add(id);
            Attach(element, () => UiText.Tip(id));
        }

        public static void Attach(VisualElement element, Func<TipContent> content, bool scrollable = false)
        {
            element.RegisterCallback<PointerEnterEvent>(e =>
            {
                lastPoint = Local(e.position);
                Hide();
                if (card == null) return;
                owner = element;
                interactive = scrollable;
                pending = element.schedule.Execute(() => Show(content())).StartingIn(DesignTokens.TooltipDelayMs);
            });
            element.RegisterCallback<PointerMoveEvent>(e => lastPoint = Local(e.position));
            element.RegisterCallback<PointerLeaveEvent>(e =>
            {
                if (owner != element) return;
                if (!interactive) { Hide(); return; }
                Cancel();
                if (!readout.worldBound.Contains(e.position))
                    pending = readout.schedule.Execute(Hide).StartingIn(DesignTokens.TooltipDelayMs);
            });
            element.RegisterCallback<PointerDownEvent>(e => { if (owner == element) Hide(); });
            element.RegisterCallback<DetachFromPanelEvent>(e => { if (owner == element) Hide(); });
        }

        static Vector2 Local(Vector2 panelPosition)
        {
            return Ui.Overlay != null ? Ui.Overlay.WorldToLocal(panelPosition) : panelPosition;
        }

        static void Cancel()
        {
            if (pending != null) { pending.Pause(); pending = null; }
        }

        static void Show(TipContent tip)
        {
            if (card == null) return;
            TooltipCard target = interactive ? readout : card;
            target.SetTip(tip);
            target.Show(true);
            target.BringToFront();
            target.PlaceNear(lastPoint);
        }

        public static void Hide()
        {
            Cancel();
            owner = null;
            if (card != null)
            {
                card.Show(false);
                card.style.visibility = Visibility.Hidden;
            }
            if (readout != null)
            {
                readout.Show(false);
                readout.style.visibility = Visibility.Hidden;
            }
        }
    }
}
