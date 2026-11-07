using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The tooltip card (DESIGN 4.10). Also used by charts for their hover read-outs.</summary>
    public sealed class TooltipCard : VisualElement
    {
        readonly VisualElement body = new VisualElement();

        public TooltipCard()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.backgroundColor = DesignTokens.Bg2;
            this.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            this.Round(DesignTokens.Radius);
            this.Pad(DesignTokens.Space2, DesignTokens.Space3);
            style.maxWidth = DesignTokens.TooltipMaxWidth;
            style.visibility = Visibility.Hidden;
            body.pickingMode = PickingMode.Ignore;
            Add(body);
        }

        public void SetTip(TipContent tip)
        {
            body.Clear();
            body.Add(Ui.CaptionBold(tip.Name, DesignTokens.Text1));
            if (!string.IsNullOrEmpty(tip.Body))
            {
                Label l = Ui.Caption(tip.Body, DesignTokens.Text2);
                l.style.marginTop = DesignTokens.Space1 / 2;
                body.Add(l);
            }
            if (!string.IsNullOrEmpty(tip.Key))
            {
                Label l = Ui.Caption(tip.Key, DesignTokens.Text3);
                l.style.marginTop = DesignTokens.Space1;
                body.Add(l);
            }
        }

        /// <summary>A chart read-out: a title line and rows with an optional colour swatch, a name and a value.</summary>
        public void SetRows(string title, List<ReadoutRow> rows)
        {
            body.Clear();
            body.Add(Ui.CaptionBold(title, DesignTokens.Text1));
            foreach (ReadoutRow r in rows)
            {
                VisualElement row = Ui.Row();
                row.pickingMode = PickingMode.Ignore;
                row.style.marginTop = DesignTokens.Space1;
                if (r.Swatch.HasValue)
                {
                    var sw = new VisualElement();
                    sw.pickingMode = PickingMode.Ignore;
                    sw.Size(10, 10);
                    sw.Round(DesignTokens.RadiusCheckbox);
                    sw.style.backgroundColor = r.Swatch.Value;
                    sw.style.marginRight = DesignTokens.Space2;
                    sw.style.flexShrink = 0;
                    row.Add(sw);
                }
                Label name = Ui.Caption(r.Name, DesignTokens.Text2);
                name.style.flexGrow = 1;
                row.Add(name);
                Label val = Ui.Caption(r.Value, DesignTokens.Text1);
                val.style.marginLeft = DesignTokens.Space4;
                val.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(val);
                body.Add(row);
            }
        }

        /// <summary>Places the card below and to the right of a point, flipped to stay inside the parent.</summary>
        public void PlaceNear(Vector2 point)
        {
            style.left = point.x + 14;
            style.top = point.y + 18;
            style.visibility = Visibility.Hidden;
            RegisterCallbackOnce<GeometryChangedEvent>(e => Reposition(point));
            schedule.Execute(() => Reposition(point)).StartingIn(0);
        }

        void Reposition(Vector2 point)
        {
            if (parent == null) return;
            float pw = parent.layout.width, ph = parent.layout.height;
            float w = layout.width, h = layout.height;
            if (float.IsNaN(w) || w <= 0f) return;
            float x = point.x + 14, y = point.y + 18;
            if (x + w > pw - 4) x = point.x - 14 - w;
            if (y + h > ph - 4) y = point.y - 14 - h;
            style.left = Mathf.Max(4, x);
            style.top = Mathf.Max(4, y);
            style.visibility = Visibility.Visible;
        }
    }

    public struct ReadoutRow
    {
        public Color? Swatch;
        public string Name;
        public string Value;
    }

    /// <summary>
    /// UI Toolkit does not show tooltips in a running player, so elements register here (DESIGN 4.10).
    /// A tooltip appears after 400 ms of hovering and goes away on leave or press.
    /// </summary>
    public static class Tooltips
    {
        public static readonly HashSet<string> AttachedIds = new HashSet<string>();

        static TooltipCard card;
        static IVisualElementScheduledItem pending;
        static VisualElement owner;
        static Vector2 lastPoint;

        public static void Init(VisualElement overlay)
        {
            card = new TooltipCard();
            overlay.Add(card);
            card.Show(false);
        }

        public static void Attach(VisualElement element, string id)
        {
            AttachedIds.Add(id);
            Attach(element, () => UiText.Tip(id));
        }

        public static void Attach(VisualElement element, Func<TipContent> content)
        {
            element.RegisterCallback<PointerEnterEvent>(e =>
            {
                lastPoint = Local(e.position);
                Cancel();
                if (card == null) return;
                owner = element;
                pending = element.schedule.Execute(() => Show(content())).StartingIn(DesignTokens.TooltipDelayMs);
            });
            element.RegisterCallback<PointerMoveEvent>(e => lastPoint = Local(e.position));
            element.RegisterCallback<PointerLeaveEvent>(e => { if (owner == element) Hide(); });
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
            card.SetTip(tip);
            card.Show(true);
            card.BringToFront();
            card.PlaceNear(lastPoint);
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
        }
    }
}
