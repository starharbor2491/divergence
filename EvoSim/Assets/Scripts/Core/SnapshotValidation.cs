using System;
using System.Collections.Generic;

namespace Evo.Core
{
    /// <summary>Save-file trust boundary. Check lengths and allocation bounds before constructing a Simulation.</summary>
    public static class SnapshotValidation
    {
        public const int MaxJsonCharacters = 64 * 1024 * 1024;
        public const long MaxFileBytes = 64 * 1024 * 1024;
        const int MaxRecords = 10000;
        const int MaxDeathBins = 1000000;
        const int MaxCounter = 1000000000;
        const double MaxTime = 10000000;

        internal static bool Range(double value, double min, double max)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max;
        }

        static void Need(bool valid)
        {
            if (!valid) throw new FormatException("Invalid saved Run state.");
        }

        static void Length<T>(T[] values, int max, int exact = -1)
        {
            Need(values != null && values.Length <= max && (exact < 0 || values.Length == exact));
        }

        static void Floats(float[] values, int length, double min, double max)
        {
            Length(values, length, length);
            foreach (float value in values) Need(Range(value, min, max));
        }

        static void Counts(int[] values, int maxLength, int exact = -1, int maxValue = MaxCounter)
        {
            Length(values, maxLength, exact);
            long sum = 0;
            foreach (int value in values) { Need(Range(value, 0, maxValue)); sum += value; }
            Need(sum <= MaxCounter);
        }

        static void Genome(float[] traits, float[] weights)
        {
            Length(traits, Traits.Count, Traits.Count);
            for (int i = 0; i < traits.Length; i++)
                Need(Range(traits[i], Traits.Min[i] - 0.00001, Traits.Max[i] + 0.00001));
            Floats(weights, BrainLayout.WeightCount, -1000, 1000);
        }

        static void Rng(RngState state)
        {
            Need(state != null && (state.S0 | state.S1 | state.S2 | state.S3) != 0 && Range(state.Spare, -100, 100));
        }

        static void Event(EventKind kind, float x, float y, float radius, float intensity, float duration, SimParameters p)
        {
            Need((int)kind >= 0 && (int)kind <= (int)EventKind.ToxinSpill &&
                Range(x, 0, p.WorldWidth) && Range(y, 0, p.WorldHeight) && Range(radius, 0.001, 1000) &&
                Range(intensity, 0, 1) && Range(duration, 0, 10000));
        }

