using System.Collections.Generic;

namespace Evo.UnityLayer
{
    public struct TipContent
    {
        public string Name;
        public string Body;
        public string Key;
    }

    public static class UiText
    {
        struct Entry { public string Name, Text, Key; }

        static readonly Dictionary<string, Entry> table = new Dictionary<string, Entry>();

        static void Add(string id, string name, string text, string key = null)
        {
            table[id] = new Entry { Name = name, Text = text, Key = key };
        }

        public static readonly string[] TraitIds =
        {
            "trait-size", "trait-speed", "trait-sense", "trait-temperature", "trait-tolerance",
            "trait-resistance", "trait-diet", "trait-reproduction", "trait-sexual", "trait-choosiness"
        };

        public static readonly string[] HelpTitles =
        {
            "Overview", "Controls", "World setup", "Environment", "Inherited traits",
            "Reproduction", "Species & colour", "Reading charts", "Events", "Saving & data"
        };

        public static readonly string[] HelpBodies =
        {
            "Divergence simulates natural selection in continuous 2D space. Cells move, eat food pellets, avoid drifting toxin, and cope with local temperature. Individuals better suited to their surroundings survive longer and leave more offspring, shifting population traits over generations until lineages split into distinct species.",
            "Drag anywhere in the water to pan and scroll to zoom around the cursor. Click any living cell to inspect its traits, energy, sense radius, and live neural outputs.",
            "A seed fixes the initial terrain and starting population so identical settings always begin from the same world. Presets configure ready-made environments, or you can adjust any slider manually.",
            "Temperature varies smoothly across the water from cool blue currents to warm coral regions. Food spawns more densely in fertile patches, while toxin particles drift, attract slightly, and gradually decay.",
            "Every cell inherits ten numerical traits alongside a 100-weight neural brain. Each trait carries an energy or survival trade-off, so what helps in one environment can be costly in another.",
            "Cells with sexual tendency below 0.50 divide on their own once they store enough energy. At 0.50 or above, two compatible neighbours within sense range must both have sufficient energy to combine genomes, with small random mutations applied at birth.",
            "Founders begin near white. As mutations accumulate in traits and brain weights, genome colours shift smoothly in perceptual colour space. Grazers show a green inner organelle, while high-diet hunters develop warm amber cores and front mandibles.",
            "Charts sample the living population once per simulated second. You can plot traits against elapsed time or average generation, hover any chart to inspect exact values, and click species rows to spotlight them in the water.",
            "Triggering an event alters local or global pressures while a run is underway—useful for testing how quickly a population adapts to sudden heat, cold, scarcity, abundance, or toxin.",
            "Runs can be saved to a JSON snapshot and resumed identically later. CSV export writes two tables—trait history and species populations—for plotting or lab analysis."
        };

