using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>Tree tab (PLAN 12.2): the family tree of Species. Clicking a line highlights that Species' Cells.</summary>
    public sealed class TreeTab : PanelTab
    {
        readonly TreeChart chart = new TreeChart();

        public TreeTab(AppController app) : base(app)
        {
            FixedHeight(chart, 460);
            chart.SpeciesClicked += id => app.ToggleHighlight(id);
            Add(Section("Family tree", "time", "chart-tree", UiText.Body("chart-tree"), chart));
        }

        public override void Bind(Simulation s)
        {
            sim = s;
            chart.Bind(s);
        }

        public override void Refresh()
        {
            if (sim != null) chart.Refresh();
        }
    }
}
