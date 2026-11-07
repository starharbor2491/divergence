using System.Collections.Generic;

namespace Evo.UnityLayer
{
    public struct TipContent
    {
        public string Name;
        public string Body;
        public string Key;
    }

    /// <summary>The copy table of DESIGN.md section 8: tooltips, chart captions and the Help screen, written once.</summary>
    public static class UiText
    {
        struct Entry { public string Name, Text; }

        static readonly Dictionary<string, Entry> table = new Dictionary<string, Entry>();

        static void Add(string id, string name, string text) { table[id] = new Entry { Name = name, Text = text }; }

        public static readonly string[] HelpTitles =
        {
            "What this is", "Controls", "Settings", "The World", "Traits",
            "Reproduction", "Species and colours", "Charts", "Events", "Save and exports"
        };

        public static readonly string[] HelpBodies =
        {
            "Divergence is a model of natural selection. Cells live in a World with Food, Toxin and varying Temperature. Cells that suit the World survive and reproduce, so their Traits become more common over generations. Over time the population can split into Species.",
            "Drag to move the view. Scroll to zoom. Click a Cell to inspect it. Space pauses. Keys 1 to 4 change speed. F fits the World. T shows Temperature. Tab hides the side panel. Escape cancels or closes.",
            "The Seed fixes the World. Food abundance, Temperature range, zone scale, Toxin level and mutation rate shape it. Presets fill these in. The same Seed and settings always make the same starting World.",
            "Temperature varies smoothly from cold to hot. Food appears as pellets, more in some places than others. Toxin is a set of particles that drift, attract each other slightly and fade.",
            "Each Cell inherits ten Traits: size, speed, sense range, preferred Temperature, Temperature tolerance, Toxin resistance, diet, reproduction threshold, and two mating Traits. Each Trait helps in some way and costs energy in another, so no Cell is best everywhere.",
            "A Cell divides when it has stored enough energy. The child gets a copy of the Genome with small random changes called mutations. Mutations are how new Traits and new Brains appear.",
            "Cells begin white. A Cell's colour comes from its Genome, so similar Cells have similar colours. A Species is a group of Cells whose Genomes are close together. A new Species appears when a group drifts far enough from the rest.",
            "Traits shows one Trait over time. Species shows population by Species. Tree shows which Species came from which. Stats shows counts and causes of death.",
            "Events change the World while a Run is going. Choose an Event, set its size, then click the World to place it.",
            "Save downloads the Run as a file. Load restores it exactly. Export CSV downloads the recorded data. Screenshot downloads an image of the window."
        };