        public static void Check(SimSnapshot s)
        {
            Need(s != null && s.FormatVersion == SimSnapshot.CurrentFormatVersion &&
                s.Params != null && s.Params.Validate() && s.Setup != null);
            SimParameters p = s.Params;
            SetupSettings setup = s.Setup;
            Need(setup.SeedText != null && setup.SeedText.Length <= 4096 &&
                Range(setup.FoodAbundance, 0.2, 2) && Range(setup.TemperatureRange, 0, 1) &&
                Range(setup.ZoneScale, 0.5, 2) && Range(setup.ToxinLevel, 0, 1) && Range(setup.MutationRate, 0, 3));
            Need(s.StepCount >= 0 && Range(s.StepCount * (double)p.Dt, 0, MaxTime) &&
                s.StepCount * (double)p.Dt / p.DeathBinSeconds < MaxDeathBins - 1 &&
                Range(s.NextCellId, 0, MaxCounter) && Range(s.FoodAcc, 0, 1));
            Rng(s.WorldRng); Rng(s.SimRng);
            Length(s.Cells, p.MaxCells);
            Need(!s.Extinct || s.Cells.Length == 0);
            Length(s.FoodX, p.MaxFood);
            Floats(s.FoodX, s.FoodX.Length, 0, p.WorldWidth);
            Floats(s.FoodY, s.FoodX.Length, 0, p.WorldHeight);
            Length(s.ToxX, p.MaxToxin);
            int tox = s.ToxX.Length;
            Floats(s.ToxX, tox, 0, p.WorldWidth); Floats(s.ToxY, tox, 0, p.WorldHeight);
            Floats(s.ToxVX, tox, -10000, 10000); Floats(s.ToxVY, tox, -10000, 10000);
            Floats(s.ToxS, tox, 0, 1); Floats(s.ToxHalf, tox, double.Epsilon, 10000);

            Length(s.Species, MaxRecords);
            Need(s.NextSpeciesId == s.Species.Length + 1);
            var speciesIds = new HashSet<int>();
            foreach (SpeciesSnap r in s.Species)
            {
                Need(r != null && r.Id > 0 && r.Id < s.NextSpeciesId && speciesIds.Add(r.Id) &&
                    Range(r.ParentId, 0, r.Id - 1) && Range(r.BirthGeneration, 0, MaxCounter) &&
                    r.Name != null && r.Name.Length <= 4096 && Range(r.BirthTime, 0, MaxTime) &&
                    (r.ExtinctTime == -1 || Range(r.ExtinctTime, r.BirthTime, MaxTime)) &&
                    Range(r.PeakPopulation, 0, p.MaxCells) && Range(r.CurrentPopulation, 0, r.PeakPopulation) &&
                    Range(r.LastPopulation, 0, r.PeakPopulation) && Range(r.EmptyChecks, 0, MaxCounter));
                Genome(r.Trait, r.Weights);
            }
            var cellIds = new HashSet<int>();
            foreach (CellSnap c in s.Cells)
            {
                Need(c != null && c.Id >= 0 && c.Id < s.NextCellId && cellIds.Add(c.Id) &&
                    Range(c.ParentId, -1, c.Id - 1) && Range(c.Generation, 0, MaxCounter) &&
                    (c.SpeciesId == 0 || speciesIds.Contains(c.SpeciesId)) &&
                    Range(c.BiteTargetId, -1, s.NextCellId - 1) && c.MatedStep >= -1 && c.MatedStep <= s.StepCount &&
                    Range(c.X, 0, p.WorldWidth) && Range(c.Y, 0, p.WorldHeight) &&
                    Range(c.Heading, -10000, 10000) && Range(c.Age, 0, MaxTime) &&
                    Range(c.Energy, 0, p.MaxEnergyPerSize * 9) && Range(c.BiteTimer, 0, 10000) &&
                    Range(c.DrainBase, 0, 1000000) && Range(c.DrainTemperature, 0, 1000000) &&
                    Range(c.DrainToxin, 0, 1000000) && Range(c.DrainAge, 0, 100000000000) &&
                    Range(c.OutTurn, -1, 1) && Range(c.OutThrust, 0, 1) &&
                    Range(c.OutBite, -1, 1) && Range(c.OutMate, -1, 1));
                Genome(c.Trait, c.Weights);
            }
            Length(s.Pending, 48);
            foreach (CandidateSnap candidate in s.Pending)
            {
                Need(candidate != null && Range(candidate.Size, 1, p.MaxCells));
                Genome(candidate.Trait, candidate.Weights);
            }
            Length(s.Events, p.MaxActiveEvents);
            foreach (ActiveEventSnap active in s.Events)
            {
                Need(active != null && active.Spec != null && Range(active.Age, 0, 30000) &&
                    Range(active.Accumulator, 0, 1) && Range(active.PlacedTime, 0, MaxTime));
                EventSpec spec = active.Spec;
                Event(spec.Kind, spec.X, spec.Y, spec.Radius, spec.Intensity, spec.Duration, p);
                Need(spec.Kind != EventKind.ToxinSpill);
            }
            Length(s.EventLog, MaxRecords);
            double previousEvent = -1;
            foreach (EventRecord record in s.EventLog)
            {
                Need(record != null && Range(record.Time, previousEvent, MaxTime));
                Event(record.Kind, record.X, record.Y, record.Radius, record.Intensity, record.Duration, p);
                previousEvent = record.Time;
            }
            StatsSnap stats = s.Stats;
            Need(stats != null && stats.IntervalSteps > 0 && Range(stats.TotalBirths, 0, MaxCounter) &&
                Range(stats.MaxGenerationEver, 0, MaxCounter) && Range(stats.PeakCells, 0, p.MaxCells) &&
                Range(stats.RollIndex, 0, 59) && Range(stats.SecBirths, 0, MaxCounter) && Range(stats.SecDeaths, 0, MaxCounter));
            Counts(stats.TotalDeaths, DeathCauses.Count, DeathCauses.Count);
            Counts(stats.BirthsPerSecond, 60, 60); Counts(stats.DeathsPerSecond, 60, 60);
            Counts(stats.DeathBins, MaxDeathBins * DeathCauses.Count);
            Need(stats.DeathBins.Length % DeathCauses.Count == 0 &&
                Range(stats.DeathBinCount, 0, stats.DeathBins.Length / DeathCauses.Count));
            Length(stats.Samples, p.MaxSamples);
            Need(stats.Samples.Length > 0);
            float previousTime = -1;
            foreach (Sample sample in stats.Samples)
            {
                Need(sample != null && Range(sample.Time, previousTime, MaxTime) && Range(sample.Count, 0, p.MaxCells) &&
                    Range(sample.MeanGeneration, 0, MaxCounter) && Range(sample.MaxGeneration, 0, MaxCounter) &&
                    Range(sample.Births, 0, stats.TotalBirths) && Range(sample.SexualFraction, 0, 1) &&
                    Range(sample.LivingSpecies, 0, s.Species.Length) && Range(sample.ExtinctSpecies, 0, s.Species.Length));
                previousTime = sample.Time;
                Counts(sample.Deaths, DeathCauses.Count, DeathCauses.Count);
                Floats(sample.TraitMean, Traits.ChartCount, 0, 100);
                Floats(sample.TraitSd, Traits.ChartCount, 0, 100);
                for (int t = 0; t < Traits.ChartCount; t++)
                    Need((sample.Count == 0 ? sample.TraitMean[t] == 0 :
                        Range(sample.TraitMean[t], Traits.Min[t] - 0.00001, Traits.Max[t] + 0.00001)) &&
                        sample.TraitSd[t] <= Traits.Range(t));
                Counts(sample.Bins, Traits.ChartCount * TraitBins.Count, Traits.ChartCount * TraitBins.Count, p.MaxCells);
                for (int t = 0; t < Traits.ChartCount; t++)
                {
                    int total = 0;
                    for (int b = 0; b < TraitBins.Count; b++) total += sample.Bins[t * TraitBins.Count + b];
                    Need(total == sample.Count);
                }
                Length(sample.SpeciesIds, p.MaxCells);
                Counts(sample.SpeciesPops, p.MaxCells, sample.SpeciesIds.Length, p.MaxCells);
                var ids = new HashSet<int>();
                int population = 0;
                for (int i = 0; i < sample.SpeciesIds.Length; i++)
                {
                    Need(speciesIds.Contains(sample.SpeciesIds[i]) && ids.Add(sample.SpeciesIds[i]));
                    population += sample.SpeciesPops[i];
                }
                Need(population <= sample.Count);
            }
        }
    }
}
