using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Species tab: stacked population chart and interactive living Species list (click to spotlight in World).</summary>
    public sealed class SpeciesTab : PanelTab
    {
        readonly SpeciesChart chart = new SpeciesChart();
        readonly VisualElement legendBox = Ui.Column();
        readonly List<LegendRow> rows = new List<LegendRow>();
        readonly List<SpeciesRecord> living = new List<SpeciesRecord>();
        readonly LegendRow other = new LegendRow("legend-other");

        public SpeciesTab(AppController app) : base(app)
        {
            var chartSection = Section("Species over time", "Cells", "chart-species", UiText.Body("chart-species"), chart);
            chartSection.style.marginTop = DesignTokens.Space1;
            FixedHeight(chart, 240);
            Add(chartSection);

            Label topNote = Ui.Caption("Top 20 Species by peak population; smaller lineages share Other.");
            topNote.style.marginTop = DesignTokens.Space2;
            Add(topNote);
            Add(other);

            VisualElement headRow = Ui.Row();
            headRow.style.marginTop = DesignTokens.Space4;
            headRow.style.marginBottom = DesignTokens.Space1;
            headRow.Add(Ui.Subhead("Living Species", DesignTokens.Text1));
            headRow.Add(Ui.Spacer());
            headRow.Add(Ui.Caption("Click to spotlight", DesignTokens.AccentText));
            Add(headRow);
            Add(legendBox);
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
            chart.Refresh();
            other.Show(chart.HasOther);
            other.Set(DesignTokens.Text3, "Other (chart)", Ui.Count(chart.OtherPopulation));
            living.Clear();
            foreach (SpeciesRecord r in sim.Species.Records) if (!r.Extinct) living.Add(r);
            living.Sort((a, b) => b.CurrentPopulation.CompareTo(a.CurrentPopulation));
            while (rows.Count < living.Count)
            {
                var row = new LegendRow("legend-species");
                row.Clicked += () => { if (row.Tag != 0) app.ToggleHighlight(row.Tag); Refresh(); };
                rows.Add(row);
                legendBox.Add(row);
            }
            for (int i = 0; i < rows.Count; i++)
            {
                if (i >= living.Count) { rows[i].Show(false); continue; }
                SpeciesRecord r = living[i];
                rows[i].Show(true);
                rows[i].Tag = r.Id;
                rows[i].Set(new Color(r.ColorR, r.ColorG, r.ColorB), r.Name + (chart.IsAggregated(r.Id) ? " (in Other)" : ""), Ui.Count(r.CurrentPopulation), app.WorldView.HighlightSpeciesId == r.Id);
            }
        }
    }
}
