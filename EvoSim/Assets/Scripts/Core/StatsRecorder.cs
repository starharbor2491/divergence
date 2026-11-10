using System;
using System.Collections.Generic;

namespace Evo.Core
{
    public static class TraitBins
    {
        public const int Count = 6;

        /// <summary>Lower edge of the first closed bin and upper edge of the last, as Trait values.</summary>
        public static float WindowLow(int trait, SimParameters p)
        {
            return Traits.FounderMean[trait] - p.TraitBinHalfWindow * Traits.Range(trait);
        }

        public static float WindowHigh(int trait, SimParameters p)
        {
            return Traits.FounderMean[trait] + p.TraitBinHalfWindow * Traits.Range(trait);
        }

        public static int BinOf(int trait, float v, SimParameters p)
        {
            float lo = WindowLow(trait, p), hi = WindowHigh(trait, p);
            int b = (int)Math.Floor((v - lo) / ((hi - lo) / Count));
            return b < 0 ? 0 : (b >= Count ? Count - 1 : b);
        }

        public static float EdgeLow(int trait, int bin, SimParameters p)
        {
            float lo = WindowLow(trait, p), hi = WindowHigh(trait, p);
            return lo + (hi - lo) * bin / Count;
        }
    }

    /// <summary>One statistics sample (PLAN 9.4).</summary>
    [Serializable]
    public class Sample
    {
        public float Time;
        public int Count;
        public float MeanGeneration;
        public int MaxGeneration;
        public int Births;
        public int[] Deaths = new int[DeathCauses.Count];
        public int LivingSpecies;
        public int ExtinctSpecies;
        public float SexualFraction;
        public float[] TraitMean = new float[Traits.ChartCount];
        public float[] TraitSd = new float[Traits.ChartCount];
        public int[] Bins = new int[Traits.ChartCount * TraitBins.Count];
        public int[] SpeciesIds = new int[0];
        public int[] SpeciesPops = new int[0];
    }

    public sealed class StatsRecorder
    {
        readonly SimParameters p;
        public readonly List<Sample> Samples = new List<Sample>();
        public int IntervalSteps;
        public int TotalBirths;
        public readonly int[] TotalDeaths = new int[DeathCauses.Count];
        public int MaxGenerationEver;
        public int PeakCells;

        // deaths per 10 s bin: DeathBins[bin * 5 + cause]
        public int[] DeathBins = new int[5 * 64];
        public int DeathBinCount;

        // short rolling window for births and deaths per minute (per simulated second counters)
        public readonly int[] BirthsPerSecond = new int[60];
        public readonly int[] DeathsPerSecond = new int[60];
        public int RollIndex;
        public int SecBirths, SecDeaths;

        public StatsRecorder(SimParameters p)
        {
            this.p = p;
            Reset();
        }

        public void Reset()
        {
            Samples.Clear();
            IntervalSteps = Math.Max(1, (int)Math.Round(p.SampleSeconds / p.Dt));
            TotalBirths = 0;
            Array.Clear(TotalDeaths, 0, TotalDeaths.Length);
            Array.Clear(DeathBins, 0, DeathBins.Length);
            DeathBinCount = 0;
            MaxGenerationEver = 0;
            PeakCells = 0;
            Array.Clear(BirthsPerSecond, 0, 60);
            Array.Clear(DeathsPerSecond, 0, 60);
            RollIndex = 0;
            SecBirths = SecDeaths = 0;
        }

        public void RecordBirth(int generation)
        {
            TotalBirths++;
            SecBirths++;
            if (generation > MaxGenerationEver) MaxGenerationEver = generation;
        }

        public void RecordDeath(DeathCause cause, double time)
        {
            TotalDeaths[(int)cause]++;
            SecDeaths++;
            int bin = (int)(time / p.DeathBinSeconds);
            if ((bin + 1) * 5 > DeathBins.Length) Array.Resize(ref DeathBins, Math.Max(DeathBins.Length * 2, (bin + 1) * 5));
            if (bin + 1 > DeathBinCount) DeathBinCount = bin + 1;
            DeathBins[bin * 5 + (int)cause]++;
        }

