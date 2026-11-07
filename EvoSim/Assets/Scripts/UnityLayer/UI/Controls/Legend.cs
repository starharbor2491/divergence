using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>One legend row (DESIGN 4.12): 28 px, a 12 px swatch, a name and a right-aligned value. Hover is bg-3.</summary>
    public sealed class LegendRow : VisualElement
    {
        public event Action Clicked;
        readonly VisualElement swatch;
        readonly Label nameLabel, valueLabel;
        bool selected, hover;

        public int Tag;

        public LegendRow(string tipId)
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.height = DesignTokens.LegendRowHeight;
            style.flexShrink = 0;
            this.Pad(0, DesignTokens.Space2);
            this.Round(DesignTokens.Radius);
            swatch = new VisualElement();
            swatch.Size(12, 12);
            swatch.Round(DesignTokens.RadiusCheckbox);
            swatch.style.flexShrink = 0;
            swatch.pickingMode = PickingMode.Ignore;
            Add(swatch);
            nameLabel = Ui.Body("");
            nameLabel.style.marginLeft = DesignTokens.Space2;
            nameLabel.style.flexGrow = 1;
            nameLabel.style.flexShrink = 1;
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.whiteSpace = WhiteSpace.NoWrap;
            Add(nameLabel);
            valueLabel = Ui.Body("", DesignTokens.Text2);
            valueLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            valueLabel.style.marginLeft = DesignTokens.Space2;
            Add(valueLabel);
            if (tipId != null) Tooltips.Attach(this, tipId);
            RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            RegisterCallback<PointerDownEvent>(e => { if (e.button == 0 && Clicked != null) Clicked(); });
            Refresh();
        }

        public void Set(Color color, string name, string value, bool selected = false)
        {
            swatch.style.backgroundColor = color;
            if (nameLabel.text != name) nameLabel.text = name;
            if (valueLabel.text != value) valueLabel.text = value;
            if (this.selected != selected) { this.selected = selected; Refresh(); }
        }

        void Refresh()
        {
            style.backgroundColor = hover || selected ? DesignTokens.Bg3 : Color.clear;
        }
    }

    /// <summary>Scroll view with the 8 px scrollbar of DESIGN 4.14.</summary>
    public static class UiScroll
    {
        public static ScrollView Create()
        {
            var s = new ScrollView(ScrollViewMode.Vertical);
            s.style.flexGrow = 1;
            s.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            s.verticalScrollerVisibility = ScrollerVisibility.Auto;
            s.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            Scroller v = s.verticalScroller;
            v.style.width = DesignTokens.ScrollbarWidth;
            v.style.backgroundColor = DesignTokens.Bg1;
            v.style.borderLeftWidth = v.style.borderRightWidth = v.style.borderTopWidth = v.style.borderBottomWidth = 0;
            v.lowButton.style.display = DisplayStyle.None;
            v.highButton.style.display = DisplayStyle.None;
            v.slider.style.marginLeft = v.slider.style.marginRight = v.slider.style.marginTop = v.slider.style.marginBottom = 0;
            v.slider.style.borderLeftWidth = v.slider.style.borderRightWidth = v.slider.style.borderTopWidth = v.slider.style.borderBottomWidth = 0;
            v.slider.style.width = DesignTokens.ScrollbarWidth;
            v.slider.style.backgroundColor = Color.clear;
            VisualElement tracker = v.slider.Q("unity-tracker");
            if (tracker != null) { tracker.style.backgroundColor = DesignTokens.Bg1; tracker.style.borderLeftWidth = tracker.style.borderRightWidth = tracker.style.borderTopWidth = tracker.style.borderBottomWidth = 0; }
            VisualElement dragger = v.slider.Q("unity-dragger");
            if (dragger != null)
            {
                dragger.style.backgroundColor = DesignTokens.Bg4;
                dragger.style.borderLeftWidth = dragger.style.borderRightWidth = dragger.style.borderTopWidth = dragger.style.borderBottomWidth = 0;
                dragger.Round(DesignTokens.Radius);
                dragger.style.width = DesignTokens.ScrollbarWidth;
                dragger.style.marginLeft = dragger.style.marginRight = 0;
                dragger.RegisterCallback<PointerEnterEvent>(e => dragger.style.backgroundColor = DesignTokens.BorderStrong);
                dragger.RegisterCallback<PointerLeaveEvent>(e => dragger.style.backgroundColor = DesignTokens.Bg4);
            }
            VisualElement border = v.slider.Q("unity-dragger-border");
            if (border != null) border.style.display = DisplayStyle.None;
            s.contentContainer.style.flexShrink = 0;
            return s;
        }
    }
}
