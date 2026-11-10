using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Traits tab (PLAN 12.2): a Trait dropdown, a time or generation switch, the mean and spread chart, and the frequency chart.</summary>
    public sealed class TraitsTab : PanelTab
    {
        readonly EvoDropdown dropdown;
        readonly EvoButton timeButton, generationButton;
        readonly TraitLineChart lineChart = new TraitLineChart();
        readonly TraitFrequencyChart freqChart = new TraitFrequencyChart();
        readonly LegendRow[] legend = new LegendRow[TraitBins.Count];
        int trait;

        public TraitsTab(AppController app) : base(app)
        {
            VisualElement controls = Ui.Row();
            Label tl = Ui.LabelText("Trait");
            tl.pickingMode = PickingMode.Position;
            controls.Add(tl);

            dropdown = new EvoDropdown("trait-select");
            var names = new List<string>();
            for (int i = 0; i < Traits.ChartCount; i++) names.Add(Capitalise(Traits.Names[i]));
            dropdown.SetItems(names);
            dropdown.Selected = 0;
            dropdown.style.flexGrow = 1;
            dropdown.style.marginLeft = DesignTokens.Space3;
            dropdown.Changed += i => { trait = i; ApplyTrait(); };
            controls.Add(dropdown);

            VisualElement axis = Ui.Row();
            axis.style.marginLeft = DesignTokens.Space3;
            timeButton = new EvoButton(ButtonKind.Secondary, "time", null, "axis-switch");
            generationButton = new EvoButton(ButtonKind.Secondary, "generation", null, "axis-switch");
            timeButton.RoundEnds(true, false);
            generationButton.RoundEnds(false, true);
            timeButton.Selected = true;
            timeButton.Clicked += () => SetAxis(false);
            generationButton.Clicked += () => SetAxis(true);
            axis.Add(timeButton);
            axis.Add(generationButton);
            controls.Add(axis);
            Add(controls);
            Label definition = Ui.Caption("");
            definition.style.marginTop = DesignTokens.Space2;
            Add(definition);
            dropdown.Changed += i => definition.text = UiText.TraitTip(i).Body;
            definition.text = UiText.TraitTip(trait).Body;
            Tooltips.Attach(tl, () => UiText.TraitTip(trait));

            FixedHeight(lineChart, 210);
            Add(Section("Mean and spread", "", "chart-trait-mean", UiText.Body("chart-trait-mean"), lineChart));

            FixedHeight(freqChart, 210);
            VisualElement freq = Ui.Column();
            freq.Add(freqChart);
            VisualElement legendBox = Ui.Column();
            legendBox.style.marginTop = DesignTokens.Space2;
            for (int b = 0; b < TraitBins.Count; b++)
            {
                legend[b] = new LegendRow("legend-bin");
                legendBox.Add(legend[b]);
            }
            freq.Add(legendBox);
            Add(Section("Frequency", "share of Cells", "chart-trait-frequency", UiText.Body("chart-trait-frequency"), freq));
        }

        static string Capitalise(string s) { return s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1); }

        void SetAxis(bool generation)
        {
            timeButton.Selected = !generation;
            generationButton.Selected = generation;
            lineChart.ByGeneration = generation;
            freqChart.ByGeneration = generation;
        }

        void ApplyTrait()
        {
            lineChart.Trait = trait;
            freqChart.Trait = trait;
            RefreshLegend();
        }

        void RefreshLegend()
        {
            if (sim == null) return;
            for (int b = 0; b < TraitBins.Count; b++)
                legend[b].Set(DesignTokens.Bins[b], TraitFrequencyChart.BinRange(trait, b, sim.P), "");
        }

        public override void Bind(Simulation s)
        {
            sim = s;
            lineChart.Bind(s);
            freqChart.Bind(s);
            RefreshLegend();
        }

        public override void Refresh()
        {
            if (sim == null) return;
            lineChart.Refresh();
            freqChart.Refresh();
        }
    }
}