        public int TotalDeathCount
        {
            get { int s = 0; for (int i = 0; i < TotalDeaths.Length; i++) s += TotalDeaths[i]; return s; }
        }

        public float BirthsPerMinute
        {
            get { int s = 0; for (int i = 0; i < 60; i++) s += BirthsPerSecond[i]; return s; }
        }

        public float DeathsPerMinute
        {
            get { int s = 0; for (int i = 0; i < 60; i++) s += DeathsPerSecond[i]; return s; }
        }

        /// <summary>Called once per simulated second to advance the births and deaths per minute window.</summary>
        public void RollSecond()
        {
            RollIndex = (RollIndex + 1) % 60;
            BirthsPerSecond[RollIndex] = SecBirths;
            DeathsPerSecond[RollIndex] = SecDeaths;
            SecBirths = SecDeaths = 0;
        }

        public void Sample(CellPool cells, SpeciesTracker species, double time)
        {
            var s = new Sample();
            s.Time = (float)time;
            int n = cells.AliveCount;
            s.Count = n;
            if (n > PeakCells) PeakCells = n;
            s.Births = TotalBirths;
            Array.Copy(TotalDeaths, s.Deaths, DeathCauses.Count);
            double gsum = 0;
            int gmax = 0, sex = 0;
            var sum = new double[Traits.ChartCount];
            var sq = new double[Traits.ChartCount];
            int maxId = 0;
            for (int i = 0; i < species.Records.Count; i++) if (species.Records[i].Id > maxId) maxId = species.Records[i].Id;
            var pop = new int[maxId + 1];
            for (int i = 0; i < n; i++)
            {
                Cell c = cells.Alive[i];
                gsum += c.Generation;
                if (c.Generation > gmax) gmax = c.Generation;
                for (int t = 0; t < Traits.ChartCount; t++)
                {
                    double v = c.G.Trait[t];
                    sum[t] += v;
                    sq[t] += v * v;
                    s.Bins[t * TraitBins.Count + TraitBins.BinOf(t, (float)v, p)]++;
                }
                if (c.G.Trait[TraitId.SexualTendency] >= p.SexThreshold) sex++;
                if (c.SpeciesId > 0 && c.SpeciesId <= maxId) pop[c.SpeciesId]++;
            }
            if (n > 0)
            {
                s.MeanGeneration = (float)(gsum / n);
                s.MaxGeneration = gmax;
                s.SexualFraction = (float)sex / n;
                for (int t = 0; t < Traits.ChartCount; t++)
                {
                    double m = sum[t] / n;
                    s.TraitMean[t] = (float)m;
                    double variance = sq[t] / n - m * m;
                    s.TraitSd[t] = variance > 0 ? (float)Math.Sqrt(variance) : 0f;
                }
            }
            int k = 0;
            for (int id = 1; id <= maxId; id++) if (pop[id] > 0) k++;
            s.SpeciesIds = new int[k];
            s.SpeciesPops = new int[k];
            k = 0;
            for (int id = 1; id <= maxId; id++)
            {
                if (pop[id] > 0) { s.SpeciesIds[k] = id; s.SpeciesPops[k] = pop[id]; k++; }
            }
            s.LivingSpecies = species.LivingCount;
            s.ExtinctSpecies = species.ExtinctCount;
            Samples.Add(s);

            if (Samples.Count >= p.MaxSamples)
            {
                // Keep every second sample, plus the final empty-population sample.
                int w = 0;
                int last = Samples.Count - 1;
                for (int i = 0; i < Samples.Count; i += 2) Samples[w++] = Samples[i];
                if (n == 0 && last % 2 != 0) Samples[w++] = s;
                Samples.RemoveRange(w, Samples.Count - w);
                IntervalSteps = IntervalSteps <= int.MaxValue / 2 ? IntervalSteps * 2 : int.MaxValue;
            }
        }
    }
}
