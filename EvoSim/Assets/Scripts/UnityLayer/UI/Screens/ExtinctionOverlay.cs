using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
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
            var icon = new IconElement("status-critical", 24f) { Tint = DesignTokens.StatusCritical };
            icon.style.marginRight = DesignTokens.Space3;
            head.Add(icon);
            head.Add(Ui.Heading(UiText.PopulationExtinct, DesignTokens.Text1));
            card.Add(head);

            Label sub = Ui.Body("Every lineage in this run died out.", DesignTokens.Text2);
            sub.style.marginTop = DesignTokens.Space2;
            card.Add(sub);

            VisualElement stats = Ui.Column();
            stats.style.marginTop = DesignTokens.Space4;
            runTime = Row(stats, "Run duration");
            maxGeneration = Row(stats, "Highest generation");
            speciesEver = Row(stats, "Total species", false);
            card.Add(stats);

            VisualElement buttons = Ui.Row();
            buttons.style.marginTop = DesignTokens.Space5;
            buttons.style.justifyContent = Justify.FlexEnd;
            var restart = new EvoButton(ButtonKind.Primary, "Restart", "restart", "restart");
            restart.Clicked += app.Restart;
            var csv = new EvoButton(ButtonKind.Secondary, "Export CSV", "csv", "csv");
            csv.style.marginRight = DesignTokens.Space2;
            csv.Clicked += app.ExportCsv;
            var home = new EvoButton(ButtonKind.Secondary, "Setup", "home", "home");
            home.style.marginRight = DesignTokens.Space2;
            home.Clicked += app.GoToSetup;
            buttons.Add(home);
            buttons.Add(csv);
            buttons.Add(restart);
            card.Add(buttons);
        }

        static Label Row(VisualElement parent, string name, bool divider = true)
        {
            VisualElement row = Ui.Row();
            row.style.height = 30;
            if (divider)
            {
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = DesignTokens.Border;
            }
            Label n = Ui.Body(name, DesignTokens.Text2);
            n.style.flexGrow = 1;
            row.Add(n);
            Label v = Ui.Mono("", DesignTokens.Text1);
            row.Add(v);
            parent.Add(row);
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
