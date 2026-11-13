using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum ButtonKind { Primary, Secondary, Icon, Tab }

    /// <summary>Liquid Glass button, icon button, toggle and segmented pill tab. Colours come from DesignTokens only.</summary>
    public sealed class EvoButton : VisualElement
    {
        public event Action Clicked;

        readonly ButtonKind kind;
        readonly IconElement icon;
        readonly Label label;
        bool hover, pressed, disabled, toggled, selected;
        Color? customIconTint;
        string tipId;

        public EvoButton(ButtonKind kind, string text, string iconName, string tipId)
        {
            this.kind = kind;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.height = DesignTokens.ControlHeight;
            style.minWidth = DesignTokens.ControlHeight;
            style.flexShrink = 0;
            this.Round(kind == ButtonKind.Primary ? DesignTokens.RadiusPill : DesignTokens.Radius);
            focusable = false;

            if (iconName != null)
            {
                icon = new IconElement(iconName, 18f);
                Add(icon);
            }
            if (!string.IsNullOrEmpty(text))
            {
                label = Ui.LabelText(text);
                label.style.whiteSpace = WhiteSpace.NoWrap;
                if (icon != null) label.style.marginLeft = DesignTokens.Space2;
                Add(label);
            }
            if (kind == ButtonKind.Icon && string.IsNullOrEmpty(text))
            {
                style.width = DesignTokens.ControlHeight;
            }
            else
            {
                this.Pad(0, kind == ButtonKind.Primary ? DesignTokens.Space5 : 14);
            }

            generateVisualContent += PaintHighlight;

            RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            RegisterCallback<PointerDownEvent>(OnDown);
            RegisterCallback<PointerUpEvent>(OnUp);
            RegisterCallback<PointerCaptureOutEvent>(e => { pressed = false; Refresh(); });
            if (tipId != null) SetTip(tipId);
            Refresh();
        }

        public string TipId { get { return tipId; } }

        public Color? AccentOverride
        {
            get { return customIconTint; }
            set { customIconTint = value; Refresh(); }
        }

        public void SetTip(string id)
        {
            if (tipId == null) Tooltips.Attach(this, () => UiText.Tip(tipId));
            tipId = id;
            Tooltips.AttachedIds.Add(id);
        }

        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; Refresh(); }
        }

        public bool Toggled
        {
            get { return toggled; }
            set { toggled = value; Refresh(); }
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; Refresh(); }
        }

        public IconElement Icon { get { return icon; } }

        public void SetText(string text) { if (label != null) label.text = text; }

        void OnDown(PointerDownEvent e)
        {
            if (disabled || e.button != 0) return;
            pressed = true;
            this.CapturePointer(e.pointerId);
            Refresh();
            e.StopPropagation();
        }

        void OnUp(PointerUpEvent e)
        {
            if (!pressed) return;
            pressed = false;
            if (this.HasPointerCapture(e.pointerId)) this.ReleasePointer(e.pointerId);
            Refresh();
            if (!disabled && IsInside(e.localPosition))
            {
                Tooltips.Hide();
                if (Clicked != null) Clicked();
            }
            e.StopPropagation();
        }

        bool IsInside(Vector2 local)
        {
            return local.x >= 0 && local.y >= 0 && local.x <= layout.width && local.y <= layout.height;
        }

        void PaintHighlight(MeshGenerationContext ctx)
        {
            if (disabled) return;
            float w = layout.width, h = layout.height;
            if (float.IsNaN(w) || w < 6f || h < 6f) return;
            bool showGloss = kind == ButtonKind.Primary || (kind == ButtonKind.Secondary && (hover || selected || toggled)) || (kind == ButtonKind.Tab && selected);
            if (!showGloss) return;

            float rad = Mathf.Min(resolvedStyle.borderTopLeftRadius, Mathf.Min(w, h) * 0.5f);
            Painter2D p = ctx.painter2D;
            p.lineCap = LineCap.Round;
            p.lineWidth = 1.15f;
            p.strokeColor = kind == ButtonKind.Primary ? new Color(1f, 1f, 1f, 0.50f) : new Color(1f, 1f, 1f, 0.28f);
            p.BeginPath();
            float rr = Mathf.Max(0f, rad - 0.6f);
            if (rr > 0.5f)
            {
                p.MoveTo(new Vector2(0.6f + rr, 0.8f));
                p.LineTo(new Vector2(w - 0.6f - rr, 0.8f));
            }
            else
            {
                p.MoveTo(new Vector2(2f, 0.8f));
                p.LineTo(new Vector2(w - 2f, 0.8f));
            }
            p.Stroke();
        }

        void Refresh()
        {
            Color bg = Color.clear, fg = DesignTokens.Text1, iconColor = DesignTokens.Text2;
            Color borderColor = Color.clear;
            float borderW = 0f;

            switch (kind)
            {
                case ButtonKind.Primary:
                    bg = disabled ? DesignTokens.Bg2 : (pressed ? DesignTokens.AccentPressed : (hover ? DesignTokens.AccentText : DesignTokens.Accent));
                    fg = disabled ? DesignTokens.TextDisabled : DesignTokens.OnAccent;
                    iconColor = fg;
                    break;
                case ButtonKind.Secondary:
                    bool activeSec = toggled || selected;
                    bg = disabled ? DesignTokens.GlassInset
                        : (pressed ? DesignTokens.Bg4
                        : (activeSec ? DesignTokens.AccentSubtle
                        : (hover ? DesignTokens.Bg3 : DesignTokens.Bg2)));
                    fg = disabled ? DesignTokens.TextDisabled : (activeSec ? DesignTokens.AccentText : DesignTokens.Text1);
                    iconColor = customIconTint.HasValue && !disabled && !activeSec ? customIconTint.Value : fg;
                    borderColor = disabled ? DesignTokens.Border : (activeSec ? DesignTokens.WithAlpha(DesignTokens.Accent, 0.55f) : DesignTokens.BorderStrong);
                    borderW = DesignTokens.BorderWidth;
                    break;
                case ButtonKind.Icon:
                    bool activeIcon = toggled || selected;
                    bg = pressed ? DesignTokens.Bg4
                        : (activeIcon && !disabled ? DesignTokens.AccentSubtle
                        : (hover && !disabled ? DesignTokens.Bg3 : Color.clear));
                    iconColor = disabled ? DesignTokens.TextDisabled
                        : (activeIcon ? DesignTokens.AccentText
                        : (customIconTint.HasValue ? customIconTint.Value
                        : (hover ? DesignTokens.Text1 : DesignTokens.Text2)));
                    fg = activeIcon ? DesignTokens.AccentText : (hover ? DesignTokens.Text1 : DesignTokens.Text2);
                    if (activeIcon && !disabled)
                    {
                        borderColor = DesignTokens.WithAlpha(DesignTokens.Accent, 0.45f);
                        borderW = 1f;
                    }
                    break;
                case ButtonKind.Tab:
                    bg = pressed ? DesignTokens.Bg4
                        : (selected ? DesignTokens.Bg3
                        : (hover && !disabled ? DesignTokens.WithAlpha(DesignTokens.Bg3, 0.40f) : Color.clear));
                    fg = selected ? DesignTokens.Text1 : (hover ? DesignTokens.Text1 : DesignTokens.Text2);
                    iconColor = selected ? DesignTokens.AccentText : fg;
                    if (selected)
                    {
                        borderColor = DesignTokens.BorderStrong;
                        borderW = 1f;
                    }
                    break;
            }

            style.backgroundColor = bg;
            this.Border(borderColor, borderW);
            if (label != null) label.style.color = fg;
            if (icon != null) icon.Tint = iconColor;
            MarkDirtyRepaint();
        }

        /// <summary>Adjusts outer ends for segmented controls.</summary>
        public void RoundEnds(bool left, bool right)
        {
            float r = DesignTokens.Radius - 2;
            style.borderTopLeftRadius = style.borderBottomLeftRadius = left ? r : 6;
            style.borderTopRightRadius = style.borderBottomRightRadius = right ? r : 6;
        }
    }
}
