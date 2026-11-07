using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The setup screen (DESIGN 6.1): wordmark, Preset, Seed, five sliders, Start, Load saved run, How it works.</summary>
    public sealed class SetupScreen : VisualElement
    {
        const string SeedAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

        readonly AppController app;
        readonly EvoDropdown preset;
        readonly EvoTextField seed;
        readonly EvoSlider food, temperature, zone, toxin, mutation;
        readonly EvoButton start;
        bool filling;

        public SetupScreen(AppController app, UiRoot ui)
        {
            this.app = app;
            this.Fill();
            style.backgroundColor = DesignTokens.Bg0;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            var card = new VisualElement();
            card.style.width = DesignTokens.SetupCardWidth;
            card.style.backgroundColor = DesignTokens.Bg1;
            card.Round(DesignTokens.Radius);
            card.Pad(DesignTokens.Space6);
            Add(card);

            // wordmark (DESIGN 4.15)
            VisualElement mark = Ui.Row();
            var glyph = new IconElement("glyph-branch", 40f) { Tint = DesignTokens.AccentText };
            glyph.style.marginRight = DesignTokens.Space3;
            mark.Add(glyph);
            mark.Add(Ui.Display(UiText.AppName, DesignTokens.Text1));
            card.Add(mark);
            Label sub = Ui.Body(UiText.AppSubtitle, DesignTokens.Text2);
            sub.style.marginLeft = 40 + DesignTokens.Space3;
            card.Add(sub);

            // preset and seed
            var presetNames = new List<string> { Presets.Custom };
            presetNames.AddRange(Presets.Names);
            preset = new EvoDropdown("preset");
            preset.SetItems(presetNames);
            preset.Changed += OnPreset;
            card.Add(FieldRow("Preset", "preset", preset, DesignTokens.Space5));

            seed = new EvoTextField("seed", "Seed");
            seed.style.flexGrow = 1;
            seed.Changed += OnSeedChanged;
            var random = new EvoButton(ButtonKind.Icon, null, "random", "random-seed");
            random.style.marginLeft = DesignTokens.Space2;
            random.Clicked += () => { seed.Value = RandomSeed(); OnSeedChanged(seed.Value); };
            VisualElement seedBox = Ui.Row(Align.FlexStart);
            seedBox.Add(seed);
            seedBox.Add(random);
            card.Add(FieldRow("Seed", "seed", seedBox, DesignTokens.Space4, Align.FlexStart));

            // sliders
            VisualElement sliders = Ui.Column();
            sliders.style.marginTop = DesignTokens.Space5;
            sliders.style.marginBottom = DesignTokens.Space5;
            food = new EvoSlider("Food abundance", "food-abundance", 0.2f, 2f, 1f, 0.05f, v => Ui.F(v, 2));
            temperature = new EvoSlider("Temperature range", "temperature-range", 0f, 1f, 0.6f, 0.05f, v => Ui.F(v, 2));
            zone = new EvoSlider("Zone scale", "zone-scale", 0.5f, 2f, 1f, 0.05f, v => Ui.F(v, 2));
            toxin = new EvoSlider("Toxin level", "toxin-level", 0f, 1f, 0.3f, 0.05f, v => Ui.F(v, 2));
            mutation = new EvoSlider("Mutation rate", "mutation-rate", 0.25f, 3f, 1f, 0.05f, v => Ui.F(v, 2));
            EvoSlider[] all = { food, temperature, zone, toxin, mutation };
            for (int i = 0; i < all.Length; i++)
            {
                if (i > 0) all[i].style.marginTop = DesignTokens.Space4;
                all[i].Changed += v => OnSliderChanged();
                sliders.Add(all[i]);
            }
            card.Add(sliders);

            // buttons
            VisualElement buttons = Ui.Row();
            start = new EvoButton(ButtonKind.Primary, "Start", null, "start");
            start.Clicked += () => app.StartRun(app.SetupDraft);
            var load = new EvoButton(ButtonKind.Secondary, "Load saved run", null, "load");
            load.style.marginLeft = DesignTokens.Space2;
            load.Clicked += app.LoadRunFromPicker;
            load.RegisterCallback<PointerDownEvent>(e => FileBridge.ArmPick(".json,application/json"), TrickleDown.TrickleDown);
            var help = new EvoButton(ButtonKind.Secondary, "How it works", null, "help");
            help.style.marginLeft = DesignTokens.Space2;
            help.Clicked += ui.OpenHelp;
            buttons.Add(start);
            buttons.Add(load);
            buttons.Add(help);
            card.Add(buttons);
        }

        VisualElement FieldRow(string label, string tipId, VisualElement field, float marginTop, Align align = Align.Center)
        {
            VisualElement row = Ui.Row(align);
            row.style.marginTop = marginTop;
            Label l = Ui.LabelText(label);
            l.style.width = 120;
            l.style.flexShrink = 0;
            if (align == Align.FlexStart) l.style.marginTop = 9;
            l.pickingMode = PickingMode.Position;
            Tooltips.Attach(l, tipId);
            row.Add(l);
            field.style.flexGrow = 1;
            row.Add(field);
            return row;
        }

        public static string RandomSeed()
        {
            var chars = new char[6];
            for (int i = 0; i < chars.Length; i++) chars[i] = SeedAlphabet[Random.Range(0, SeedAlphabet.Length)];
            return new string(chars);
        }

        /// <summary>Shows the draft settings held by the app (the last Run's settings when returning from a Run).</summary>
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
            string match = Presets.Match(s);
            preset.Selected = match == Presets.Custom ? 0 : System.Array.IndexOf(Presets.Names, match) + 1;
            seed.SetInvalid(null);
            start.Disabled = false;
            filling = false;
        }

        void OnPreset(int index)
        {
            if (filling || index <= 0) return;
            app.SetupDraft = Presets.Get(Presets.Names[index - 1]);
            LoadFromDraft();
        }

        void OnSeedChanged(string text)
        {
            if (filling) return;
            bool empty = string.IsNullOrWhiteSpace(text);
            seed.SetInvalid(empty ? "Enter a Seed." : null);
            start.Disabled = empty;
            if (!empty) { app.SetupDraft.SeedText = text.Trim(); SyncPresetName(); }
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
            SyncPresetName();
        }

        void SyncPresetName()
        {
            string match = Presets.Match(app.SetupDraft);
            filling = true;
            preset.Selected = match == Presets.Custom ? 0 : System.Array.IndexOf(Presets.Names, match) + 1;
            filling = false;
        }
    }
}
