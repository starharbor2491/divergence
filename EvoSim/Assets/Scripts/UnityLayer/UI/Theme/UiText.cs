using System.Collections.Generic;

namespace Evo.UnityLayer
{
    public struct TipContent
    {
        public string Name;
        public string Body;
        public string Key;
    }

    /// <summary>The shared copy table: tooltips, chart captions, Preset descriptions and the Help screen, written once.</summary>
    public static class UiText
    {
        struct Entry { public string Name, Text; }

        static readonly Dictionary<string, Entry> table = new Dictionary<string, Entry>();

        static void Add(string id, string name, string text) { table[id] = new Entry { Name = name, Text = text }; }

        /// <summary>Canonical tooltip ids in Trait index order, including the two mating Traits.</summary>
        public static readonly string[] TraitIds =
        {
            "trait-size", "trait-speed", "trait-sense", "trait-temperature", "trait-tolerance",
            "trait-resistance", "trait-diet", "trait-reproduction", "trait-sexual", "trait-choosiness"
        };

        public static readonly string[] HelpTitles =
        {
            "What this is", "Controls", "Settings", "The World", "Traits",
            "Reproduction", "Species and colours", "Charts", "Events", "Save and exports"
        };

        public static readonly string[] HelpBodies =
        {
            "Divergence is a model of natural selection. Cells live in a World with Food, Toxin and varying Temperature. Cells that suit the World survive and reproduce, so their Traits become more common over generations. Over time the population can split into Species.",
            "Drag to move the view. Scroll to zoom. Click a Cell to inspect it and see its sense range. Space pauses. Keys 1 to 4 change speed. F fits the World. T boosts the thermal map. Tab hides the charts panel. Escape cancels or closes.",
            "The Seed fixes the World. Food abundance, Temperature range, zone scale, Toxin level and mutation rate shape it. Presets fill these in. The same Seed and settings always make the same starting World.",
            "Temperature varies smoothly from cold blue currents to hot coral zones. Food appears as glowing green pellets, more in some places than others. Toxin is a set of pink particles that drift, attract each other slightly and fade.",
            "Each Cell inherits ten Traits: size, speed, sense range, preferred Temperature, Temperature tolerance, Toxin resistance, diet, reproduction threshold, sexual tendency and choosiness. Traits have benefits and trade-offs, so no Cell is best everywhere. Hover a Trait for its meaning, units and trade-off.",
            "Below sexual tendency 0.50, a Cell divides at its energy threshold. At 0.50 or above, it needs a sexual partner within sense range with enough energy and mutual acceptance under both Cells' choosiness. Both pay energy; crossover takes each Trait and Brain weight from either parent, then mutation can make small random changes. Without a partner it waits; it never divides alone.",
            "Cells begin white. A Cell's colour comes from its Genome, so similar Cells have similar colours. Grazers show a green inner core while high-diet hunters grow warm amber cores and front mandibles. A Species is a group of Cells whose Genomes are close together; a new Species appears when a group drifts far enough apart.",
            "Traits shows one Trait over time or mean Generation, with a dashed line marking the Founders' starting average. Species shows the twenty Species with highest historical peaks; the rest form Other. Tree shows which Species split from which. Stats shows deaths in the latest ten minutes; legend totals cover the whole Run. Sexual share counts living Cells with sexual tendency at least 0.50, not births.",
            "Events change the World while a Run is going. Pick an Event from the bottom dock, set its size, then click the World to place it. Whole World applies without placement; for toxin spills, radius still sets quantity.",
            "Save requests a Run download. Load restores a valid saved file. Export CSV requests two data files; your browser may ask permission for multiple downloads. Screenshot requests a PNG. Check your browser's downloads; the app cannot confirm files reached disk."
        };