        static UiText()
        {
            // Controls
            Add("play", "Resume", "Resumes the simulation. Key: Space.");
            Add("pause", "Pause", "Freezes the simulation. You can still pan, zoom and select Cells. Key: Space.");
            Add("speed-1", "Speed 1x", "Runs the simulation at normal speed. Key: 1.");
            Add("speed-2", "Speed 2x", "Runs at twice normal speed. Key: 2.");
            Add("speed-3", "Speed 5x", "Runs at five times normal speed. Key: 3.");
            Add("speed-4", "Speed 10x", "Runs at ten times normal speed. On slow computers the real speed may be lower. Key: 4.");
            Add("events", "Events", "Places a heat wave, ice age, famine, food bloom or toxin spill in the World.");
            Add("temperature-tint", "Temperature tint", "Shows or hides the colour overlay for Temperature: blue is cold, red is hot. Key: T.");
            Add("save", "Save run", "Downloads this Run as a file you can load later, even in another browser.");
            Add("load", "Load run", "Loads a Run from a saved file and replaces the current Run.");
            Add("csv", "Export CSV", "Downloads the recorded Trait and Species data as two CSV files.");
            Add("screenshot", "Screenshot", "Downloads a PNG image of the window.");
            Add("help", "How it works", "Opens the Help screen.");
            Add("panel", "Side panel", "Shows or hides the charts. Key: Tab.");
            Add("home", "Menu", "Returns to the setup screen. The current Run is discarded unless you saved it.");
            Add("random-seed", "New random seed", "Fills the Seed field with a random code.");
            Add("restart", "Restart", "Starts a new Run with the same Seed and settings.");
            Add("close", "Close", "Closes this window. Key: Escape.");
            Add("inspector-close", "Close", "Stops inspecting this Cell. Key: Escape.");

            // Setup
            Add("preset", "Preset", "A ready-made set of settings and a Seed. Changing any setting afterwards switches this to Custom.");
            Add("seed", "Seed", "A code that fixes the World. The same Seed and settings always start the same World.");
            Add("food-abundance", "Food abundance", "How fast Food appears. Higher values support more Cells. Range 0.20 to 2.00.");
            Add("temperature-range", "Temperature range", "How different the coldest and hottest parts of the World are. 0 is uniform, 1 is the strongest contrast.");
            Add("zone-scale", "Zone scale", "How large the warm and cold zones are. Lower values make large zones, higher values make small ones.");
            Add("toxin-level", "Toxin level", "How much Toxin the World starts with. 0 means none.");
            Add("mutation-rate", "Mutation rate", "How much Traits and Brains change when a Cell is born. 1.00 is normal.");
            Add("start", "Start", "Starts a new Run with these settings.");

            // Tabs and charts
            Add("tab-traits", "Traits", "Shows how one Trait changes in the living population over time.");
            Add("tab-species", "Species", "Shows how many Cells belong to each Species over time.");
            Add("tab-tree", "Tree", "Shows which Species split from which, and when.");
            Add("tab-stats", "Stats", "Shows live counts and why Cells die.");
            Add("trait-select", "Trait", "Chooses which Trait the charts show.");
            Add("axis-switch", "X axis", "Plots time in seconds, or the mean Generation of the living Cells.");
            Add("chart-trait-mean", "Mean and spread", "The line is the average value of the Trait. The shaded band is one standard deviation either side.");
            Add("chart-trait-frequency", "Frequency", "Shows what share of Cells has each range of the Trait. Darker bins are lower values.");
            Add("chart-species", "Species over time", "Each colour is one Species. The height of a band is its population.");
            Add("chart-tree", "Family tree", "Each line is a Species from when it first appeared to when it died out. Branches show where a Species split off.");
            Add("chart-deaths", "Deaths by cause", "Counts of Cells that died in each 10 second period, by cause.");
            Add("event-marker", "Event marker", "Marks when an Event happened. H heat wave, I ice age, F famine, B food bloom, T toxin spill.");

            // Events
            Add("event-heat-wave", "Heat wave", "Raises Temperature inside the circle, then lets it return to normal.");
            Add("event-ice-age", "Ice age", "Lowers Temperature inside the circle, then lets it return to normal.");
            Add("event-famine", "Famine", "Removes most Food inside the circle and slows new Food there for a while.");
            Add("event-food-bloom", "Food bloom", "Adds extra Food inside the circle for a while.");
            Add("event-toxin-spill", "Toxin spill", "Releases Toxin inside the circle. It spreads slowly and fades.");
            Add("event-radius", "Radius", "The size of the area affected, in World units.");
            Add("event-intensity", "Intensity", "How far Temperature moves from normal at the centre.");
            Add("event-duration", "Duration", "How long the Event lasts at full strength, in seconds.");
            Add("event-whole-world", "Whole World", "Applies the Event everywhere instead of in one place.");
            Add("event-place", "Place", "Closes this window. Then click the World to place the Event.");
            Add("event-apply", "Apply", "Applies the Event to the whole World at once.");
            Add("event-cancel", "Cancel", "Closes this window without placing an Event. Key: Escape.");

            // Inspector and legend
            Add("inspect-energy", "Energy", "The Cell's stored energy and its maximum. It dies at zero.");
            Add("inspect-age", "Age", "Time since the Cell was born. Old Cells lose extra energy.");
            Add("inspect-generation", "Generation", "How many births separate this Cell from a Founder.");
            Add("inspect-trait", "Trait bar", "The Trait value inside its allowed range. The tick marks the Founders' average.");
            Add("inspect-brain", "Brain output", "What the Cell's Brain is asking it to do now: turn, move, bite.");
            Add("legend-species", "Species", "Click to highlight this Species' Cells in the World. Click again to clear.");
            Add("legend-cause", "Cause of death", "The colour used for this cause in the Deaths chart.");
            Add("legend-bin", "Trait range", "The range of Trait values that this shade covers.");
            Add("stat-readout", "Live count", "A live count taken from the current Run.");
            Add("temperature-legend", "Temperature legend", "Blue is cold and red is hot. The tint is drawn over the World.");
        }

        public static bool Has(string id) { return table.ContainsKey(id); }

        public static IEnumerable<string> Ids { get { return table.Keys; } }

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

        /// <summary>Wraps text to a maximum line length by inserting line feeds (the Help screen uses 70).</summary>
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

        // Plain interface strings that are not tooltips
        public const string AppName = "divergence";
        public const string AppSubtitle = "a natural selection simulation";
        public const string WindowTooSmall = "Window too small. Enlarge the window to at least 1280 by 720.";
        public const string LoadFailedVersion = "This file cannot be loaded. It was made by a different version.";
        public const string LoadFailedGeneric = "This file cannot be loaded. It is not a Divergence Run.";
        public const string PopulationExtinct = "Population extinct";
        public const string PopulationCap = "population cap reached";
    }
}