        static UiText()
        {
            Add("play", "Resume", "Continues the simulation.", "Key: Space");
            Add("pause", "Pause", "Pauses simulation steps while keeping camera and cell inspection active.", "Key: Space");
            Add("speed-1", "1x speed", "Real-time simulation speed.", "Key: 1");
            Add("speed-2", "2x speed", "Twice normal simulation speed.", "Key: 2");
            Add("speed-3", "5x speed", "Five times normal simulation speed.", "Key: 3");
            Add("speed-4", "10x speed", "Up to ten times normal speed, depending on CPU load.", "Key: 4");
            Add("events", "Events", "Introduce a localized or world-wide environmental change.");
            Add("temperature-tint", "Thermal overlay", "Highlights cold blue and warm red temperature zones across the water.", "Key: T");
            Add("fit-view", "Reset view", "Frames the entire world inside the open viewport.", "Key: F");
            Add("field-notes", "Field log", "Live notes when pressures change, traits shift, or new species branch off.");
            Add("save", "Save snapshot", "Downloads the current run state as a JSON file.");
            Add("load", "Load snapshot", "Opens a previously saved JSON run file.");
            Add("csv", "Export CSV", "Downloads trait and species time-series tables as CSV files.");
            Add("screenshot", "Save PNG", "Captures the current window as a PNG image.");
            Add("help", "Guide", "Opens the reference guide for traits, events, and controls.");
            Add("panel", "Charts panel", "Shows or hides the right-hand charts panel.", "Key: Tab");
            Add("home", "Back to setup", "Returns to world setup after confirmation.");
            Add("random-seed", "Randomize seed", "Generates a new 6-character seed.");
            Add("restart", "Restart run", "Starts a fresh run with the same seed and settings.");
            Add("close", "Close", "Closes this dialog.", "Key: Escape");
            Add("inspector-close", "Deselect", "Clears the current cell selection.", "Key: Escape");
            Add("confirm-cancel", "Cancel", "Keeps the current run open.", "Key: Escape");
            Add("confirm-discard", "Discard", "Ends the current run and proceeds.");
            Add("confirm-save", "Save first", "Downloads a snapshot of the current run before you leave.");

            Add("preset", "Presets", "Starting configurations demonstrating different selection pressures.");
            Add("preset-mild", "Mild world", "Moderate temperature variation and steady food supply.");
            Add("preset-gradient", "Cold gradient", "Sharp north–south thermal contrast that favours regional specialization.");
            Add("preset-basin", "Toxic basin", "High initial toxin density that selects for toxin resistance.");
            Add("preset-rapid", "Rapid evolution", "Abundant food, strong thermal contrast, and 2x mutation rate.");
            Add("seed", "Seed", "Determines terrain layout and initial founder placement.");
            Add("food-abundance", "Food abundance", "Controls how quickly food pellets replenish (0.20 to 2.00).");
            Add("temperature-range", "Temperature contrast", "Difference between the coldest and warmest regions (0.00 to 1.00).");
            Add("zone-scale", "Climate scale", "Lower values create broad climate belts; higher values create smaller pockets.");
            Add("toxin-level", "Starting toxin", "Initial density of drifting toxin clouds (0.00 to 1.00).");
            Add("mutation-rate", "Mutation rate", "Scales how much traits and brain weights vary at birth (1.00 is baseline).");
            Add("start", "Start run", "Begins a new simulation with these settings.");

            Add("tab-traits", "Traits", "Population mean, spread, and distribution for a chosen trait.");
            Add("tab-species", "Species", "Population size of each species over time.");
            Add("tab-tree", "Tree", "Branching lineage diagram showing when species split or went extinct.");
            Add("tab-stats", "Stats", "Live vital rates and causes of death.");
            Add("trait-select", "Selected trait", "Switches which inherited trait is plotted below.");
            Add("axis-switch", "Horizontal axis", "Switch between elapsed time and mean generation of living cells.");
            Add("chart-trait-mean", "Mean & spread", "Solid line shows population mean; shaded band shows ±1 SD; dashed line marks founders.");
            Add("chart-trait-frequency", "Distribution", "Share of living cells falling into six value bands from low (dark) to high (light).");
            Add("chart-species", "Species populations", "Stacked counts for the top 20 species by peak size, with smaller lineages grouped into Other.");
            Add("chart-tree", "Lineage tree", "Horizontal bars span each species' lifetime, weighted by population size.");
            Add("chart-deaths", "Causes of death", "Stacked 10-second death counts over the last 10 minutes; legend numbers show cumulative totals.");
            Add("event-marker", "Event marker", "Marks placed events along the timeline (H heat, I ice, F famine, B bloom, T toxin).");

            Add("event-heat-wave", "Heat wave", "Temporarily raises local temperature before easing back to baseline.");
            Add("event-ice-age", "Ice age", "Temporarily lowers local temperature before easing back to baseline.");
            Add("event-famine", "Famine", "Clears most food in the target area and slows regrowth while active.");
            Add("event-food-bloom", "Food bloom", "Spawns dense extra food across the target area while active.");
            Add("event-toxin-spill", "Toxin spill", "Releases a cloud of toxin particles that slowly disperses and decays.");
            Add("event-radius", "Radius", "Target radius in world units (also determines particle count for toxin spills).");
            Add("event-toxin-amount", "Particle count", "Requests 30 particles per unit of radius, capped by remaining world toxin capacity.");
            Add("event-intensity", "Intensity", "Peak temperature shift at the centre of the event.");
            Add("event-duration", "Duration", "Time held at full strength, in simulated seconds.");
            Add("event-whole-world", "Whole world", "Applies the event uniformly everywhere instead of at a clicked point.");
            Add("event-place", "Place in world", "Closes this dialog so you can click a target spot in the water.");
            Add("event-apply", "Apply now", "Triggers the event across the entire world immediately.");
            Add("event-cancel", "Cancel", "Closes without triggering an event.", "Key: Escape");

            Add("trait-size", "Size", "0.50 to 3.00 (radius is half this). Larger cells store more energy and can prey on smaller cells, at higher metabolic cost.");
            Add("trait-speed", "Speed", "0.50 to 4.00 units/s. Reaches food or escapes faster, though movement energy drain scales with speed squared.");
            Add("trait-sense", "Sense range", "2.00 to 15.00 units. Detects food, neighbours, and toxin from farther away at higher upkeep cost.");
            Add("trait-temperature", "Preferred temperature", "0.00 (cold) to 1.00 (hot). Matching local water temperature avoids thermal energy drain.");
            Add("trait-tolerance", "Temperature tolerance", "0.05 to 0.50. Allows wider temperature mismatch before stress sets in, at higher baseline cost.");
            Add("trait-resistance", "Toxin resistance", "0.00 to 1.00. Cuts toxin energy drain by up to 90% in exchange for higher baseline upkeep.");
            Add("trait-diet", "Diet", "0.00 (grazer) to 1.00 (hunter). Higher values extract more energy from smaller cells and less from food pellets.");
            Add("trait-reproduction", "Reproduction threshold", "0.50 to 0.95 of max energy. Lower values divide sooner with smaller reserves passed to offspring.");
            Add("trait-sexual", "Sexual tendency", "Below 0.50 divides alone; 0.50 or above requires a compatible partner within sense range.");
            Add("trait-choosiness", "Choosiness", "0.00 to 1.00. Higher values restrict mating to genetically closer partners.");

            Add("inspect-energy", "Energy", "Current energy vs. capacity; reaching zero is fatal.");
            Add("inspect-age", "Age", "Simulated time since birth; older cells incur gradual senescence drain.");
            Add("inspect-generation", "Generation", "Lineage steps from a founder (generation 0).");
            Add("inspect-parent", "Parent ID", "ID of the parent cell, or 'none' for founders.");
            Add("inspect-trait", "Trait value", "Vertical tick marks the founder average.");
            Add("inspect-brain", "Neural outputs", "Live steering, thrust, bite, and mating signals from this cell's brain.");
            Add("legend-species", "Spotlight species", "Click to highlight members of this species in the water.");
            Add("legend-other", "Other lineages", "Combines smaller species outside the historical top 20.");
            Add("legend-cause", "Cumulative deaths", "Total deaths from this cause since the start of the run.");
            Add("legend-bin", "Value band", "Trait range represented by this colour band.");
            Add("stat-readout", "Live metric", "Updated from the active simulation.");
            Add("stat-sexual-share", "Sexual share", "Percentage of living cells with sexual tendency ≥ 0.50.");
            Add("temperature-legend", "Thermal scale", "Cold blue (0.0) to warm red (1.0).");
        }

