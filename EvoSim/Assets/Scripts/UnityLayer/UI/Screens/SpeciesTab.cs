using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Species tab (PLAN 12.2): stacked population chart and a legend of living Species. Clicking a row highlights its Cells.</summary>
    public sealed class SpeciesTab : PanelTab
    {
        readonly SpeciesChart chart = new SpeciesChart();
        readonly VisualElement legendBox = Ui.Column();
        readonly List<LegendRow> rows = new List<LegendRow>();
        readonly List<SpeciesRecord> living = new List<SpeciesRecord>();
        readonly LegendRow other = new LegendRow("legend-other");

        public SpeciesTab(AppController app) : base(app)
        {
            FixedHeight(chart, 260);
            Add(Section("Species over time", "Cells", "chart-species", UiText.Body("chart-species"), chart));
            Add(Ui.Caption("Top 20 Species by peak population; the rest share Other."));
            Add(other);
            Label head = Ui.LabelText("Living Species");
            head.style.marginTop = DesignTokens.Space5;
            head.style.marginBottom = DesignTokens.Space2;
            Add(head);
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
