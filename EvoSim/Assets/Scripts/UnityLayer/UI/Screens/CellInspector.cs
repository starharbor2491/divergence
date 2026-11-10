using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The Cell inspector card (DESIGN 4.16) at the top left of the World view.</summary>
    public sealed class CellInspector : VisualElement
    {
        // The 280 px card leaves 248 px inside, so labels are shortened to fit beside the bar.
        const float LabelWidth = 124f;
        const float BarWidth = 76f;
        static readonly string[] ShortNames =
        {
            "Size", "Speed", "Sense range", "Preferred temp", "Temp tolerance", "Toxin resist", "Diet", "Repro threshold"
        };

        readonly AppController app;
        readonly VisualElement swatch;
        readonly Label speciesName, energy, age, generation, parentValue;
        readonly VisualElement[] traitFill = new VisualElement[Traits.ChartCount];
        readonly Label[] traitValue = new Label[Traits.ChartCount];
        readonly VisualElement[] outFill = new VisualElement[4];
        readonly VisualElement outMateRow;
        float sinceUpdate = 99f;
        int shownId = WorldView.NoCell;

        public CellInspector(AppController app)
        {
            this.app = app;
            style.position = Position.Absolute;
            style.left = DesignTokens.Space4;
            style.top = DesignTokens.Space4;
            style.width = DesignTokens.InspectorWidth;
            style.backgroundColor = DesignTokens.Bg2;
            this.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            this.Round(DesignTokens.Radius);
            this.Pad(DesignTokens.Space4);
            this.Show(false);

            VisualElement head = Ui.Row();
            head.Add(Ui.Title("Cell"));
            head.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "inspector-close");
            close.Clicked += () => app.ClearSelection();
            head.Add(close);
            Add(head);

            VisualElement sp = Ui.Row();
            sp.style.marginTop = DesignTokens.Space1;
            swatch = new VisualElement();
            swatch.Size(12, 12);
            swatch.Round(DesignTokens.RadiusCheckbox);
            sp.Add(swatch);
            speciesName = Ui.Body("");
            speciesName.style.marginLeft = DesignTokens.Space2;
            sp.Add(speciesName);
            Add(sp);

            VisualElement rows = Ui.Column();
            rows.style.marginTop = DesignTokens.Space3;
            energy = Readout(rows, "energy", "inspect-energy");
            age = Readout(rows, "age", "inspect-age");
            generation = Readout(rows, "generation", "inspect-generation");
            parentValue = Readout(rows, "parent", "inspect-parent");
            Add(rows);

            VisualElement bars = Ui.Column();
            bars.style.marginTop = DesignTokens.Space3;
            for (int i = 0; i < Traits.ChartCount; i++)
            {
                VisualElement row = Ui.Row();
                row.style.height = 24;
                Label n = Ui.Caption(ShortNames[i], DesignTokens.Text2);
                n.style.width = LabelWidth;
                n.style.overflow = Overflow.Hidden;
                n.style.whiteSpace = WhiteSpace.NoWrap;
                row.Add(n);
                VisualElement track = new VisualElement();
                track.Size(BarWidth, 8);
                track.style.backgroundColor = DesignTokens.Bg3;
                track.Round(DesignTokens.RadiusCheckbox);
                traitFill[i] = new VisualElement();
                traitFill[i].style.height = 8;
                traitFill[i].style.backgroundColor = DesignTokens.Accent;
                traitFill[i].Round(DesignTokens.RadiusCheckbox);
                track.Add(traitFill[i]);
                var tick = new VisualElement();
                tick.style.position = Position.Absolute;
                tick.style.width = 1;
                tick.style.height = 8;
                tick.style.left = Mathf.Clamp01(Traits.Normalise(i, Traits.FounderMean[i])) * BarWidth;
                tick.style.backgroundColor = DesignTokens.Text3;
                track.Add(tick);
                row.Add(track);
                traitValue[i] = Ui.Caption("", DesignTokens.Text1);
                traitValue[i].style.flexGrow = 1;
                traitValue[i].style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(traitValue[i]);
                row.pickingMode = PickingMode.Position;
                int trait = i;
                Tooltips.Attach(row, () =>
                {
                    TipContent tip = UiText.TraitTip(trait);
                    tip.Body += " " + UiText.Body("inspect-trait");
                    return tip;
                });
                bars.Add(row);
            }
            Add(bars);

            VisualElement brain = Ui.Column();
            brain.style.marginTop = DesignTokens.Space3;
            string[] outNames = { "turn", "thrust", "bite", "mate" };
            for (int i = 0; i < 4; i++)
            {
                VisualElement row = Ui.Row();
                row.style.height = 22;
                Label n = Ui.Caption(outNames[i], DesignTokens.Text2);
                n.style.width = LabelWidth;
                row.Add(n);
                VisualElement track = new VisualElement();
                track.Size(BarWidth, 8);
                track.style.backgroundColor = DesignTokens.Bg3;
                track.Round(DesignTokens.RadiusCheckbox);
                outFill[i] = new VisualElement();
                outFill[i].style.height = 8;
                outFill[i].style.backgroundColor = DesignTokens.Accent;
                outFill[i].Round(DesignTokens.RadiusCheckbox);
                track.Add(outFill[i]);
                row.Add(track);
                row.pickingMode = PickingMode.Position;
                Tooltips.Attach(row, "inspect-brain");
                brain.Add(row);
                if (i == 3) outMateRow = row;
            }
            Add(brain);
        }

        Label Readout(VisualElement parent, string name, string tipId)
        {
            VisualElement row = Ui.Row();
            row.style.height = 24;
            Label n = Ui.Caption(name);
            n.pickingMode = PickingMode.Position;
            Tooltips.Attach(n, tipId);
            n.style.flexGrow = 1;
            row.Add(n);
            Label v = Ui.Body("");
            row.Add(v);
            parent.Add(row);
            return v;
        }

        /// <summary>Called every frame; the card refreshes ten times a second.</summary>
        public void Tick(float realDt)
        {
            int id = app.WorldView.SelectedCellId;
            Simulation sim = app.Sim;
            if (id == WorldView.NoCell || sim == null)
            {
                if (this.IsShown()) this.Show(false);
                shownId = WorldView.NoCell;
                return;
            }
            Cell c = sim.FindCell(id);
            if (c == null || !c.Alive)
            {
                app.ClearSelection();
                this.Show(false);
                shownId = WorldView.NoCell;
                return;
            }
            sinceUpdate += realDt;
            if (shownId == id && sinceUpdate < 0.1f) return;
            sinceUpdate = 0f;
            shownId = id;
            this.Show(true);

            SpeciesRecord sr = sim.Species.Get(c.SpeciesId);
            swatch.style.backgroundColor = sr != null ? new Color(sr.ColorR, sr.ColorG, sr.ColorB) : new Color(c.ColorR, c.ColorG, c.ColorB);
            speciesName.text = sr != null ? sr.Name : "Species " + c.SpeciesId;
            energy.text = Ui.F(c.Energy, 0) + " / " + Ui.F(c.Emax, 0);
            age.text = Ui.Mmss(c.Age);
            generation.text = c.Generation.ToString();
            parentValue.text = c.ParentId < 0 ? "none" : c.ParentId.ToString();
            for (int i = 0; i < Traits.ChartCount; i++)
            {
                float v = c.G.Trait[i];
                traitFill[i].style.width = Mathf.Clamp01(Traits.Normalise(i, v)) * BarWidth;
                traitValue[i].text = Ui.F(v, 2);
            }
            outFill[0].style.width = Mathf.Clamp01((c.OutTurn + 1f) * 0.5f) * BarWidth;
            outFill[1].style.width = Mathf.Clamp01(c.OutThrust) * BarWidth;
            outFill[2].style.width = Mathf.Clamp01((c.OutBite + 1f) * 0.5f) * BarWidth;
            outFill[3].style.width = Mathf.Clamp01((c.OutMate + 1f) * 0.5f) * BarWidth;
        }
    }
}
