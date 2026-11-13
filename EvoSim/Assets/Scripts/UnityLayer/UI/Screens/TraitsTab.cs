using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Traits tab: Trait selector, Time / Generation pill switch, Trait trade-off summary, mean/spread chart, and frequency bins.</summary>
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
            dropdown.style.marginLeft = DesignTokens.Space2;
            dropdown.Changed += i => { trait = i; ApplyTrait(); };
            controls.Add(dropdown);

            VisualElement axis = Ui.Row();
            axis.style.marginLeft = DesignTokens.Space2;
            axis.style.backgroundColor = DesignTokens.GlassInset;
            axis.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            axis.Round(DesignTokens.Radius);
            axis.Pad(2);
            timeButton = new EvoButton(ButtonKind.Tab, "Time", null, "axis-switch");
            generationButton = new EvoButton(ButtonKind.Tab, "Gen", null, "axis-switch");
            timeButton.style.height = DesignTokens.ControlHeight - 4;
            generationButton.style.height = DesignTokens.ControlHeight - 4;
            timeButton.Pad(0, 10);
            generationButton.Pad(0, 10);
            timeButton.Round(DesignTokens.Radius - 3);
            generationButton.Round(DesignTokens.Radius - 3);
            timeButton.Selected = true;
            timeButton.Clicked += () => SetAxis(false);
            generationButton.Clicked += () => SetAxis(true);
            axis.Add(timeButton);
            axis.Add(generationButton);
            controls.Add(axis);
            Add(controls);

            VisualElement defCard = new VisualElement();
            LiquidGlass.StyleWell(defCard, DesignTokens.Radius);
            defCard.Pad(DesignTokens.Space2, DesignTokens.Space3);
            defCard.style.marginTop = DesignTokens.Space2;
            Label definition = Ui.Caption("", DesignTokens.Text2);
            defCard.Add(definition);
            Add(defCard);

            dropdown.Changed += i => definition.text = UiText.TraitTip(i).Body;
            definition.text = UiText.TraitTip(trait).Body;
            Tooltips.Attach(tl, () => UiText.TraitTip(trait));

            FixedHeight(lineChart, 205);
            Add(Section("Mean and spread", "dashed = Founder start", "chart-trait-mean", UiText.Body("chart-trait-mean"), lineChart));

            FixedHeight(freqChart, 195);
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
            Add(Section("Trait frequency", "share of living Cells", "chart-trait-frequency", UiText.Body("chart-trait-frequency"), freq));
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
