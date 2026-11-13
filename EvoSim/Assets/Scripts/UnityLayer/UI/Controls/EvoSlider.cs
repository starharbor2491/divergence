using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
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
            Label nameLabel = Ui.LabelText(name, DesignTokens.Text1);
            nameLabel.pickingMode = PickingMode.Position;
            Tooltips.Attach(nameLabel, tipId);
            header.Add(nameLabel);

            string rangeText = CompactNum(min) + "–" + CompactNum(max);
            Label rangeLabel = Ui.MonoCaption(rangeText, DesignTokens.Text3);
            rangeLabel.style.marginLeft = DesignTokens.Space2;
            header.Add(rangeLabel);

            header.Add(Ui.Spacer());
            valueLabel = Ui.Mono("", DesignTokens.Text1, DesignTokens.TypeLabelSize);
            header.Add(valueLabel);
            Add(header);

            area = new VisualElement();
            area.style.height = 28;
            area.style.marginTop = 4;
            area.style.justifyContent = Justify.Center;
            Add(area);

            var track = new VisualElement();
            track.pickingMode = PickingMode.Ignore;
            track.style.height = 5;
            track.Round(3);
            track.style.backgroundColor = new Color(1f, 1f, 1f, 0.14f);
            area.Add(track);

            // Subtle default baseline tick on the rail
            if (max > min)
            {
                float defFrac = Mathf.Clamp01((defaultValue - min) / (max - min));
                var defTick = new VisualElement();
                defTick.pickingMode = PickingMode.Ignore;
                defTick.style.position = Position.Absolute;
                defTick.style.left = Length.Percent(defFrac * 100f);
                defTick.style.top = 9;
                defTick.style.width = 1.5f;
                defTick.style.height = 10;
                defTick.style.backgroundColor = new Color(1f, 1f, 1f, 0.25f);
                area.Add(defTick);
            }

            fill = new VisualElement();
            fill.pickingMode = PickingMode.Ignore;
            fill.style.height = 5;
            fill.Round(3);
            fill.style.position = Position.Absolute;
            fill.style.left = 0;
            area.Add(fill);

            thumb = new VisualElement();
            thumb.pickingMode = PickingMode.Ignore;
            thumb.style.position = Position.Absolute;
            thumb.style.top = 6.5f;
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

        static string CompactNum(float v)
        {
            if (Mathf.Abs(v - Mathf.Round(v)) < 0.001f)
                return Mathf.RoundToInt(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Mathf.Abs(v * 10f - Mathf.Round(v * 10f)) < 0.01f)
                return v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            return v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
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
            fill.style.backgroundColor = enabled ? DesignTokens.ChartSeries1 : DesignTokens.TextDisabled;
            float d = (hover || dragging) && enabled ? DesignTokens.SliderThumbHoverSize : DesignTokens.SliderThumbSize;
            thumb.style.width = d; thumb.style.height = d;
            thumb.style.marginTop = (15f - d) * 0.5f;
            thumb.style.left = Length.Percent(t * 100f);
            thumb.style.translate = new Translate(-d * 0.5f, 0);
            thumb.style.backgroundColor = enabled ? DesignTokens.Text1 : DesignTokens.TextDisabled;
            thumb.Border(dragging && enabled ? DesignTokens.ChartSeries1 : DesignTokens.Bg0, 2);
            thumb.Round(d * 0.5f);
            valueLabel.text = format(value);
            valueLabel.style.color = enabled ? DesignTokens.Text1 : DesignTokens.TextDisabled;
        }
    }
}
