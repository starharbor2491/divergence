using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Liquid Glass "Population extinct" summary card over the frozen World (DESIGN 6.4).</summary>
    public sealed class ExtinctionOverlay : VisualElement
    {
        readonly Label runTime, maxGeneration, speciesEver;

        public ExtinctionOverlay(AppController app)
        {
            this.Fill();
            style.backgroundColor = DesignTokens.Scrim;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            this.Show(false);

            var card = new VisualElement();
            card.style.width = DesignTokens.ExtinctionWidth;
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Modal);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement head = Ui.Row();
            var badge = new VisualElement();
            badge.Size(44, 44);
            badge.Round(14);
            badge.style.backgroundColor = DesignTokens.WithAlpha(DesignTokens.StatusCritical, 0.18f);
            badge.Border(DesignTokens.WithAlpha(DesignTokens.StatusCritical, 0.45f), 1);
            badge.style.alignItems = Align.Center;
            badge.style.justifyContent = Justify.Center;
            badge.style.marginRight = DesignTokens.Space3;
            badge.Add(new IconElement("status-critical", 24f) { Tint = DesignTokens.StatusCritical });
            head.Add(badge);

            VisualElement titleCol = Ui.Column();
            titleCol.Add(Ui.Heading(UiText.PopulationExtinct, DesignTokens.Text1));
            titleCol.Add(Ui.Caption("Every Cell in the World ran out of energy or succumbed to Pressures.", DesignTokens.Text2));
            head.Add(titleCol);
            card.Add(head);

            VisualElement stats = Ui.Row();
            stats.style.marginTop = DesignTokens.Space4;
            runTime = StatPill(stats, "Run time", false);
            maxGeneration = StatPill(stats, "Highest Gen", true);
            speciesEver = StatPill(stats, "Species ever", true);
            card.Add(stats);

            VisualElement buttons = Ui.Row();
            buttons.style.marginTop = DesignTokens.Space5;
            buttons.style.justifyContent = Justify.FlexEnd;
            var restart = new EvoButton(ButtonKind.Primary, "Restart", "restart", "restart");
            restart.Clicked += app.Restart;
            var csv = new EvoButton(ButtonKind.Secondary, "Export CSV", "csv", "csv");
            csv.style.marginRight = DesignTokens.Space2;
            csv.Clicked += app.ExportCsv;
            var home = new EvoButton(ButtonKind.Secondary, "Menu", "home", "home");
            home.style.marginRight = DesignTokens.Space2;
            home.Clicked += app.GoToSetup;
            buttons.Add(home);
            buttons.Add(csv);
            buttons.Add(restart);
            card.Add(buttons);
        }

        static Label StatPill(VisualElement parent, string name, bool gap)
        {
            VisualElement pill = Ui.Column();
            pill.style.flexGrow = 1;
            pill.style.flexBasis = 0;
            if (gap) pill.style.marginLeft = DesignTokens.Space2;
            LiquidGlass.StyleWell(pill, DesignTokens.Radius);
            pill.Pad(DesignTokens.Space3);
            pill.Add(Ui.Caption(name, DesignTokens.Text3));
            Label v = Ui.Mono("", DesignTokens.Text1, DesignTokens.TypeTitleSize);
            v.style.marginTop = 2;
            pill.Add(v);
            parent.Add(pill);
            return v;
        }

        public void Open(Simulation sim)
        {
            runTime.text = Ui.Mmss(sim.Time);
            maxGeneration.text = Ui.Count(sim.Stats.MaxGenerationEver);
            speciesEver.text = Ui.Count(sim.Species.Records.Count);
            Tooltips.Hide();
            this.Show(true);
        }

        public void Close() { this.Show(false); }
    }
}