        public static bool Has(string id) { return table.ContainsKey(id); }

        public static IEnumerable<string> Ids { get { return table.Keys; } }

        public static TipContent TraitTip(int trait)
        {
            return Tip(trait >= 0 && trait < TraitIds.Length ? TraitIds[trait] : "inspect-trait");
        }

        public static string Name(string id)
        {
            Entry e;
            return table.TryGetValue(id, out e) ? e.Name : id;
        }

        public static TipContent Tip(string id)
        {
            Entry e;
            if (!table.TryGetValue(id, out e)) return new TipContent { Name = id, Body = "", Key = null };
            return new TipContent { Name = e.Name, Body = e.Text, Key = e.Key };
        }

        public static string Body(string id)
        {
            return Tip(id).Body;
        }

        public static string Wrap(string text, int max)
        {
            var sb = new System.Text.StringBuilder();
            int line = 0;
            foreach (string word in text.Split(' '))
            {
                if (line > 0 && line + 1 + word.Length > max) { sb.Append('\n'); line = 0; }
                else if (line > 0) { sb.Append(' '); line++; }
                sb.Append(word);
                line += word.Length;
            }
            return sb.ToString();
        }

        public const string AppName = "Divergence";
        public const string AppSubtitle = "Natural selection in a continuous aquatic world";
        public const string WindowTooSmall = "Window too small. Enlarge the browser window to at least 1280 × 720.";
        public const string LoadFailedVersion = "This file cannot be loaded. It was made by a different version.";
        public const string LoadFailedGeneric = "This file cannot be loaded. It is not a Divergence Run.";
        public const string PopulationExtinct = "Population extinct";
        public const string PopulationCap = "at cap";
        public const string DownloadRequested = "Download requested.";
        public const string CsvDownloadRequested = "Exporting trait and species CSV files.";
    }
}
