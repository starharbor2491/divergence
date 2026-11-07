using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The Help screen (DESIGN 6.5): a section list on the left and the section text on the right, over the scrim.</summary>
    public sealed class HelpScreen : VisualElement
    {
        public const int MaxLineLength = 70;

        readonly Label[] navRows = new Label[UiText.HelpTitles.Length];
        readonly VisualElement[] navItems = new VisualElement[UiText.HelpTitles.Length];
        readonly ScrollView scroll = UiScroll.Create();
        readonly Label title, body;
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
            card.style.width = Length.Percent(80);
            card.style.maxWidth = DesignTokens.HelpWidth;
            card.style.height = Length.Percent(80);
            card.style.backgroundColor = DesignTokens.Bg1;
            card.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            card.Round(DesignTokens.Radius);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement head = Ui.Row();
            head.Add(Ui.Heading("How it works", DesignTokens.Text1));
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
            for (int i = 0; i < navRows.Length; i++)
            {
                int index = i;
                var row = new VisualElement();
                row.style.height = DesignTokens.ControlHeight;
                row.style.justifyContent = Justify.Center;
                row.Pad(0, DesignTokens.Space3);
                row.Round(DesignTokens.Radius);
                navRows[i] = Ui.LabelText(UiText.HelpTitles[i]);
                row.Add(navRows[i]);
                row.RegisterCallback<PointerEnterEvent>(e => row.style.backgroundColor = DesignTokens.Bg3);
                row.RegisterCallback<PointerLeaveEvent>(e => row.style.backgroundColor = index == selected ? DesignTokens.Bg3 : Color.clear);
                row.RegisterCallback<PointerDownEvent>(e => Select(index));
                navItems[i] = row;
                nav.Add(row);
            }
            columns.Add(nav);

            scroll.style.marginLeft = DesignTokens.Space5;
            columns.Add(scroll);
            title = Ui.Title("");
            body = Ui.Body("");
            body.style.whiteSpace = WhiteSpace.NoWrap;
            body.style.marginTop = DesignTokens.Space3;
            body.style.marginRight = DesignTokens.Space4;
            scroll.Add(title);
            scroll.Add(body);
            Select(0);
        }

        void Select(int index)
        {
            selected = index;
            for (int i = 0; i < navItems.Length; i++) navItems[i].style.backgroundColor = i == index ? DesignTokens.Bg3 : Color.clear;
            title.text = UiText.HelpTitles[index];
            body.text = UiText.Wrap(UiText.HelpBodies[index], MaxLineLength);
            scroll.scrollOffset = Vector2.zero;
        }

        public void Open()
        {
            Tooltips.Hide();
            IsOpen = true;
            this.Show(true);
            BringToFront();
        }

        public void Close()
        {
            IsOpen = false;
            Tooltips.Hide();
            this.Show(false);
        }
    }
}