        static UiText()
        {
            // Controls
            Add("play", "Resume", "Resumes the simulation. Key: Space.");
            Add("pause", "Pause", "Freezes the simulation so you can pan, zoom and inspect Cells. Key: Space.");
            Add("speed-1", "Speed 1x", "Runs the simulation at normal real-time speed. Key: 1.");
            Add("speed-2", "Speed 2x", "Runs at twice normal speed. Key: 2.");
            Add("speed-3", "Speed 5x", "Runs at five times normal speed. Key: 3.");
            Add("speed-4", "Speed 10x", "Runs at ten times normal speed. On slow computers the real speed may be lower. Key: 4.");
            Add("events", "Events", "Places a heat wave, ice age, famine, food bloom or toxin spill in the World.");
            Add("temperature-tint", "Thermal map", "Boosts the colour overlay for Temperature: blue is cold, coral-red is hot. Key: T.");
            Add("fit-view", "Fit World", "Centres and zooms the camera to fit the whole World inside the stage. Key: F.");
            Add("field-notes", "Field notes", "Plain-English observations of natural selection, placed Events and Species splits as they happen.");
            Add("save", "Save run", "Requests a download of this Run for loading later. Check your browser's downloads before discarding it.");
            Add("load", "Load run", "Opens a saved Run file. A valid file replaces the current Run only after confirmation.");
            Add("csv", "Export CSV", "Requests two CSV downloads for recorded Trait and Species data. Your browser may ask permission for multiple downloads.");
            Add("screenshot", "Screenshot", "Requests a PNG download of the window. Check your browser's downloads for the file.");
            Add("help", "How it works", "Opens the illustrated guide to natural selection, Traits and controls.");
            Add("panel", "Charts panel", "Shows or hides the floating charts panel. Key: Tab.");
            Add("home", "Menu", "Opens a confirmation before returning to setup and discarding this Run.");
            Add("random-seed", "New random seed", "Fills the Seed field with a random code.");
            Add("restart", "Restart", "Opens a confirmation before replacing this Run with a new one using the same Seed and settings.");
            Add("close", "Close", "Closes this window. Key: Escape.");
            Add("inspector-close", "Close", "Stops inspecting this Cell. Key: Escape.");
            Add("confirm-cancel", "Cancel", "Keeps the current Run without discarding or replacing it. Key: Escape.");
            Add("confirm-discard", "Discard", "Discards the current Run and continues the chosen action. Check that any requested save is in your browser's downloads first.");
            Add("confirm-save", "Save", "Requests a Run download and keeps the current Run. Check your browser's downloads before choosing Discard; the app cannot confirm a disk write.");

            // Setup & Presets
            Add("preset", "Preset", "A ready-made set of settings and a Seed. Changing any setting afterwards switches to Custom.");
            Add("preset-mild", "Mild world", "Gentle Temperature variation and steady Food. Ideal first Run to watch gradual Mutation and colour drift.");
            Add("preset-gradient", "Cold gradient", "Strong contrast between cold and hot regions. Watch Cells adapt different preferred Temperatures by zone.");
            Add("preset-basin", "Toxic basin", "Dense drifting Toxin clouds across the World. Creates strong Pressure for higher Toxin resistance.");
            Add("preset-rapid", "Rapid evolution", "Abundant Food, sharp Temperature zones and double mutation rate for fast Trait shifts and Species splits.");
            Add("seed", "Seed", "A code that fixes the World. The same Seed and settings always start the same World.");
            Add("food-abundance", "Food abundance", "How fast Food appears. Higher values support more Cells. Range 0.20 to 2.00.");
            Add("temperature-range", "Temperature range", "How different the coldest and hottest parts of the World are. 0 is uniform, 1 is the strongest contrast.");
            Add("zone-scale", "Zone scale", "How large the warm and cold zones are. Lower values make large zones, higher values make smaller pockets.");
            Add("toxin-level", "Toxin level", "How much Toxin the World starts with. 0 means none.");
            Add("mutation-rate", "Mutation rate", "How much Traits and Brains change when a Cell is born. 1.00 is normal.");
            Add("start", "Start run", "Starts a new Run in this World.");

            // Tabs and charts
            Add("tab-traits", "Traits", "Shows how one Trait changes in the living population over time or Generations.");
            Add("tab-species", "Species", "Shows how many Cells belong to each Species over time.");
            Add("tab-tree", "Tree", "Shows which Species split from which, and when.");
            Add("tab-stats", "Stats", "Shows live counts and why Cells die.");
            Add("trait-select", "Trait", "Chooses which Trait the charts show.");
            Add("axis-switch", "X axis", "Plots time in seconds, or the actual mean Generation of living Cells. The mean can decrease as the population changes, so the line can turn back.");
            Add("chart-trait-mean", "Mean and spread", "The line is the average value of the Trait; the shaded band is one standard deviation either side. The dashed line is the Founders' starting average.");
            Add("chart-trait-frequency", "Frequency", "Shows what share of Cells has each range of the Trait. Darker shades are lower values.");
            Add("chart-species", "Species over time", "Band height is population for each of the twenty Species with highest historical peaks. Other combines the rest; hover identifies the plotted layers.");
            Add("chart-tree", "Family tree", "Each line is a Species from when it first appeared to when it died out. Curved branches show where a new Species split off.");
            Add("chart-deaths", "Deaths by cause", "Shows deaths by cause in 10-second bins from the latest ten minutes. Legend totals count deaths over the whole Run.");
            Add("event-marker", "Event marker", "Marks when an Event happened. H heat wave, I ice age, F famine, B food bloom, T toxin spill.");

            // Events
            Add("event-heat-wave", "Heat wave", "Raises Temperature inside the circle, then lets it return to normal.");
            Add("event-ice-age", "Ice age", "Lowers Temperature inside the circle, then lets it return to normal.");
            Add("event-famine", "Famine", "Removes most Food inside the circle and slows new Food there for a while.");
            Add("event-food-bloom", "Food bloom", "Adds extra Food inside the circle for a while.");
            Add("event-toxin-spill", "Toxin spill", "Releases Toxin inside the circle. It spreads slowly and fades.");
            Add("event-radius", "Radius", "Radius in World units. For a toxin spill it also sets the requested particle count; Whole World changes placement, not quantity.");
            Add("event-toxin-amount", "Toxin quantity", "By default, radius requests 30 Toxin particles per World unit: 150 to 1800 for radius 5 to 60. Remaining global capacity can reduce the actual count; Whole World changes positions, not quantity.");
            Add("event-intensity", "Intensity", "How far Temperature moves from normal at the centre.");
            Add("event-duration", "Duration", "How long the Event lasts at full strength, in seconds.");
            Add("event-whole-world", "Whole World", "Applies the Event everywhere instead of in one place.");
            Add("event-place", "Place", "Closes this window. Then click the World to place the Event.");
            Add("event-apply", "Apply", "Applies the Event to the whole World at once.");
            Add("event-cancel", "Cancel", "Closes this window without placing an Event. Key: Escape.");

            // Trait meaning, units and trade-offs
            Add("trait-size", "Size", "Relative size, 0.50 to 3.00; radius is half this value in World units. Larger Cells store more energy and can eat smaller Cells, but cost more energy to sustain and move.");
            Add("trait-speed", "Speed", "Maximum movement speed, 0.50 to 4.00 World units per second. Faster movement helps reach Food or escape, but movement energy cost rises with speed squared.");
            Add("trait-sense", "Sense range", "Detection radius for Food, Cells and Toxin, 2.00 to 15.00 World units. A wider range finds things farther away but increases baseline energy cost.");
            Add("trait-temperature", "Preferred Temperature", "Best-suited Temperature on the 0.00 cold to 1.00 hot scale. Matching the local Temperature avoids stress; mismatches beyond tolerance cost energy.");
            Add("trait-tolerance", "Temperature tolerance", "Allowed Temperature mismatch without stress, 0.05 to 0.50 on the Temperature scale. Wider tolerance reduces stress in more places but raises baseline energy cost.");
            Add("trait-resistance", "Toxin resistance", "Unitless resistance, 0.00 to 1.00, reducing Toxin energy drain by up to 90%. Higher resistance protects against Toxin but raises baseline energy cost.");
            Add("trait-diet", "Diet", "Unitless diet, 0.00 to 1.00: low values favour Food pellets, high values favour eating smaller Cells. Higher values gain more energy from prey but less from Food; predation also needs sufficient size and biting.");
            Add("trait-reproduction", "Reproduction threshold", "Energy needed to initiate reproduction, 0.50 to 0.95 of maximum energy. A lower threshold allows earlier reproduction but leaves less energy to pass to offspring.");
            Add("trait-sexual", "Sexual tendency", "Unitless tendency, 0.00 to 1.00: below 0.50 divides alone; at least 0.50 requires a sexual partner. Crossover combines two Genomes, but both parents pay energy and a Cell without a compatible partner waits.");
            Add("trait-choosiness", "Choosiness", "Unitless partner selectivity, 0.00 to 1.00, used by sexual Cells. Higher values accept only closer Genomes, preserving similarity but reducing the number of compatible partners.");

            // Inspector and legend
            Add("inspect-energy", "Energy", "The Cell's stored energy and its maximum. It dies at zero.");
            Add("inspect-age", "Age", "Time since the Cell was born. Old Cells lose extra energy.");
            Add("inspect-generation", "Generation", "Birth depth from a Founder, whose Generation is zero. A sexual child's Generation is one more than the higher parent Generation.");
            Add("inspect-parent", "Parent ID", "The recorded parent's ID, not a Generation. 'none' means a Founder; sexual births record only the initiating parent's ID.");
            Add("inspect-trait", "Trait bar", "The Trait value inside its allowed range. The tick marks the Founders' average.");
            Add("inspect-brain", "Brain activity", "What the Cell's Brain is telling it to do right now: steer left or right, swim forward, bite prey, or court a partner.");
            Add("legend-species", "Species", "Click to spotlight this Species' Cells in the World. Click again to clear.");
            Add("legend-other", "Other", "The chart draws the twenty Species with highest peak populations over the Run; all others form Other. Individual living Species rows still highlight their Cells, including Species inside Other.");
            Add("legend-cause", "Cause of death", "This cause's legend count covers the whole Run, not just the latest ten minutes shown in the chart.");
            Add("legend-bin", "Trait range", "The range of Trait values that this shade covers.");
            Add("stat-readout", "Live count", "A live count taken from the current Run.");
            Add("stat-sexual-share", "Sexual share", "Share of living Cells whose sexual tendency is at least 0.50. This is not the share of births from sexual reproduction.");
            Add("temperature-legend", "Temperature legend", "Blue is cold and coral-red is hot. The thermal map is drawn over the World.");
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
            string body = e.Text;
            string key = null;
            int k = body.LastIndexOf(" Key: ", System.StringComparison.Ordinal);
            if (k >= 0)
            {
                key = body.Substring(k + 1).TrimEnd('.');
                body = body.Substring(0, k);
            }
            return new TipContent { Name = e.Name, Body = body, Key = key };
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
        public const string AppSubtitle = "Watch natural selection shape Traits and split Species in a living World";
        public const string WindowTooSmall = "Window too small. Enlarge the window to at least 1280 by 720.";
        public const string LoadFailedVersion = "This file cannot be loaded. It was made by a different version.";
        public const string LoadFailedGeneric = "This file cannot be loaded. It is not a Divergence Run.";
        public const string PopulationExtinct = "Population extinct";
        public const string PopulationCap = "population cap";
        public const string DownloadRequested = "Download requested. Check your browser's downloads.";
        public const string CsvDownloadRequested = "Two CSV downloads requested. Your browser may ask permission for multiple downloads.";
    }
}
