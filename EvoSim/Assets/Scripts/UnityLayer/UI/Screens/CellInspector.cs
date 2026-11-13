using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class CellInspector : VisualElement
    {
        const float LabelWidth = 108f;
        const float BarWidth = 92f;
        static readonly string[] ShortNames =
        {
            "Size", "Speed", "Sense range", "Preferred temp", "Temp tolerance", "Toxin resist", "Diet", "Repro threshold"
        };

        readonly AppController app;
        readonly VisualElement portrait, energyFill;
        readonly Label speciesName, roleBadge, energyText, age, generation, parentValue;
        readonly VisualElement[] traitFill = new VisualElement[Traits.ChartCount];
        readonly Label[] traitValue = new Label[Traits.ChartCount];
        readonly VisualElement[] outFill = new VisualElement[4];
        Color cellBodyCol = Color.white;
        float cellDiet;
        float sinceUpdate = 99f;
        int shownId = WorldView.NoCell;

        public CellInspector(AppController app)
        {
            this.app = app;
            style.position = Position.Absolute;
            style.left = 12;
            style.top = 10;
            style.width = DesignTokens.InspectorWidth;
            LiquidGlass.Attach(this, DesignTokens.RadiusPanel, GlassTone.Panel);
            this.Pad(14);
            this.Show(false);

            VisualElement head = Ui.Row();
            portrait = new VisualElement();
            portrait.Size(36, 36);
            portrait.Round(18);
            portrait.style.backgroundColor = DesignTokens.GlassInset;
            portrait.Border(DesignTokens.Border, 1);
            portrait.style.marginRight = 10;
            portrait.style.flexShrink = 0;
            portrait.pickingMode = PickingMode.Ignore;
            portrait.generateVisualContent += PaintPortrait;
            head.Add(portrait);

            VisualElement titleCol = Ui.Column();
            titleCol.style.flexGrow = 1;
            titleCol.style.flexShrink = 1;
            speciesName = Ui.Title("Cell");
            roleBadge = Ui.Caption("", DesignTokens.Text2);
            titleCol.Add(speciesName);
            titleCol.Add(roleBadge);
            head.Add(titleCol);

            var close = new EvoButton(ButtonKind.Icon, null, "close", "inspector-close");
            close.Size(30, 30);
            close.Clicked += () => app.ClearSelection();
            head.Add(close);
            Add(head);

            VisualElement energyBox = Ui.Column();
            energyBox.style.marginTop = DesignTokens.Space3;
            energyBox.pickingMode = PickingMode.Position;
            Tooltips.Attach(energyBox, "inspect-energy");
            VisualElement eHead = Ui.Row();
            eHead.Add(Ui.Caption("Energy", DesignTokens.Text2));
            eHead.Add(Ui.Spacer());
            energyText = Ui.MonoCaption("", DesignTokens.Text1);
            eHead.Add(energyText);
            energyBox.Add(eHead);
            var eTrack = new VisualElement();
            eTrack.style.height = 6;
            eTrack.style.marginTop = 4;
            eTrack.style.backgroundColor = DesignTokens.GlassInset;
            eTrack.Round(3);
            eTrack.pickingMode = PickingMode.Ignore;
            energyFill = new VisualElement();
            energyFill.style.height = 6;
            energyFill.Round(3);
            energyFill.style.backgroundColor = DesignTokens.AccentText;
            energyFill.pickingMode = PickingMode.Ignore;
            eTrack.Add(energyFill);
            energyBox.Add(eTrack);
            Add(energyBox);

            VisualElement meta = Ui.Row();
            meta.style.marginTop = 10;
            meta.style.paddingBottom = 8;
            meta.style.borderBottomWidth = 1;
            meta.style.borderBottomColor = DesignTokens.Border;
            generation = MetaItem(meta, "Gen", "inspect-generation", false);
            age = MetaItem(meta, "Age", "inspect-age", true);
            parentValue = MetaItem(meta, "Parent", "inspect-parent", true);
            Add(meta);

            Label traitsHead = Ui.CaptionBold("Traits", DesignTokens.Text2);
            traitsHead.style.marginTop = 10;
            traitsHead.style.marginBottom = 4;
            Add(traitsHead);

            VisualElement bars = Ui.Column();
            for (int i = 0; i < Traits.ChartCount; i++)
            {
                VisualElement row = Ui.Row();
                row.style.height = 21;
                Label n = Ui.Caption(ShortNames[i], DesignTokens.Text2);
                n.style.width = LabelWidth;
                n.style.overflow = Overflow.Hidden;
                n.style.whiteSpace = WhiteSpace.NoWrap;
                row.Add(n);
                VisualElement track = new VisualElement();
                track.Size(BarWidth, 6);
                track.style.backgroundColor = DesignTokens.GlassInset;
                track.Round(3);
                traitFill[i] = new VisualElement();
                traitFill[i].style.height = 6;
                traitFill[i].style.backgroundColor = DesignTokens.ChartSeries1;
                traitFill[i].Round(3);
                track.Add(traitFill[i]);
                var tick = new VisualElement();
                tick.style.position = Position.Absolute;
                tick.style.width = 1.5f;
                tick.style.height = 8;
                tick.style.top = -1;
                tick.style.left = Mathf.Clamp01(Traits.Normalise(i, Traits.FounderMean[i])) * BarWidth;
                tick.style.backgroundColor = DesignTokens.Text1;
                track.Add(tick);
                row.Add(track);
                traitValue[i] = Ui.MonoCaption("", DesignTokens.Text1);
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

            Label brainHead = Ui.CaptionBold("Neural outputs", DesignTokens.Text2);
            brainHead.style.marginTop = 10;
            brainHead.style.marginBottom = 4;
            Add(brainHead);

            VisualElement brain = Ui.Column();
            string[] outNames = { "Steer", "Swim", "Bite", "Court" };
            for (int i = 0; i < 4; i++)
            {
                VisualElement row = Ui.Row();
                row.style.height = 19;
                Label n = Ui.Caption(outNames[i], DesignTokens.Text2);
                n.style.width = LabelWidth;
                row.Add(n);
                VisualElement track = new VisualElement();
                track.Size(BarWidth, 5);
                track.style.backgroundColor = DesignTokens.GlassInset;
                track.Round(3);
                outFill[i] = new VisualElement();
                outFill[i].style.height = 5;
                outFill[i].style.backgroundColor = i == 2 ? DesignTokens.WorldPredatorCore : DesignTokens.Text2;
                outFill[i].Round(3);
                track.Add(outFill[i]);
                row.Add(track);
                row.pickingMode = PickingMode.Position;
                Tooltips.Attach(row, "inspect-brain");
                brain.Add(row);
            }
            Add(brain);
        }

        static Label MetaItem(VisualElement parent, string name, string tipId, bool gap)
        {
            VisualElement col = Ui.Column();
            col.style.flexGrow = 1;
            if (gap) col.style.marginLeft = DesignTokens.Space2;
            col.pickingMode = PickingMode.Position;
            Tooltips.Attach(col, tipId);
            col.Add(Ui.Caption(name, DesignTokens.Text3));
            Label v = Ui.Mono("", DesignTokens.Text1, DesignTokens.TypeLabelSize);
            col.Add(v);
            parent.Add(col);
            return v;
        }

        void PaintPortrait(MeshGenerationContext ctx)
        {
            Painter2D p = ctx.painter2D;
            Vector2 c = new Vector2(18f, 18f);
            p.fillColor = DesignTokens.WithAlpha(cellBodyCol, 0.22f);
            p.BeginPath(); p.Arc(c, 14f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();

            p.fillColor = DesignTokens.WithAlpha(cellBodyCol, 0.85f);
            p.BeginPath(); p.Arc(c, 10f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();

            Color core = Color.Lerp(DesignTokens.WorldGrazerCore, DesignTokens.WorldPredatorCore, Mathf.Clamp01(cellDiet * 1.25f));
            p.fillColor = Color.Lerp(cellBodyCol, core, 0.6f);
            p.BeginPath(); p.Arc(new Vector2(16.8f, 18f), 4f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();

            p.fillColor = Color.white;
            p.BeginPath(); p.Arc(new Vector2(22.5f, 14.6f), 2f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
            p.BeginPath(); p.Arc(new Vector2(22.5f, 21.4f), 2f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
            p.fillColor = DesignTokens.WorldEye;
            p.BeginPath(); p.Arc(new Vector2(23.1f, 14.6f), 1.15f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
            p.BeginPath(); p.Arc(new Vector2(23.1f, 21.4f), 1.15f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
        }

        static string DescribeCell(Cell c, SimParameters p)
        {
            float temp = c.G.Trait[TraitId.PreferredTemperature];
            string climate = temp < 0.36f ? "Cold-suited" : (temp > 0.64f ? "Warm-suited" : "Temperate");
            float diet = c.G.Trait[TraitId.Diet];
            string foodRole = diet >= 0.45f ? "hunter" : (diet >= 0.25f ? "omnivore" : "grazer");
            string repro = c.G.Trait[TraitId.SexualTendency] >= p.SexThreshold ? "sexual" : "asexual";
            return climate + " " + foodRole + " · " + repro;
        }

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
            cellBodyCol = new Color(c.ColorR, c.ColorG, c.ColorB);
            cellDiet = c.G.Trait[TraitId.Diet];
            portrait.MarkDirtyRepaint();

            speciesName.text = sr != null ? sr.Name : "Species " + c.SpeciesId;
            roleBadge.text = DescribeCell(c, sim.P);
            float eFrac = c.Emax > 0f ? Mathf.Clamp01(c.Energy / c.Emax) : 0f;
            energyFill.style.width = Length.Percent(eFrac * 100f);
            energyFill.style.backgroundColor = eFrac < 0.25f ? DesignTokens.StatusCritical : (eFrac < 0.45f ? DesignTokens.StatusWarning : DesignTokens.AccentText);
            energyText.text = Ui.F(c.Energy, 0) + " / " + Ui.F(c.Emax, 0);

            age.text = Ui.Mmss(c.Age);
            generation.text = c.Generation.ToString();
            parentValue.text = c.ParentId < 0 ? "none" : "#" + c.ParentId;
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
