using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Stats tab (PLAN 12.2, DESIGN 5.7): live counts in two columns, then deaths by cause.</summary>
    public sealed class StatsTab : PanelTab
    {
        readonly Label alive, births, deaths, maxGen, living, extinct, sexual;
        readonly DeathsChart chart = new DeathsChart();
        readonly LegendRow[] legend = new LegendRow[DeathCauses.Count];

        public StatsTab(AppController app) : base(app)
        {
            VisualElement grid = Ui.Column();
            grid.style.marginTop = DesignTokens.Space2;
            alive = AddRow(grid, "cells alive", "stat-readout");
            births = AddRow(grid, "births per minute", "stat-readout");
            deaths = AddRow(grid, "deaths per minute", "stat-readout");
            maxGen = AddRow(grid, "highest generation", "stat-readout");
            living = AddRow(grid, "living species", "stat-readout");
            extinct = AddRow(grid, "extinct species", "stat-readout");
            sexual = AddRow(grid, "sexual share", "stat-readout");
            Add(grid);

            FixedHeight(chart, 220);
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
            Add(Section("Deaths by cause", "Cells per 10 s", "chart-deaths", UiText.Body("chart-deaths"), deathsBox));
        }

        Label AddRow(VisualElement parent, string name, string tipId)
        {
            VisualElement row = Ui.Row();
            row.style.height = 28;
            Label n = Ui.Caption(name);
            n.pickingMode = PickingMode.Position;
            Tooltips.Attach(n, tipId);
            n.style.flexGrow = 1;
            row.Add(n);
            Label v = Ui.Body("");
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
                legend[c].Set(DesignTokens.Causes[c], DeathCauses.Names[c], Ui.Count(sim.Stats.TotalDeaths[c]));
        }
    }
}
