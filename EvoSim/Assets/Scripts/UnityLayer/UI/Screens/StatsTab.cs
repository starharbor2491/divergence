using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class StatsTab : PanelTab
    {
        readonly Label alive, births, deaths, maxGen, living, extinct, sexual;
        readonly DeathsChart chart = new DeathsChart();
        readonly LegendRow[] legend = new LegendRow[DeathCauses.Count];

        public StatsTab(AppController app) : base(app)
        {
            VisualElement table = Ui.Column();
            table.style.marginTop = DesignTokens.Space1;
            alive = StatLine(table, "Cells alive", "stat-readout", true);
            living = StatLine(table, "Living species", "stat-readout", false);
            extinct = StatLine(table, "Extinct species", "stat-readout", false);
            maxGen = StatLine(table, "Highest generation", "stat-readout", false);
            births = StatLine(table, "Births / min", "stat-readout", false);
            deaths = StatLine(table, "Deaths / min", "stat-readout", false);
            sexual = StatLine(table, "Sexual share", "stat-sexual-share", false, false);
            Add(table);

            FixedHeight(chart, 210);
            VisualElement deathsBox = Ui.Column();
            deathsBox.Add(chart);
            VisualElement legendBox = Ui.Column();
            legendBox.style.marginTop = DesignTokens.Space2;
            for (int c = 0; c < DeathCauses.Count; c++)
            {
                legend[c] = new LegendRow("legend-cause");
                legendBox.Add(legend[c]);
            }
            deathsBox.Add(legendBox);
            Add(Section("Causes of death", "per 10 s", "chart-deaths", "Bars show the latest 10 minutes; legend numbers show cumulative totals.", deathsBox));
        }

        static Label StatLine(VisualElement parent, string name, string tipId, bool accent, bool divider = true)
        {
            VisualElement row = Ui.Row();
            row.style.height = 30;
            if (divider)
            {
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = DesignTokens.Border;
            }
            Label n = Ui.Body(name, DesignTokens.Text2);
            n.pickingMode = PickingMode.Position;
            Tooltips.Attach(n, tipId);
            n.style.flexGrow = 1;
            row.Add(n);
            Label v = Ui.Mono("", accent ? DesignTokens.Text1 : DesignTokens.Text1, DesignTokens.TypeBodySize);
            v.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(v);
            parent.Add(row);
            return v;
        }

        public override void Bind(Simulation s)
        {
            sim = s;
            chart.Bind(s);
            Refresh();
        }

        public override void Refresh()
        {
            if (sim == null) return;
            CellPool pool = sim.Cells;
            int maxGeneration = 0, sexualCount = 0;
            for (int i = 0; i < pool.AliveCount; i++)
            {
                Cell c = pool.Alive[i];
                if (c.Generation > maxGeneration) maxGeneration = c.Generation;
                if (c.G.Trait[TraitId.SexualTendency] >= sim.P.SexThreshold) sexualCount++;
            }
            alive.text = Ui.Count(pool.AliveCount);
            births.text = Ui.F(sim.Stats.BirthsPerMinute, 0);
            deaths.text = Ui.F(sim.Stats.DeathsPerMinute, 0);
            maxGen.text = Ui.Count(maxGeneration);
            living.text = Ui.Count(sim.Species.LivingCount);
            extinct.text = Ui.Count(sim.Species.ExtinctCount);
            sexual.text = (pool.AliveCount == 0 ? 0 : Mathf.RoundToInt(100f * sexualCount / pool.AliveCount)) + "%";
            chart.Refresh();
            for (int c = 0; c < DeathCauses.Count; c++)
                legend[c].Set(DesignTokens.Causes[c], DeathsChart.CauseNames[c], Ui.Count(sim.Stats.TotalDeaths[c]));
        }
    }
}
