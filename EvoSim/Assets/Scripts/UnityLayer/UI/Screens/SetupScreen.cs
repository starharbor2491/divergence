using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class SetupScreen : VisualElement
    {
        const string SeedAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

        static readonly string[] PresetTipIds = { "preset-mild", "preset-gradient", "preset-basin", "preset-rapid" };
        static readonly string[] PresetIcons = { "wave", "temperature", "toxin-spill", "bolt" };
        static readonly string[] PresetSummaries =
        {
            "Gentle warmth and steady food. Good for watching gradual colour and trait drift.",
            "Cold north to warm south. Cells adapt to different local temperatures.",
            "Dense drifting toxin clouds select strongly for toxin resistance.",
            "Abundant food, sharp climate zones, and 2x mutation rate for rapid divergence."
        };

        readonly AppController app;
        readonly VisualElement[] presetCards = new VisualElement[Presets.Names.Length];
        readonly Label customBadge;
        readonly EvoTextField seed;
        readonly EvoSlider food, temperature, zone, toxin, mutation;
        readonly EvoButton start;
        bool filling;

        public SetupScreen(AppController app, UiRoot ui)
        {
            this.app = app;
            this.Fill();
            style.backgroundColor = DesignTokens.Hex("#06090D", 0.28f);
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            var card = new VisualElement();
            card.style.width = DesignTokens.SetupCardWidth;
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Panel);
            card.Pad(DesignTokens.Space6);
            Add(card);

            VisualElement columns = Ui.Row(Align.Stretch);
            card.Add(columns);

            VisualElement left = Ui.Column();
            left.style.width = 430;
            left.style.flexShrink = 0;
            columns.Add(left);

            VisualElement mark = Ui.Row();
            var glyph = new IconElement("glyph-branch", 28f) { Tint = DesignTokens.Text1 };
            glyph.style.marginRight = DesignTokens.Space3;
            mark.Add(glyph);
            mark.Add(Ui.Display(UiText.AppName, DesignTokens.Text1));
            left.Add(mark);

            Label lead = Ui.Body(
                "Cells inherit traits and neural brains, compete for food, and adapt to temperature and toxin. " +
                "Pick an environment below, then trigger events during the run to watch populations evolve.",
                DesignTokens.Text2);
            lead.style.marginTop = DesignTokens.Space3;
            left.Add(lead);

            VisualElement presetHead = Ui.Row();
            presetHead.style.marginTop = DesignTokens.Space5;
            presetHead.style.marginBottom = DesignTokens.Space2;
            Label chooseLabel = Ui.Subhead("Starting environment", DesignTokens.Text1);
            chooseLabel.pickingMode = PickingMode.Position;
            Tooltips.Attach(chooseLabel, "preset");
            presetHead.Add(chooseLabel);
            presetHead.Add(Ui.Spacer());
            customBadge = Ui.Caption("Modified", DesignTokens.AccentText);
            customBadge.Show(false);
            presetHead.Add(customBadge);
            left.Add(presetHead);

            for (int i = 0; i < Presets.Names.Length; i++)
            {
                int index = i;
                var tile = BuildPresetCard(Presets.Names[i], PresetSummaries[i], PresetIcons[i], PresetTipIds[i]);
                if (i > 0) tile.style.marginTop = DesignTokens.Space2;
                tile.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button != 0) return;
                    SelectPreset(index);
                    e.StopPropagation();
                });
                presetCards[i] = tile;
                left.Add(tile);
            }

            VisualElement vDiv = Ui.Divider(true);
            vDiv.style.height = StyleKeyword.Auto;
            vDiv.style.alignSelf = Align.Stretch;
            vDiv.style.marginLeft = DesignTokens.Space6;
            vDiv.style.marginRight = DesignTokens.Space6;
            columns.Add(vDiv);

            VisualElement right = Ui.Column();
            right.style.flexGrow = 1;
            right.style.justifyContent = Justify.SpaceBetween;
            columns.Add(right);

            VisualElement parameters = Ui.Column();
            right.Add(parameters);

            VisualElement seedRow = Ui.Row(Align.FlexStart);
            Label seedLabel = Ui.LabelText("Seed");
            seedLabel.style.width = 72;
            seedLabel.style.marginTop = 9;
            seedLabel.pickingMode = PickingMode.Position;
            Tooltips.Attach(seedLabel, "seed");
            seedRow.Add(seedLabel);

            seed = new EvoTextField("seed", "Seed");
            seed.style.flexGrow = 1;
            seed.Changed += OnSeedChanged;
            var random = new EvoButton(ButtonKind.Secondary, null, "random", "random-seed");
            random.style.width = DesignTokens.ControlHeight;
            random.Pad(0);
            random.style.marginLeft = DesignTokens.Space2;
            random.Clicked += () => { seed.Value = RandomSeed(); OnSeedChanged(seed.Value); };
            VisualElement seedBox = Ui.Row(Align.FlexStart);
            seedBox.style.flexGrow = 1;
            seedBox.Add(seed);
            seedBox.Add(random);
            seedRow.Add(seedBox);
            parameters.Add(seedRow);

            VisualElement sliders = Ui.Column();
            sliders.style.marginTop = DesignTokens.Space4;
            food = new EvoSlider("Food abundance", "food-abundance", 0.2f, 2f, 1f, 0.05f, v => Ui.F(v, 2));
            temperature = new EvoSlider("Temperature contrast", "temperature-range", 0f, 1f, 0.6f, 0.05f, v => Ui.F(v, 2));
            zone = new EvoSlider("Climate scale", "zone-scale", 0.5f, 2f, 1f, 0.05f, v => Ui.F(v, 2));
            toxin = new EvoSlider("Starting toxin", "toxin-level", 0f, 1f, 0.3f, 0.05f, v => Ui.F(v, 2));
            mutation = new EvoSlider("Mutation rate", "mutation-rate", 0.25f, 3f, 1f, 0.05f, v => Ui.F(v, 2));
            EvoSlider[] all = { food, temperature, zone, toxin, mutation };
            for (int i = 0; i < all.Length; i++)
            {
                if (i > 0) all[i].style.marginTop = 10;
                all[i].Changed += v => OnSliderChanged();
                sliders.Add(all[i]);
            }
            parameters.Add(sliders);

            VisualElement buttons = Ui.Row();
            buttons.style.marginTop = DesignTokens.Space5;
            start = new EvoButton(ButtonKind.Primary, "Start run", "play", "start");
            start.style.height = 40;
            start.Clicked += () => app.StartRun(app.SetupDraft);

            var load = new EvoButton(ButtonKind.Secondary, "Load file", "load", "load");
            load.style.height = 40;
            load.style.marginLeft = DesignTokens.Space2;
            FileBridge.BindPicker(load, app.LoadRunFromPicker, () => !ui.ModalOpen && !ui.WindowTooSmall);

            var help = new EvoButton(ButtonKind.Secondary, "How it works", "help", "help");
            help.style.height = 40;
            help.style.marginLeft = DesignTokens.Space2;
            help.Clicked += ui.OpenHelp;

            buttons.Add(start);
            buttons.Add(Ui.Spacer());
            buttons.Add(load);
            buttons.Add(help);
            right.Add(buttons);
        }

        static VisualElement BuildPresetCard(string name, string summary, string iconName, string tipId)
        {
            VisualElement card = Ui.Row(Align.FlexStart);
            card.Pad(11, 14);
            card.Round(DesignTokens.RadiusCard);
            card.style.backgroundColor = DesignTokens.Bg2;
            card.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            Tooltips.Attach(card, tipId);

            var icon = new IconElement(iconName, 17f) { Tint = DesignTokens.Text2 };
            icon.style.marginTop = 2;
            icon.style.marginRight = DesignTokens.Space3;
            card.Add(icon);

            VisualElement copy = Ui.Column();
            copy.style.flexGrow = 1;
            copy.style.flexShrink = 1;
            copy.pickingMode = PickingMode.Ignore;
            copy.Add(Ui.LabelText(name, DesignTokens.Text1));
            Label desc = Ui.Caption(summary, DesignTokens.Text2);
            desc.style.marginTop = 2;
            copy.Add(desc);
            card.Add(copy);

            card.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (card.userData is bool sel && sel) return;
                card.style.backgroundColor = DesignTokens.Bg3;
            });
            card.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (card.userData is bool sel && sel) return;
                card.style.backgroundColor = DesignTokens.Bg2;
            });
            return card;
        }

        public static string RandomSeed()
        {
            var chars = new char[6];
            for (int i = 0; i < chars.Length; i++) chars[i] = SeedAlphabet[Random.Range(0, SeedAlphabet.Length)];
            return new string(chars);
        }

        public void LoadFromDraft()
        {
            filling = true;
            SetupSettings s = app.SetupDraft;
            seed.Value = s.SeedText;
            food.SetValue(s.FoodAbundance, false);
            temperature.SetValue(s.TemperatureRange, false);
            zone.SetValue(s.ZoneScale, false);
            toxin.SetValue(s.ToxinLevel, false);
            mutation.SetValue(s.MutationRate, false);
            seed.SetInvalid(null);
            start.Disabled = false;
            filling = false;
            SyncPresetSelection();
            app.RefreshSetupPreview();
        }

        void SelectPreset(int index)
        {
            if (filling || index < 0 || index >= Presets.Names.Length) return;
            app.SetupDraft = Presets.Get(Presets.Names[index]);
            LoadFromDraft();
        }

        void OnSeedChanged(string text)
        {
            if (filling) return;
            bool empty = string.IsNullOrWhiteSpace(text);
            seed.SetInvalid(empty ? "Enter a seed." : null);
            start.Disabled = empty;
            if (!empty)
            {
                app.SetupDraft.SeedText = text.Trim();
                SyncPresetSelection();
                app.RefreshSetupPreview();
            }
        }

        void OnSliderChanged()
        {
            if (filling) return;
            SetupSettings s = app.SetupDraft;
            s.FoodAbundance = food.Value;
            s.TemperatureRange = temperature.Value;
            s.ZoneScale = zone.Value;
            s.ToxinLevel = toxin.Value;
            s.MutationRate = mutation.Value;
            SyncPresetSelection();
            app.RefreshSetupPreview();
        }

        void SyncPresetSelection()
        {
            string match = Presets.Match(app.SetupDraft);
            bool isCustom = match == Presets.Custom;
            customBadge.Show(isCustom);
            for (int i = 0; i < presetCards.Length; i++)
            {
                bool active = !isCustom && Presets.Names[i] == match;
                presetCards[i].userData = active;
                presetCards[i].style.backgroundColor = active ? DesignTokens.ActivePill : DesignTokens.Bg2;
                presetCards[i].Border(active ? DesignTokens.BorderStrong : DesignTokens.Border, 1f);
                var ic = presetCards[i].Q<IconElement>();
                if (ic != null) ic.Tint = active ? DesignTokens.AccentText : DesignTokens.Text2;
            }
        }
    }
}
