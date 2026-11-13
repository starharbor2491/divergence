using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum FontRole { Regular, Medium, SemiBold, Bold, MonoRegular, MonoMedium, MonoBold }

    /// <summary>Small helpers that turn DesignTokens into element styles. Components call these and never write a literal.</summary>
    public static class Ui
    {
        public static Font UiRegular, UiMedium, UiSemiBold, UiBold;
        public static Font FontRegular, FontMedium, FontBold;

        /// <summary>The element that receives popups and tooltips (a full-window layer above everything).</summary>
        public static VisualElement Overlay;

        static Font FontFor(FontRole role)
        {
            Font f;
            switch (role)
            {
                case FontRole.Bold: f = UiBold ?? FontBold; break;
                case FontRole.SemiBold: f = UiSemiBold ?? UiMedium ?? FontMedium; break;
                case FontRole.Medium: f = UiMedium ?? FontMedium; break;
                case FontRole.MonoBold: f = FontBold ?? UiBold; break;
                case FontRole.MonoMedium: f = FontMedium ?? FontRegular; break;
                case FontRole.MonoRegular: f = FontRegular ?? UiRegular; break;
                default: f = UiRegular ?? FontRegular; break;
            }
            return f ?? UiRegular ?? FontRegular;
        }

        public static void Type(VisualElement e, FontRole role, int size)
        {
            Font f = FontFor(role);
            if (f != null) e.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(f));
            e.style.fontSize = size;
            e.style.unityFontStyleAndWeight = FontStyle.Normal;
            e.style.whiteSpace = WhiteSpace.Normal;
        }

        // ---- type tokens (DESIGN 3): Plus Jakarta Sans for UI/prose, JetBrains Mono for figures/telemetry
        public static Label Display(string text, Color c) { return Make(text, FontRole.Bold, DesignTokens.TypeDisplaySize, c); }
        public static Label Heading(string text, Color c) { return Make(text, FontRole.Bold, DesignTokens.TypeHeadingSize, c); }
        public static Label Title(string text, Color? c = null) { return Make(text, FontRole.Bold, DesignTokens.TypeTitleSize, c ?? DesignTokens.Text1); }
        public static Label Subhead(string text, Color? c = null) { return Make(text, FontRole.SemiBold, DesignTokens.TypeBodySize, c ?? DesignTokens.Text1); }
        public static Label Body(string text, Color? c = null) { return Make(text, FontRole.Regular, DesignTokens.TypeBodySize, c ?? DesignTokens.Text1); }
        public static Label LabelText(string text, Color? c = null) { return Make(text, FontRole.SemiBold, DesignTokens.TypeLabelSize, c ?? DesignTokens.Text1); }
        public static Label Caption(string text, Color? c = null) { return Make(text, FontRole.Regular, DesignTokens.TypeCaptionSize, c ?? DesignTokens.Text3); }
        public static Label CaptionBold(string text, Color? c = null) { return Make(text, FontRole.SemiBold, DesignTokens.TypeCaptionSize, c ?? DesignTokens.Text1); }
        public static Label Mono(string text, Color? c = null, int size = DesignTokens.TypeBodySize) { return Make(text, FontRole.MonoMedium, size, c ?? DesignTokens.Text1); }
        public static Label MonoCaption(string text, Color? c = null) { return Make(text, FontRole.MonoRegular, DesignTokens.TypeCaptionSize, c ?? DesignTokens.Text3); }

        public static Label Make(string text, FontRole role, int size, Color color)
        {
            var l = new Label(text);
            Type(l, role, size);
            l.style.color = color;
            l.style.marginLeft = l.style.marginRight = l.style.marginTop = l.style.marginBottom = 0;
            l.style.paddingLeft = l.style.paddingRight = l.style.paddingTop = l.style.paddingBottom = 0;
            l.pickingMode = PickingMode.Ignore;
            return l;
        }

        // ---- layout
        public static VisualElement Row(Align align = Align.Center)
        {
            var e = new VisualElement();
            e.style.flexDirection = FlexDirection.Row;
            e.style.alignItems = align;
            return e;
        }

        public static VisualElement Column()
        {
            var e = new VisualElement();
            e.style.flexDirection = FlexDirection.Column;
            return e;
        }

        public static VisualElement Spacer()
        {
            var e = new VisualElement();
            e.style.flexGrow = 1;
            e.pickingMode = PickingMode.Ignore;
            return e;
        }

        /// <summary>1 px divider: a hairline in a row or full width in a column.</summary>
        public static VisualElement Divider(bool vertical, int length = 24)
        {
            var e = new VisualElement();
            e.style.backgroundColor = DesignTokens.Border;
            e.pickingMode = PickingMode.Ignore;
            e.style.flexShrink = 0;
            if (vertical) { e.style.width = DesignTokens.BorderWidth; e.style.height = length; }
            else { e.style.height = DesignTokens.BorderWidth; e.style.alignSelf = Align.Stretch; }
            return e;
        }

        public static VisualElement Card(float pad = DesignTokens.Space4)
        {
            var e = new VisualElement();
            LiquidGlass.StyleWell(e, DesignTokens.RadiusCard);
            e.Pad(pad);
            return e;
        }

        // ---- shared element extensions
        /// <summary>Adds children with a fixed gap between them (UI Toolkit has no flex gap).</summary>
        public static T AddSpaced<T>(this T parent, float gap, params VisualElement[] children) where T : VisualElement
        {
            bool row = parent.style.flexDirection.value == FlexDirection.Row;
            bool first = parent.childCount == 0;
            foreach (VisualElement c in children)
            {
                if (!first)
                {
                    if (row) c.style.marginLeft = gap; else c.style.marginTop = gap;
                }
                first = false;
                parent.Add(c);
            }
            return parent;
        }

        public static void Pad(this VisualElement e, float all) { e.Pad(all, all, all, all); }
        public static void Pad(this VisualElement e, float vertical, float horizontal) { e.Pad(vertical, horizontal, vertical, horizontal); }
        public static void Pad(this VisualElement e, float top, float right, float bottom, float left)
        {
            e.style.paddingTop = top; e.style.paddingRight = right; e.style.paddingBottom = bottom; e.style.paddingLeft = left;
        }

        public static void Margin(this VisualElement e, float top, float right, float bottom, float left)
        {
            e.style.marginTop = top; e.style.marginRight = right; e.style.marginBottom = bottom; e.style.marginLeft = left;
        }

        public static void Border(this VisualElement e, Color color, float width)
        {
            e.style.borderTopColor = e.style.borderRightColor = e.style.borderBottomColor = e.style.borderLeftColor = color;
            e.style.borderTopWidth = e.style.borderRightWidth = e.style.borderBottomWidth = e.style.borderLeftWidth = width;
        }

        public static void Round(this VisualElement e, float radius)
        {
            e.style.borderTopLeftRadius = e.style.borderTopRightRadius = e.style.borderBottomLeftRadius = e.style.borderBottomRightRadius = radius;
        }

        public static void Size(this VisualElement e, float w, float h)
        {
            e.style.width = w; e.style.height = h;
        }

        public static void Show(this VisualElement e, bool visible)
        {
            e.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static bool IsShown(this VisualElement e) { return e.resolvedStyle.display != DisplayStyle.None && e.style.display != DisplayStyle.None; }

        public static void Fill(this VisualElement e)
        {
            e.style.position = Position.Absolute;
            e.style.left = 0; e.style.top = 0; e.style.right = 0; e.style.bottom = 0;
        }

        public static string Mmss(double seconds)
        {
            int s = Mathf.Max(0, (int)seconds);
            int h = s / 3600, m = (s / 60) % 60, sec = s % 60;
            return h > 0 ? string.Format("{0}:{1:00}:{2:00}", h, m, sec) : string.Format("{0:00}:{1:00}", m, sec);
        }

        /// <summary>No thousands separator under 10,000; commas from 10,000.</summary>
        public static string Count(int n)
        {
            return n >= 10000 ? n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) : n.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string F(float v, int decimals)
        {
            return v.ToString(decimals == 0 ? "0" : (decimals == 1 ? "0.0" : "0.00"), System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
