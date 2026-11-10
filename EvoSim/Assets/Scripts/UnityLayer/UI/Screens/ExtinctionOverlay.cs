using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>"Population extinct" card over the frozen World (DESIGN 6.4).</summary>
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
            card.style.backgroundColor = DesignTokens.Bg1;
            card.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            card.Round(DesignTokens.Radius);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement head = Ui.Row();
            var icon = new IconElement("status-critical", 32f) { Tint = DesignTokens.StatusCritical };
            icon.style.marginRight = DesignTokens.Space3;
            head.Add(icon);
            head.Add(Ui.Heading(UiText.PopulationExtinct, DesignTokens.Text1));
            card.Add(head);

            VisualElement rows = Ui.Column();
            rows.style.marginTop = DesignTokens.Space4;
            runTime = Row(rows, "run time");
            maxGeneration = Row(rows, "highest generation");
            speciesEver = Row(rows, "species ever");
            card.Add(rows);

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

        static Label Row(VisualElement parent, string name)
        {
            VisualElement row = Ui.Row();
            row.style.height = 28;
            Label n = Ui.Caption(name);
            n.style.flexGrow = 1;
            row.Add(n);
            Label v = Ui.Body("");
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
