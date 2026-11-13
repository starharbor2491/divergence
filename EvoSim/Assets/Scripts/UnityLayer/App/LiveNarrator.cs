using System.Collections.Generic;
using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    public struct FieldNote
    {
        public double Time;
        public Color Dot;
        public string Title;
        public string Body;
    }

    /// <summary>
    /// Pure read-only observer of a Simulation that translates real biological events (placed Pressures, speciation splits,
    /// extinctions, and significant Trait adaptations) into plain-English classroom Field Notes using CONTEXT.md terms.
    /// </summary>
    public sealed class LiveNarrator
    {
        const int MaxNotes = 12;

        public readonly List<FieldNote> Notes = new List<FieldNote>();
        public int Version { get; private set; }

        Simulation sim;
        int seenEvents;
        int seenSpecies;
        int seenExtinct;
        float[] announcedMean = new float[Traits.ChartCount];
        bool announcedPredation;
        bool announcedSexual;
        float checkTimer;

        public void Bind(Simulation s)
        {
            sim = s;
            Notes.Clear();
            Version++;
            if (s == null) return;

            seenEvents = s.Events.Log.Count;
            seenSpecies = s.Species.Records.Count;
            seenExtinct = s.Species.ExtinctCount;
            for (int i = 0; i < Traits.ChartCount; i++) announcedMean[i] = Traits.FounderMean[i];
            announcedPredation = false;
            announcedSexual = false;
            checkTimer = 0f;

            Push(0, DesignTokens.Accent,
                "Founders released",
                Ui.Count(s.Cells.AliveCount) + " Founder Cells start near white. Mutations shift their Traits and colours over Generations.");

            // If binding a Run that already has history (loaded Run or synthetic fixture), summarise the latest milestone
            if (s.Species.Records.Count > 1)
            {
                SpeciesRecord newest = s.Species.Records[s.Species.Records.Count - 1];
                Push(newest.BirthTime, new Color(newest.ColorR, newest.ColorG, newest.ColorB),
                    newest.Name + " diverged",
                    Ui.Count(s.Species.LivingCount) + " living Species now coexist across the World.");
            }
        }

        public void Tick(float realDt)
        {
            if (sim == null) return;

            // Immediate check for newly placed Events
            while (seenEvents < sim.Events.Log.Count)
            {
                EventRecord ev = sim.Events.Log[seenEvents++];
                PushEventNote(ev);
            }

            checkTimer += realDt;
            if (checkTimer < 1.0f) return;
            checkTimer = 0f;

            // Speciation splits
            while (seenSpecies < sim.Species.Records.Count)
            {
                SpeciesRecord sp = sim.Species.Records[seenSpecies++];
                SpeciesRecord parent = sim.Species.Get(sp.ParentId);
                string from = parent != null ? " from " + parent.Name : "";
                Push(sim.Time, new Color(sp.ColorR, sp.ColorG, sp.ColorB),
                    "Speciation: " + sp.Name,
                    sp.Name + " split" + from + " after its Genome drifted far enough apart.");
            }

            // Extinctions
            if (sim.Species.ExtinctCount > seenExtinct)
            {
                seenExtinct = sim.Species.ExtinctCount;
                if (!sim.Extinct)
                {
                    Push(sim.Time, DesignTokens.StatusWarning,
                        "Lineage extinct",
                        "A less-suited Species died out while better-adapted Species kept reproducing.");
                }
            }

            // Trait adaptation & lifestyle milestones
            var samples = sim.Stats.Samples;
            if (samples.Count > 0 && sim.Cells.AliveCount >= 12)
            {
                Sample latest = samples[samples.Count - 1];
                if (latest.MeanGeneration >= 1.2f)
                {
                    int bestTrait = -1;
                    float bestShift = 0.13f;
                    for (int t = 0; t < Traits.ChartCount; t++)
                    {
                        float normShift = Mathf.Abs(latest.TraitMean[t] - announcedMean[t]) / Traits.Range(t);
                        if (normShift > bestShift)
                        {
                            bestShift = normShift;
                            bestTrait = t;
                        }
                    }
                    if (bestTrait >= 0)
                    {
                        float before = announcedMean[bestTrait];
                        float now = latest.TraitMean[bestTrait];
                        announcedMean[bestTrait] = now;
                        string dir = now > before ? "rose" : "fell";
                        Push(sim.Time, DesignTokens.Accent,
                            "Trait shift: " + Traits.Names[bestTrait],
                            "Average " + Traits.Names[bestTrait].ToLowerInvariant() + " " + dir + " from " + Ui.F(before, 2) + " to " + Ui.F(now, 2) + " as better-suited Cells survived.");
                    }
                }

                // Lifestyle emergence
                int predators = 0, sexual = 0;
                for (int i = 0; i < sim.Cells.AliveCount; i++)
                {
                    Cell c = sim.Cells.Alive[i];
                    if (c.G.Trait[TraitId.Diet] >= 0.45f) predators++;
                    if (c.G.Trait[TraitId.SexualTendency] >= sim.P.SexThreshold) sexual++;
                }
                if (!announcedPredation && predators >= Mathf.Max(8, sim.Cells.AliveCount / 6))
                {
                    announcedPredation = true;
                    Push(sim.Time, DesignTokens.WorldPredatorCore,
                        "Predation emerging",
                        Mathf.RoundToInt(100f * predators / sim.Cells.AliveCount) + "% of Cells evolved high diet Traits and hunt smaller Cells.");
                }
                if (!announcedSexual && sexual >= Mathf.Max(8, sim.Cells.AliveCount / 5))
                {
                    announcedSexual = true;
                    Push(sim.Time, DesignTokens.AccentText,
                        "Sexual reproduction spreading",
                        Mathf.RoundToInt(100f * sexual / sim.Cells.AliveCount) + "% of Cells now combine Genomes with compatible partners.");
                }
            }
        }

        void PushEventNote(EventRecord ev)
        {
            string scope = ev.WholeWorld ? "across the whole World" : "in the target zone";
            switch (ev.Kind)
            {
                case EventKind.HeatWave:
                    Push(ev.Time, DesignTokens.TempHot, "Heat wave active",
                        "Temperature rose " + scope + ". Warm-suited Cells save energy and reproduce more.");
                    break;
                case EventKind.IceAge:
                    Push(ev.Time, DesignTokens.TempCold, "Ice age active",
                        "Temperature dropped " + scope + ". Cold-suited Cells hold the survival advantage.");
                    break;
                case EventKind.Famine:
                    Push(ev.Time, DesignTokens.StatusWarning, "Famine active",
                        "Food dropped sharply " + scope + ". Efficient Cells with wider sense range endure best.");
                    break;
                case EventKind.FoodBloom:
                    Push(ev.Time, DesignTokens.WorldFood, "Food bloom active",
                        "Extra Food appeared " + scope + ", fuelling rapid births and new Mutations.");
                    break;
                case EventKind.ToxinSpill:
                    Push(ev.Time, DesignTokens.WorldToxin, "Toxin spill released",
                        "Drifting Toxin drains nearby Cells. Higher Toxin resistance reduces the damage.");
                    break;
            }
        }

        void Push(double time, Color dot, string title, string body)
        {
            Notes.Insert(0, new FieldNote { Time = time, Dot = dot, Title = title, Body = body });
            if (Notes.Count > MaxNotes) Notes.RemoveAt(Notes.Count - 1);
            Version++;
        }
    }
}
