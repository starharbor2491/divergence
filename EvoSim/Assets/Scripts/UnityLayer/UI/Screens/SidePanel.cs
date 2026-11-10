using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The right-hand panel: four tabs (DESIGN 4.7) over scrolling content. Width is 30% of the window, 420 to 560 px.</summary>
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
            style.backgroundColor = DesignTokens.Bg1;
            style.borderLeftWidth = DesignTokens.BorderWidth;
            style.borderLeftColor = DesignTokens.Border;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Column;

            VisualElement row = Ui.Row();
            row.style.flexShrink = 0;
            row.style.borderBottomWidth = DesignTokens.BorderWidth;
            row.style.borderBottomColor = DesignTokens.Border;
            string[] names = { "Traits", "Species", "Tree", "Stats" };
            string[] ids = { "tab-traits", "tab-species", "tab-tree", "tab-stats" };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                tabButtons[i] = new EvoButton(ButtonKind.Tab, names[i], ids[i], ids[i]);
                tabButtons[i].style.flexGrow = 1;
                tabButtons[i].style.borderTopLeftRadius = tabButtons[i].style.borderTopRightRadius = 0;
                tabButtons[i].style.borderBottomLeftRadius = tabButtons[i].style.borderBottomRightRadius = 0;
                tabButtons[i].Clicked += () => Select(index);
                row.Add(tabButtons[i]);
            }
            Add(row);

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

        /// <summary>Redraws the open tab once per real second (DESIGN 5.1).</summary>
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
            style.width = Mathf.Clamp(windowWidth * 0.30f, DesignTokens.SidePanelMin, DesignTokens.SidePanelMax);
        }
    }
}
