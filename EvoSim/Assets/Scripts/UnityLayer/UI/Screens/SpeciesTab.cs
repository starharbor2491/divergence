using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class SpeciesTab : PanelTab
    {
        readonly SpeciesChart chart = new SpeciesChart();
        readonly VisualElement legendBox = Ui.Column();
        readonly List<LegendRow> rows = new List<LegendRow>();
        readonly List<SpeciesRecord> living = new List<SpeciesRecord>();
        readonly Dictionary<int, int> liveCounts = new Dictionary<int, int>();
        readonly LegendRow other = new LegendRow("legend-other");

        public SpeciesTab(AppController app) : base(app)
        {
            var chartSection = Section("Species populations", "cells", "chart-species", "Top 20 lineages by historical peak; smaller groups combine into Other.", chart);
            chartSection.style.marginTop = DesignTokens.Space1;
            FixedHeight(chart, 236);
            Add(chartSection);

            VisualElement headRow = Ui.Row();
            headRow.style.marginTop = DesignTokens.Space4;
            headRow.style.marginBottom = DesignTokens.Space1;
            headRow.Add(Ui.Subhead("Living species", DesignTokens.Text1));
            headRow.Add(Ui.Spacer());
            headRow.Add(Ui.Caption("Click to spotlight", DesignTokens.Text3));
            Add(headRow);
            Add(legendBox);
            other.style.marginTop = DesignTokens.Space1;
            Add(other);
        }

        public override void Bind(Simulation s)
        {
            sim = s;
            chart.Bind(s);
            Refresh();
        }

        int LiveCountFor(SpeciesRecord r)
        {
            int c;
            return liveCounts.TryGetValue(r.Id, out c) ? c : r.CurrentPopulation;
        }

        public override void Refresh()
        {
            if (sim == null) return;
            chart.Refresh();
            other.Show(chart.HasOther);
            other.Set(DesignTokens.Text3, "Other (combined)", Ui.Count(chart.OtherPopulation));

            liveCounts.Clear();
            if (sim.Cells.AliveCount > 0)
            {
                for (int i = 0; i < sim.Cells.AliveCount; i++)
                {
                    int sid = sim.Cells.Alive[i].SpeciesId;
                    int prev;
                    liveCounts[sid] = (liveCounts.TryGetValue(sid, out prev) ? prev : 0) + 1;
                }
            }

            living.Clear();
            foreach (SpeciesRecord r in sim.Species.Records)
                if (!r.Extinct && (sim.Cells.AliveCount == 0 || LiveCountFor(r) > 0)) living.Add(r);
            living.Sort((a, b) => LiveCountFor(b).CompareTo(LiveCountFor(a)));
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
                rows[i].Set(new Color(r.ColorR, r.ColorG, r.ColorB),
                    r.Name + (chart.IsAggregated(r.Id) ? " (in Other)" : ""),
                    Ui.Count(LiveCountFor(r)),
                    app.WorldView.HighlightSpeciesId == r.Id);
            }
        }
    }
}
