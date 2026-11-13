using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Liquid Glass Help & Guide overlay with left navigation rail, readable prose, and Trait/Event reference cards.</summary>
    public sealed class HelpScreen : VisualElement
    {
        public const int MaxLineLength = 70;

        readonly Label[] navRows = new Label[UiText.HelpTitles.Length];
        readonly VisualElement[] navItems = new VisualElement[UiText.HelpTitles.Length];
        readonly ScrollView scroll = UiScroll.Create();
        readonly Label title, body;
        readonly VisualElement extras = Ui.Column();
        int selected;
        public bool IsOpen { get; private set; }

        public HelpScreen()
        {
            this.Fill();
            style.backgroundColor = DesignTokens.Scrim;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            this.Show(false);

            var card = new VisualElement();
            card.style.width = Length.Percent(82);
            card.style.maxWidth = DesignTokens.HelpWidth;
            card.style.height = Length.Percent(82);
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Modal);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement head = Ui.Row();
            var icon = new IconElement("glyph-branch", 26f) { Tint = DesignTokens.AccentText };
            icon.style.marginRight = DesignTokens.Space3;
            head.Add(icon);
            head.Add(Ui.Heading("How Divergence works", DesignTokens.Text1));
            head.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "close");
            close.Clicked += Close;
            head.Add(close);
            card.Add(head);

            VisualElement columns = Ui.Row(Align.Stretch);
            columns.style.flexGrow = 1;
            columns.style.marginTop = DesignTokens.Space4;
            columns.style.minHeight = 0;
            card.Add(columns);

            VisualElement nav = Ui.Column();
            nav.style.width = DesignTokens.HelpNavWidth;
            nav.style.flexShrink = 0;
            LiquidGlass.StyleWell(nav, DesignTokens.RadiusCard);
            nav.Pad(DesignTokens.Space2);
            for (int i = 0; i < navRows.Length; i++)
            {
                int index = i;
                var row = new VisualElement();
                row.style.height = DesignTokens.ControlHeight;
                row.style.justifyContent = Justify.Center;
                row.Pad(0, DesignTokens.Space3);
                row.Round(DesignTokens.Radius - 2);
                navRows[i] = Ui.LabelText(UiText.HelpTitles[i]);
                row.Add(navRows[i]);
                row.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (index != selected) row.style.backgroundColor = DesignTokens.Bg3;
                });
                row.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (index != selected) row.style.backgroundColor = Color.clear;
                });
                row.RegisterCallback<PointerDownEvent>(e => Select(index));
                navItems[i] = row;
                nav.Add(row);
            }
            columns.Add(nav);

            scroll.style.marginLeft = DesignTokens.Space5;
            columns.Add(scroll);
            title = Ui.Heading("", DesignTokens.Text1);
            body = Ui.Body("", DesignTokens.Text2);
            body.style.maxWidth = 620;
            body.style.marginTop = DesignTokens.Space3;
            body.style.marginRight = DesignTokens.Space4;
            extras.style.marginTop = DesignTokens.Space4;
            extras.style.marginRight = DesignTokens.Space4;
            extras.style.maxWidth = 620;
            scroll.Add(title);
            scroll.Add(body);
            scroll.Add(extras);
            Select(0);
        }

        void Select(int index)
        {
            selected = index;
            for (int i = 0; i < navItems.Length; i++)
            {
                bool active = i == index;
                navItems[i].style.backgroundColor = active ? DesignTokens.AccentSubtle : Color.clear;
                navItems[i].Border(active ? DesignTokens.WithAlpha(DesignTokens.Accent, 0.45f) : Color.clear, active ? 1 : 0);
                navRows[i].style.color = active ? DesignTokens.AccentText : DesignTokens.Text2;
            }
            title.text = UiText.HelpTitles[index];
            body.text = UiText.HelpBodies[index];
            BuildExtras(index);
            scroll.scrollOffset = Vector2.zero;
        }

        void BuildExtras(int index)
        {
            extras.Clear();
            string section = UiText.HelpTitles[index];
            if (section == "Traits")
            {
                for (int t = 0; t < UiText.TraitIds.Length; t++)
                {
                    TipContent tip = UiText.TraitTip(t);
                    var item = new VisualElement();
                    LiquidGlass.StyleWell(item, DesignTokens.Radius);
                    item.Pad(DesignTokens.Space3);
                    if (t > 0) item.style.marginTop = DesignTokens.Space2;
                    item.Add(Ui.Subhead(tip.Name, DesignTokens.AccentText));
                    Label desc = Ui.Caption(tip.Body, DesignTokens.Text2);
                    desc.style.marginTop = 3;
                    item.Add(desc);
                    extras.Add(item);
                }
            }
            else if (section == "Events")
            {
                for (int k = 0; k < EventInfo.Ids.Length; k++)
                {
                    Color ec = EventsMenu.AccentFor((EventKind)k);
                    var item = Ui.Row();
                    LiquidGlass.StyleWell(item, DesignTokens.Radius);
                    item.Pad(DesignTokens.Space3);
                    if (k > 0) item.style.marginTop = DesignTokens.Space2;
                    var ic = new IconElement(EventInfo.Ids[k], 20f) { Tint = ec };
                    ic.style.marginRight = DesignTokens.Space3;
                    item.Add(ic);
                    VisualElement col = Ui.Column();
                    col.style.flexShrink = 1;
                    col.Add(Ui.Subhead(EventInfo.Names[k], DesignTokens.Text1));
                    col.Add(Ui.Caption(UiText.Body("event-" + EventInfo.Ids[k]), DesignTokens.Text2));
                    item.Add(col);
                    extras.Add(item);
                }
            }
        }

        public void Open()
        {
            Tooltips.Hide();
            IsOpen = true;
            this.Show(true);
        }

        public void Close()
        {
            IsOpen = false;
            Tooltips.Hide();
            this.Show(false);
        }
    }
}
