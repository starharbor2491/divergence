using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Liquid Glass dropdown control that opens a frosted menu card in the overlay layer.</summary>
    public sealed class EvoDropdown : VisualElement
    {
        public event Action<int> Changed;

        readonly Label current;
        readonly IconElement chevron;
        readonly List<string> items = new List<string>();
        readonly Func<int, string> iconFor;
        VisualElement catcher;
        int selected = -1;
        bool hover;

        public EvoDropdown(string tipId, Func<int, string> iconFor = null)
        {
            this.iconFor = iconFor;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.height = DesignTokens.ControlHeight;
            style.backgroundColor = DesignTokens.Bg2;
            this.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            this.Round(DesignTokens.Radius);
            this.Pad(0, DesignTokens.Space3);
            current = Ui.LabelText("");
            current.style.flexGrow = 1;
            Add(current);
            chevron = new IconElement("chevron-down", 18f);
            Add(chevron);
            Tooltips.Attach(this, tipId);
            RegisterCallback<PointerEnterEvent>(e => { hover = true; Refresh(); });
            RegisterCallback<PointerLeaveEvent>(e => { hover = false; Refresh(); });
            RegisterCallback<PointerDownEvent>(e => { if (e.button == 0) { Open(); e.StopPropagation(); } });
            RegisterCallback<DetachFromPanelEvent>(e => Close());
            RegisterCallback<GeometryChangedEvent>(e => { if (e.oldRect != e.newRect) Close(); });
            Refresh();
        }

        public void SetItems(IList<string> names)
        {
            items.Clear();
            items.AddRange(names);
            if (selected >= items.Count) selected = -1;
            Refresh();
        }

        public int Selected
        {
            get { return selected; }
            set { selected = value; Refresh(); }
        }

        void Refresh()
        {
            current.text = selected >= 0 && selected < items.Count ? items[selected] : "";
            style.backgroundColor = hover ? DesignTokens.Bg3 : DesignTokens.Bg2;
            chevron.Tint = hover ? DesignTokens.Text1 : DesignTokens.Text2;
        }

        static EvoDropdown openInstance;
        public static bool IsOpen { get { return openInstance != null; } }

        public static bool CloseOpen()
        {
            if (openInstance == null) return false;
            openInstance.Close();
            return true;
        }

        public void Close()
        {
            if (catcher != null) { catcher.RemoveFromHierarchy(); catcher = null; }
            if (openInstance == this) openInstance = null;
        }

        void Open()
        {
            if (catcher != null || Ui.Overlay == null) return;
            CloseOpen();
            openInstance = this;
            Tooltips.Hide();
            VisualElement overlay = Ui.Overlay;
            catcher = new VisualElement();
            catcher.Fill();
            catcher.RegisterCallback<PointerDownEvent>(e => { Close(); e.StopPropagation(); });
            overlay.Add(catcher);

            var list = new VisualElement();
            list.style.position = Position.Absolute;
            LiquidGlass.Attach(list, DesignTokens.RadiusCard, GlassTone.Modal);
            list.Pad(DesignTokens.Space1);
            Rect rect = new Rect(overlay.WorldToLocal(worldBound.position), worldBound.size);
            list.style.left = rect.x;
            list.style.top = rect.yMax + 6;
            list.style.width = Mathf.Max(rect.width, 180f);
            catcher.Add(list);

            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                bool isSel = index == selected;
                VisualElement row = Ui.Row();
                row.style.height = DesignTokens.ControlHeight - 2;
                row.Pad(0, DesignTokens.Space3);
                row.Round(DesignTokens.Radius - 2);
                row.style.backgroundColor = isSel ? DesignTokens.AccentSubtle : Color.clear;
                var check = new IconElement(isSel ? "check" : (iconFor != null && iconFor(index) != null ? iconFor(index) : "check"), 18f);
                check.Tint = isSel ? DesignTokens.AccentText : Color.clear;
                check.style.marginRight = DesignTokens.Space2;
                row.Add(check);
                row.Add(Ui.LabelText(items[i], isSel ? DesignTokens.AccentText : DesignTokens.Text1));
                row.RegisterCallback<PointerEnterEvent>(e => row.style.backgroundColor = isSel ? DesignTokens.AccentSubtle : DesignTokens.Bg3);
                row.RegisterCallback<PointerLeaveEvent>(e => row.style.backgroundColor = isSel ? DesignTokens.AccentSubtle : Color.clear);
                row.RegisterCallback<PointerDownEvent>(e =>
                {
                    selected = index;
                    Refresh();
                    Close();
                    if (Changed != null) Changed(index);
                    e.StopPropagation();
                });
                list.Add(row);
            }
        }
    }
}
