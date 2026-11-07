using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>20 px checkbox with a label (DESIGN 4.6).</summary>
    public sealed class EvoCheckbox : VisualElement
    {
        public event Action<bool> Changed;
        readonly VisualElement box;
        readonly IconElement mark;
        bool value, hover;

        public EvoCheckbox(string text, string tipId)
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.height = DesignTokens.ControlHeight;
            box = new VisualElement();
            box.Size(20, 20);
            box.Round(DesignTokens.RadiusCheckbox);
            box.style.alignItems = Align.Center;
            box.style.justifyContent = Justify.Center;
            box.style.flexShrink = 0;
            mark = new IconElement("check", 16f);
            box.Add(mark);
            Add(box);
            Label l = Ui.Body(text);
            l.style.marginLeft = DesignTokens.Space2;
            Add(l);
            Tooltips.Attach(this, tipId);
            RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button != 0) return;
                Value = !value;
                if (Changed != null) Changed(value);
            });
            Refresh();
        }

        public bool Value
        {
            get { return value; }
            set { this.value = value; Refresh(); }
        }

        void Refresh()
        {
            box.style.backgroundColor = value ? DesignTokens.Accent : (hover ? DesignTokens.Bg3 : DesignTokens.Bg2);
            box.Border(value ? DesignTokens.Accent : DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            mark.Tint = value ? DesignTokens.OnAccent : Color.clear;
        }
    }

    /// <summary>Single line text field (DESIGN 4.4): 40 px, bg-2, focus outline, invalid hint.</summary>
    public sealed class EvoTextField : VisualElement
    {
        public event Action<string> Changed;
        readonly TextField field;
        readonly VisualElement frame;
        readonly VisualElement hint;
        readonly Label hintText;
        bool focused, invalid;

        public EvoTextField(string tipId, string placeholder = "")
        {
            frame = new VisualElement();
            frame.style.height = DesignTokens.ControlHeight;
            frame.style.backgroundColor = DesignTokens.Bg2;
            frame.style.justifyContent = Justify.Center;
            frame.Round(DesignTokens.Radius);
            frame.Pad(0, DesignTokens.Space3);
            Add(frame);

            field = new TextField();
            field.style.marginLeft = field.style.marginRight = field.style.marginTop = field.style.marginBottom = 0;
            field.style.flexGrow = 1;
            VisualElement input = field.Q(className: "unity-base-text-field__input");
            field.RegisterCallback<GeometryChangedEvent>(e => StyleInput());
            field.RegisterCallback<FocusInEvent>(e => { focused = true; Refresh(); });
            field.RegisterCallback<FocusOutEvent>(e => { focused = false; Refresh(); });
            field.RegisterValueChangedCallback(e => { if (Changed != null) Changed(e.newValue); });
            frame.Add(field);
            Tooltips.Attach(this, tipId);

            hint = Ui.Row();
            hint.style.marginTop = DesignTokens.Space1;
            hint.Add(new IconElement("status-serious", 16f) { Tint = DesignTokens.StatusSerious });
            hintText = Ui.Caption("", DesignTokens.Text2);
            hintText.style.marginLeft = DesignTokens.Space2;
            hint.Add(hintText);
            hint.Show(false);
            Add(hint);
            Refresh();
        }

        void StyleInput()
        {
            VisualElement input = field.Q(className: "unity-base-text-field__input");
            if (input == null) return;
            input.style.backgroundColor = Color.clear;
            input.Border(Color.clear, 0);
            input.Pad(0);
            input.style.color = DesignTokens.Text1;
            Ui.Type(input, FontRole.Regular, DesignTokens.TypeBodySize);
            input.style.unityTextAlign = TextAnchor.MiddleLeft;
            field.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        public string Value
        {
            get { return field.value; }
            set { field.SetValueWithoutNotify(value); }
        }

        public void SetInvalid(string message)
        {
            invalid = !string.IsNullOrEmpty(message);
            hint.Show(invalid);
            hintText.text = message ?? "";
            Refresh();
        }

        void Refresh()
        {
            Color c = focused ? DesignTokens.Focus : (invalid ? DesignTokens.StatusSerious : DesignTokens.BorderStrong);
            frame.Border(c, focused ? 2 : DesignTokens.BorderWidth);
        }
    }

    /// <summary>Status message block with an icon, text and an optional action (DESIGN 4.13).</summary>
    public sealed class Message : VisualElement
    {
        public Message(string status, string text)
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.backgroundColor = DesignTokens.Bg2;
            this.Pad(DesignTokens.Space3, DesignTokens.Space4);
            style.borderLeftWidth = 3;
            Color c = status == "status-good" ? DesignTokens.StatusGood
                : status == "status-warning" ? DesignTokens.StatusWarning
                : status == "status-critical" ? DesignTokens.StatusCritical : DesignTokens.StatusSerious;
            style.borderLeftColor = c;
            this.Round(DesignTokens.Radius);
            var icon = new IconElement(status, 20f) { Tint = c };
            icon.style.marginRight = DesignTokens.Space3;
            Add(icon);
            Label l = Ui.Body(text);
            l.style.flexShrink = 1;
            Add(l);
        }
    }
}
