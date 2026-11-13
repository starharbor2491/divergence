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

            Push(0, DesignTokens.AccentText,
                "Run started",
                Ui.Count(s.Cells.AliveCount) + " founders begin with near-identical white genomes.");

            if (s.Species.Records.Count > 1)
            {
                SpeciesRecord newest = s.Species.Records[s.Species.Records.Count - 1];
                Push(newest.BirthTime, new Color(newest.ColorR, newest.ColorG, newest.ColorB),
                    newest.Name + " branched off",
                    Ui.Count(s.Species.LivingCount) + " living species currently share the water.");
            }
        }

        public void Tick(float realDt)
        {
            if (sim == null) return;

            while (seenEvents < sim.Events.Log.Count)
            {
                EventRecord ev = sim.Events.Log[seenEvents++];
                PushEventNote(ev);
            }

            checkTimer += realDt;
            if (checkTimer < 1.0f) return;
            checkTimer = 0f;

            while (seenSpecies < sim.Species.Records.Count)
            {
                SpeciesRecord sp = sim.Species.Records[seenSpecies++];
                SpeciesRecord parent = sim.Species.Get(sp.ParentId);
                string from = parent != null ? " from " + parent.Name : "";
                Push(sim.Time, new Color(sp.ColorR, sp.ColorG, sp.ColorB),
                    "New lineage: " + sp.Name,
                    sp.Name + " branched" + from + " as accumulated mutations separated the two groups.");
            }

            if (sim.Species.ExtinctCount > seenExtinct)
            {
                seenExtinct = sim.Species.ExtinctCount;
                if (!sim.Extinct)
                {
                    Push(sim.Time, DesignTokens.StatusWarning,
                        "Lineage lost",
                        "One species died out while better-adapted neighbours persisted.");
                }
            }

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
                        string dir = now > before ? "rose" : "dropped";
                        Push(sim.Time, DesignTokens.AccentText,
                            Traits.Names[bestTrait] + " shift",
                            "Mean " + Traits.Names[bestTrait].ToLowerInvariant() + " " + dir + " from " + Ui.F(before, 2) + " to " + Ui.F(now, 2) + ".");
                    }
                }

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
                        Mathf.RoundToInt(100f * predators / sim.Cells.AliveCount) + "% of living cells now hunt smaller cells.");
                }
                if (!announcedSexual && sexual >= Mathf.Max(8, sim.Cells.AliveCount / 5))
                {
                    announcedSexual = true;
                    Push(sim.Time, DesignTokens.AccentText,
                        "Sexual reproduction",
                        Mathf.RoundToInt(100f * sexual / sim.Cells.AliveCount) + "% of living cells now require a compatible partner.");
                }
            }
        }

        void PushEventNote(EventRecord ev)
        {
            string scope = ev.WholeWorld ? "world-wide" : "in the target area";
            switch (ev.Kind)
            {
                case EventKind.HeatWave:
                    Push(ev.Time, DesignTokens.TempHot, "Heat wave",
                        "Local temperature rose " + scope + ", favouring warm-adapted cells.");
                    break;
                case EventKind.IceAge:
                    Push(ev.Time, DesignTokens.TempCold, "Ice age",
                        "Local temperature dropped " + scope + ", favouring cold-tolerant cells.");
                    break;
                case EventKind.Famine:
                    Push(ev.Time, DesignTokens.StatusWarning, "Famine",
                        "Food became scarce " + scope + ", rewarding efficient foraging.");
                    break;
                case EventKind.FoodBloom:
                    Push(ev.Time, DesignTokens.WorldFood, "Food bloom",
                        "Dense food appeared " + scope + ", accelerating reproduction.");
                    break;
                case EventKind.ToxinSpill:
                    Push(ev.Time, DesignTokens.WorldToxin, "Toxin spill",
                        "Released toxin cloud " + scope + ", selecting for higher resistance.");
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
