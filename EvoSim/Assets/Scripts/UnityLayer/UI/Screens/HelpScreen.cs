using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
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
            card.style.width = DesignTokens.HelpWidth;
            card.style.height = 540;
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Modal);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement head = Ui.Row();
            head.Add(Ui.Heading("Reference guide", DesignTokens.Text1));
            head.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "close");
            close.Clicked += Close;
            head.Add(close);
            card.Add(head);

            VisualElement div = Ui.Divider(false);
            div.style.marginTop = DesignTokens.Space3;
            card.Add(div);

            VisualElement columns = Ui.Row(Align.Stretch);
            columns.style.flexGrow = 1;
            columns.style.marginTop = DesignTokens.Space4;
            columns.style.minHeight = 0;
            card.Add(columns);

            VisualElement nav = Ui.Column();
            nav.style.width = DesignTokens.HelpNavWidth;
            nav.style.flexShrink = 0;
            nav.style.borderRightWidth = 1;
            nav.style.borderRightColor = DesignTokens.Border;
            nav.style.paddingRight = DesignTokens.Space3;
            for (int i = 0; i < navRows.Length; i++)
            {
                int index = i;
                var row = new VisualElement();
                row.style.height = 34;
                row.style.justifyContent = Justify.Center;
                row.Pad(0, DesignTokens.Space3);
                row.Round(DesignTokens.Radius);
                navRows[i] = Ui.LabelText(UiText.HelpTitles[i]);
                row.Add(navRows[i]);
                row.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (index != selected) row.style.backgroundColor = DesignTokens.Bg2;
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
            body.style.maxWidth = 540;
            body.style.marginTop = DesignTokens.Space2;
            body.style.marginRight = DesignTokens.Space3;
            extras.style.marginTop = DesignTokens.Space4;
            extras.style.marginRight = DesignTokens.Space3;
            extras.style.maxWidth = 540;
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
                navItems[i].style.backgroundColor = active ? DesignTokens.ActivePill : Color.clear;
                navRows[i].style.color = active ? DesignTokens.Text1 : DesignTokens.Text2;
            }
            title.text = UiText.HelpTitles[index];
            body.text = UiText.HelpBodies[index];
            BuildExtras(index);
            scroll.scrollOffset = Vector2.zero;
        }

        void AddKeyRow(string label, string detail)
        {
            VisualElement r = Ui.Row(Align.FlexStart);
            r.style.paddingTop = 8;
            r.style.paddingBottom = 8;
            r.style.borderBottomWidth = 1;
            r.style.borderBottomColor = DesignTokens.Border;
            Label k = Ui.LabelText(label, DesignTokens.Text1);
            k.style.width = 160;
            k.style.flexShrink = 0;
            r.Add(k);
            Label v = Ui.Caption(detail, DesignTokens.Text2);
            v.style.flexGrow = 1;
            v.style.flexShrink = 1;
            r.Add(v);
            extras.Add(r);
        }

        void BuildExtras(int index)
        {
            extras.Clear();
            switch (index)
            {
                case 0:
                    AddKeyRow("1. Variation", "Offspring inherit their parents' traits and brain weights with small random mutations.");
                    AddKeyRow("2. Selection", "Food scarcity, temperature stress, toxin drain, and predation remove poorly suited cells.");
                    AddKeyRow("3. Divergence", "Surviving lineages shift population averages and eventually split into separate species.");
                    AddKeyRow("Try an experiment", "Run Cold gradient at 5x speed, trigger a Heat wave on the cold side, and watch Preferred temperature climb.");
                    break;
                case 1:
                    AddKeyRow("Drag / Scroll", "Pan across the water and zoom smoothly around the cursor.");
                    AddKeyRow("Click a cell", "Opens the cell inspector and draws its sense radius in the water.");
                    AddKeyRow("Space", "Pause or resume simulation steps.");
                    AddKeyRow("1 · 2 · 3 · 4", "Switch simulation speed between 1x, 2x, 5x, and 10x.");
                    AddKeyRow("T / F / Tab", "Toggle thermal overlay (T), reset camera framing (F), or hide charts (Tab).");
                    AddKeyRow("Escape", "Cancel event placement, close dialogs, or deselect the inspected cell.");
                    break;
                case 2:
                    AddKeyRow("Mild world", "Moderate warmth and steady food—good baseline for observing gradual drift.");
                    AddKeyRow("Cold gradient", "Strong north–south thermal range that encourages regional adaptation.");
                    AddKeyRow("Toxic basin", "Heavy starting toxin clouds that reward higher toxin resistance.");
                    AddKeyRow("Rapid evolution", "Rich food, strong thermal contrast, and 2x mutation rate for fast speciation.");
                    break;
                case 3:
                    AddKeyRow("Temperature", "Continuous blue-to-red field (0.00 to 1.00). Mismatches beyond tolerance drain energy.");
                    AddKeyRow("Food", "Green pellets that replenish faster in fertile pockets; eaten on contact.");
                    AddKeyRow("Toxin", "Drifting pink particles that cluster gently, decay over time, and drain nearby cells.");
                    break;
                case 4:
                    for (int t = 0; t < UiText.TraitIds.Length; t++)
                    {
                        TipContent tip = UiText.TraitTip(t);
                        AddKeyRow(tip.Name, tip.Body);
                    }
                    break;
                case 5:
                    AddKeyRow("Asexual (< 0.50)", "Divides alone once energy crosses the reproduction threshold, passing 40% to the child.");
                    AddKeyRow("Sexual (≥ 0.50)", "Requires a mutually compatible partner within sense range; mixes traits from both parents.");
                    AddKeyRow("Choosiness", "Higher choosiness restricts mating to genetically closer neighbours.");
                    break;
                case 6:
                    AddKeyRow("Membrane colour", "Projected from all 8 physical traits and 100 brain weights—closer colours mean closer genomes.");
                    AddKeyRow("Green core", "Low-diet grazer specialized in eating food pellets.");
                    AddKeyRow("Amber core & jaws", "High-diet hunter capable of biting and eating smaller cells.");
                    AddKeyRow("Speciation", "Checked every 2 simulated seconds; groups exceeding genetic distance split into a new species.");
                    break;
                case 7:
                    AddKeyRow("Traits", "Mean line ± 1 SD band (with dashed founder baseline) plus 6-band value distribution.");
                    AddKeyRow("Species", "Stacked population counts; click any living species below to spotlight its cells.");
                    AddKeyRow("Tree", "Evolutionary branching diagram weighted by population size.");
                    AddKeyRow("Stats", "Live birth/death rates and stacked 10-second mortality causes.");
                    break;
                case 8:
                    for (int k = 0; k < EventInfo.Ids.Length; k++)
                        AddKeyRow(EventInfo.Names[k], UiText.Body("event-" + EventInfo.Ids[k]));
                    break;
                case 9:
                    AddKeyRow("Save / Load JSON", "Stores exact RNG state, cells, species, and history so a loaded run continues identically.");
                    AddKeyRow("Export CSV", "Downloads trait time-series and species population tables for external analysis.");
                    AddKeyRow("Save PNG", "Captures the current viewport and charts as an image.");
                    break;
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
