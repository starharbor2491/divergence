using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Floating right-hand Liquid Glass charts panel with a segmented pill tab switcher over scrolling charts.</summary>
    public sealed class SidePanel : VisualElement
    {
        readonly AppController app;
        readonly EvoButton[] tabButtons = new EvoButton[4];
        readonly PanelTab[] tabs = new PanelTab[4];
        readonly ScrollView scroll = UiScroll.Create();
        int active;
        float sinceRefresh;
        Simulation bound;

        public SidePanel(AppController app)
        {
            this.app = app;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Column;
            this.Margin(DesignTokens.Space3, DesignTokens.Space3, DesignTokens.Space3, 0);
            LiquidGlass.Attach(this, DesignTokens.RadiusPanel, GlassTone.Panel);

            // Segmented pill switcher
            VisualElement barWrap = Ui.Column();
            barWrap.Pad(DesignTokens.Space3, DesignTokens.Space3, DesignTokens.Space2, DesignTokens.Space3);
            barWrap.style.flexShrink = 0;

            VisualElement row = Ui.Row();
            row.style.backgroundColor = DesignTokens.GlassInset;
            row.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            row.Round(DesignTokens.Radius + 2);
            row.Pad(3);
            string[] names = { "Traits", "Species", "Tree", "Stats" };
            string[] ids = { "tab-traits", "tab-species", "tab-tree", "tab-stats" };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                tabButtons[i] = new EvoButton(ButtonKind.Tab, names[i], ids[i], ids[i]);
                tabButtons[i].style.flexGrow = 1;
                tabButtons[i].style.height = DesignTokens.ControlHeight - 4;
                tabButtons[i].Pad(0, DesignTokens.Space2);
                tabButtons[i].Round(DesignTokens.Radius - 1);
                tabButtons[i].Clicked += () => Select(index);
                row.Add(tabButtons[i]);
            }
            barWrap.Add(row);
            Add(barWrap);

            tabs[0] = new TraitsTab(app);
            tabs[1] = new SpeciesTab(app);
            tabs[2] = new TreeTab(app);
            tabs[3] = new StatsTab(app);
            Add(scroll);
            Select(0);
        }

        public void Select(int index)
        {
            EvoDropdown.CloseOpen();
            ChartElement.HideReadouts();
            active = index;
            for (int i = 0; i < 4; i++) tabButtons[i].Selected = i == index;
            scroll.Clear();
            scroll.Add(tabs[index]);
            scroll.scrollOffset = Vector2.zero;
            sinceRefresh = 0f;
            if (bound != null) tabs[index].Refresh();
        }

        public void BindRun(Simulation sim)
        {
            bound = sim;
            for (int i = 0; i < 4; i++) tabs[i].Bind(sim);
        }

        public void Tick(float realDt)
        {
            if (bound == null) return;
            sinceRefresh += realDt;
            if (sinceRefresh < 1f) return;
            sinceRefresh = 0f;
            tabs[active].Refresh();
        }

        public void SetWidthFor(float windowWidth)
        {
            style.width = Mathf.Clamp(windowWidth * 0.28f, DesignTokens.SidePanelMin, DesignTokens.SidePanelMax);
        }
    }
}
