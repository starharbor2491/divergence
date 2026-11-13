using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Stats tab: bento grid of live ecosystem figures plus stacked Deaths by cause chart.</summary>
    public sealed class StatsTab : PanelTab
    {
        readonly Label alive, births, deaths, maxGen, living, extinct, sexual;
        readonly DeathsChart chart = new DeathsChart();
        readonly LegendRow[] legend = new LegendRow[DeathCauses.Count];

        public StatsTab(AppController app) : base(app)
        {
            VisualElement r1 = Ui.Row();
            r1.style.marginTop = DesignTokens.Space1;
            alive = StatCard(r1, "Cells alive", "stat-readout", false, true);
            living = StatCard(r1, "Living Species", "stat-readout", true, true);
            maxGen = StatCard(r1, "Highest Gen", "stat-readout", true, false);
            Add(r1);

            VisualElement r2 = Ui.Row();
            r2.style.marginTop = DesignTokens.Space2;
            births = StatCard(r2, "Births / min", "stat-readout", false, false);
            deaths = StatCard(r2, "Deaths / min", "stat-readout", true, false);
            extinct = StatCard(r2, "Extinct Species", "stat-readout", true, false);
            Add(r2);

            VisualElement sexRow = Ui.Row();
            sexRow.style.marginTop = DesignTokens.Space2;
            LiquidGlass.StyleWell(sexRow, DesignTokens.Radius);
            sexRow.Pad(DesignTokens.Space2, DesignTokens.Space3);
            sexRow.pickingMode = PickingMode.Position;
            Tooltips.Attach(sexRow, "stat-sexual-share");
            sexRow.Add(Ui.Caption("Cells using sexual reproduction", DesignTokens.Text2));
            sexRow.Add(Ui.Spacer());
            sexual = Ui.Mono("", DesignTokens.AccentText, DesignTokens.TypeBodySize);
            sexRow.Add(sexual);
            Add(sexRow);

            FixedHeight(chart, 210);
            VisualElement deathsBox = Ui.Column();
            deathsBox.Add(chart);
            Label sub = Ui.Caption("Bars show latest 10 minutes. Legend counts the whole Run.");
            sub.style.marginTop = DesignTokens.Space2;
            deathsBox.Add(sub);
            VisualElement legendBox = Ui.Column();
            legendBox.style.marginTop = DesignTokens.Space1;
            for (int c = 0; c < DeathCauses.Count; c++)
            {
                legend[c] = new LegendRow("legend-cause");
                legendBox.Add(legend[c]);
            }
            deathsBox.Add(legendBox);
            Add(Section("Deaths by cause", "Cells per 10 s", "chart-deaths", UiText.Body("chart-deaths"), deathsBox));
        }

        static Label StatCard(VisualElement parent, string name, string tipId, bool gap, bool accent)
        {
            VisualElement card = Ui.Column();
            card.style.flexGrow = 1;
            card.style.flexBasis = 0;
            if (gap) card.style.marginLeft = DesignTokens.Space2;
            LiquidGlass.StyleWell(card, DesignTokens.Radius);
            card.Pad(DesignTokens.Space2, DesignTokens.Space3);
            card.pickingMode = PickingMode.Position;
            Tooltips.Attach(card, tipId);
            card.Add(Ui.Caption(name, DesignTokens.Text3));
            Label v = Ui.Mono("", accent ? DesignTokens.AccentText : DesignTokens.Text1, DesignTokens.TypeTitleSize);
            v.style.marginTop = 2;
            card.Add(v);
            parent.Add(card);
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
