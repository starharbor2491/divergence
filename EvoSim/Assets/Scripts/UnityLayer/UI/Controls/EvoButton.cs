using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum ButtonKind { Primary, Secondary, Icon, Tab }

    /// <summary>Button, icon button, toggle and tab in one class (DESIGN 4.1, 4.2 and 4.7). Colours come from tokens only.</summary>
    public sealed class EvoButton : VisualElement
    {
        public event Action Clicked;

        readonly ButtonKind kind;
        readonly IconElement icon;
        readonly Label label;
        bool hover, pressed, disabled, toggled, selected;
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
            this.Round(DesignTokens.Radius);
            focusable = false;

            if (iconName != null)
            {
                icon = new IconElement(iconName, 20f);
                Add(icon);
            }
            if (!string.IsNullOrEmpty(text))
            {
                label = Ui.LabelText(text);
                if (icon != null) label.style.marginLeft = DesignTokens.Space2;
                Add(label);
            }
            if (kind == ButtonKind.Icon)
            {
                style.width = DesignTokens.ControlHeight;
            }
            else
            {
                this.Pad(0, DesignTokens.Space4);
            }
            if (kind == ButtonKind.Secondary) this.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);

            RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            RegisterCallback<PointerDownEvent>(OnDown);
            RegisterCallback<PointerUpEvent>(OnUp);
            RegisterCallback<PointerCaptureOutEvent>(e => { pressed = false; Refresh(); });
            if (tipId != null) SetTip(tipId);
            Refresh();
        }

        public string TipId { get { return tipId; } }

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

        /// <summary>The on state of a toggle (icon in accent-text with a 2 px accent line along the bottom).</summary>
        public bool Toggled
        {
            get { return toggled; }
            set { toggled = value; Refresh(); }
        }

        /// <summary>The chosen member of a segmented control or the open tab.</summary>
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

        void Refresh()
        {
            Color bg = Color.clear, fg = DesignTokens.Text1, iconColor = DesignTokens.Text2;
            Color borderColor = DesignTokens.BorderStrong;
            bool underline = false;
            switch (kind)
            {
                case ButtonKind.Primary:
                    bg = disabled ? DesignTokens.Bg2 : (pressed ? DesignTokens.AccentPressed : (hover ? DesignTokens.AccentText : DesignTokens.Accent));
                    fg = disabled ? DesignTokens.TextDisabled : DesignTokens.OnAccent;
                    iconColor = fg;
                    break;
                case ButtonKind.Secondary:
                    bg = pressed ? DesignTokens.Bg4 : ((hover || selected) && !disabled ? DesignTokens.Bg3 : DesignTokens.Bg2);
                    fg = disabled ? DesignTokens.TextDisabled : (selected ? DesignTokens.AccentText : DesignTokens.Text1);
                    iconColor = fg;
                    borderColor = disabled ? DesignTokens.Border : DesignTokens.BorderStrong;
                    break;
                case ButtonKind.Icon:
                    bg = pressed ? DesignTokens.Bg4 : ((hover || selected) && !disabled ? DesignTokens.Bg3 : Color.clear);
                    iconColor = disabled ? DesignTokens.TextDisabled
                        : (toggled || selected ? DesignTokens.AccentText : (hover ? DesignTokens.Text1 : DesignTokens.Text2));
                    underline = toggled;
                    break;
                case ButtonKind.Tab:
                    bg = pressed ? DesignTokens.Bg4 : (hover && !disabled ? DesignTokens.Bg3 : Color.clear);
                    fg = selected ? DesignTokens.Text1 : DesignTokens.Text2;
                    iconColor = selected ? DesignTokens.AccentText : DesignTokens.Text2;
                    underline = selected;
                    break;
            }
            style.backgroundColor = bg;
            if (kind == ButtonKind.Secondary) this.Border(borderColor, DesignTokens.BorderWidth);
            if (kind == ButtonKind.Icon || kind == ButtonKind.Tab)
            {
                style.borderBottomWidth = underline ? 2 : 0;
                style.borderBottomColor = DesignTokens.Accent;
            }
            if (label != null) label.style.color = (kind == ButtonKind.Icon) ? iconColor : fg;
            if (icon != null) icon.Tint = iconColor;
        }

        /// <summary>Square corners at one end, for segmented controls (DESIGN 4.2).</summary>
        public void RoundEnds(bool left, bool right)
        {
            style.borderTopLeftRadius = style.borderBottomLeftRadius = left ? DesignTokens.Radius : 0;
            style.borderTopRightRadius = style.borderBottomRightRadius = right ? DesignTokens.Radius : 0;
        }
    }
}
