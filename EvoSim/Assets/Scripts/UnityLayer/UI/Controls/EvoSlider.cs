using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Slider with a label on the left and a readout on the right (DESIGN 4.3). Double-click resets to the default.</summary>
    public sealed class EvoSlider : VisualElement
    {
        public event Action<float> Changed;

        readonly float min, max, defaultValue, step;
        readonly Func<float, string> format;
        readonly Label valueLabel;
        readonly VisualElement area, fill, thumb;
        float value;
        bool dragging, hover, enabled = true;
        bool firstPressOnThumb;

        public EvoSlider(string name, string tipId, float min, float max, float defaultValue, float step, Func<float, string> format)
        {
            this.min = min; this.max = max; this.defaultValue = defaultValue; this.step = step; this.format = format;
            style.height = DesignTokens.SliderRowHeight;
            style.flexShrink = 0;

            VisualElement header = Ui.Row();
            header.style.height = DesignTokens.TypeLabelLine;
            Label nameLabel = Ui.LabelText(name);
            nameLabel.pickingMode = PickingMode.Position;
            Tooltips.Attach(nameLabel, tipId);
            header.Add(nameLabel);
            header.Add(Ui.Spacer());
            valueLabel = Ui.Body("");
            header.Add(valueLabel);
            Add(header);

            area = new VisualElement();
            area.style.height = 32;
            area.style.marginTop = DesignTokens.Space2;
            area.style.justifyContent = Justify.Center;
            Add(area);

            var track = new VisualElement();
            track.pickingMode = PickingMode.Ignore;
            track.style.height = 4;
            track.Round(2);
            track.style.backgroundColor = DesignTokens.Bg3;
            area.Add(track);

            fill = new VisualElement();
            fill.pickingMode = PickingMode.Ignore;
            fill.style.height = 4;
            fill.Round(2);
            fill.style.position = Position.Absolute;
            fill.style.left = 0;
            area.Add(fill);

            thumb = new VisualElement();
            thumb.pickingMode = PickingMode.Ignore;
            thumb.style.position = Position.Absolute;
            thumb.style.top = 8;
            thumb.Round(9);
            area.Add(thumb);

            area.RegisterCallback<PointerDownEvent>(OnDown);
            area.RegisterCallback<PointerMoveEvent>(OnMove);
            area.RegisterCallback<PointerUpEvent>(OnUp);
            area.RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            area.RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            area.RegisterCallback<PointerCaptureOutEvent>(e => { dragging = false; Refresh(); });
            value = defaultValue;
            Refresh();
        }

        public float Value
        {
            get { return value; }
            set { SetValue(value, false); }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; Refresh(); }
        }

        public void SetValue(float v, bool notify)
        {
            v = Mathf.Clamp(v, min, max);
            if (step > 0f) v = Mathf.Round((v - min) / step) * step + min;
            v = Mathf.Clamp(v, min, max);
            bool changed = !Mathf.Approximately(v, value);
            value = v;
            Refresh();
            if (changed && notify && Changed != null) Changed(value);
        }

        void FromPointer(Vector2 local)
        {
            float w = area.layout.width;
            if (float.IsNaN(w) || float.IsInfinity(w) || w <= 0f) return;
            SetValue(min + Mathf.Clamp01(local.x / w) * (max - min), true);
        }

        void OnDown(PointerDownEvent e)
        {
            if (!enabled || e.button != 0) return;
            bool onThumb = thumb.worldBound.Contains(e.position);
            if (e.clickCount >= 2 && firstPressOnThumb && onThumb) { SetValue(defaultValue, true); return; }
            if (e.clickCount < 2) firstPressOnThumb = onThumb;
            dragging = true;
            area.CapturePointer(e.pointerId);
            FromPointer(e.localPosition);
            Tooltips.Hide();
        }

        void OnMove(PointerMoveEvent e)
        {
            if (dragging) FromPointer(e.localPosition);
        }

        void OnUp(PointerUpEvent e)
        {
            if (!dragging) return;
            dragging = false;
            if (area.HasPointerCapture(e.pointerId)) area.ReleasePointer(e.pointerId);
            Refresh();
        }

        void Refresh()
        {
            float t = max > min ? (value - min) / (max - min) : 0f;
            fill.style.width = Length.Percent(t * 100f);
            fill.style.backgroundColor = enabled ? DesignTokens.Accent : DesignTokens.TextDisabled;
            float d = (hover || dragging) && enabled ? DesignTokens.SliderThumbHoverSize : DesignTokens.SliderThumbSize;
            thumb.style.width = d; thumb.style.height = d;
            thumb.style.marginTop = (16f - d) * 0.5f;
            thumb.style.left = Length.Percent(t * 100f);
            thumb.style.translate = new Translate(-d * 0.5f, 0);
            thumb.style.backgroundColor = enabled ? DesignTokens.Text1 : DesignTokens.TextDisabled;
            thumb.Border(DesignTokens.Bg1, 2);
            thumb.Round(d * 0.5f);
            valueLabel.text = format(value);
            valueLabel.style.color = enabled ? DesignTokens.Text1 : DesignTokens.TextDisabled;
        }
    }
}
