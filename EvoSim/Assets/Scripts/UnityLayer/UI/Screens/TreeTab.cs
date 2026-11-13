using Evo.Core;

namespace Evo.UnityLayer
{
    public sealed class TreeTab : PanelTab
    {
        readonly TreeChart chart = new TreeChart();

        public TreeTab(AppController app) : base(app)
        {
            var sec = Section("Lineage tree", "time", "chart-tree", "Branches show speciation splits; line thickness reflects population size.", chart);
            sec.style.marginTop = DesignTokens.Space1;
            FixedHeight(chart, 460);
            chart.SpeciesClicked += id => app.ToggleHighlight(id);
            Add(sec);
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
