using System;
using System.Collections.Generic;

namespace Evo.Core
{
    [Serializable]
    public class CellSnap
    {
        public int Id, ParentId, Generation, SpeciesId;
        public float X, Y, Heading, Energy, Age;
        public float DrainBase, DrainTemperature, DrainToxin, DrainAge, BiteTimer;
        public int BiteTargetId;
        public long MatedStep;
        public float OutTurn, OutThrust, OutBite, OutMate;
        public float[] Trait;
        public float[] Weights;
    }

    [Serializable]
    public class SpeciesSnap
    {
        public int Id, ParentId, BirthGeneration, PeakPopulation, CurrentPopulation, LastPopulation, EmptyChecks;
        public string Name;
        public double BirthTime, ExtinctTime;
        public float[] Trait;
        public float[] Weights;
    }

    [Serializable]
    public class CandidateSnap
    {
        public int Size;
        public float[] Trait;
        public float[] Weights;
    }

    [Serializable]
    public class ActiveEventSnap
    {
        public EventSpec Spec;
        public float Age, Accumulator;
        public double PlacedTime;
    }

    [Serializable]
    public class StatsSnap
    {
        public Sample[] Samples;
        public int IntervalSteps, TotalBirths, MaxGenerationEver, PeakCells, DeathBinCount, RollIndex, SecBirths, SecDeaths;
        public int[] TotalDeaths, DeathBins, BirthsPerSecond, DeathsPerSecond;
    }

    /// <summary>A complete saved Run (PLAN 15.1). Plain public fields and arrays only, so JsonUtility can write it.</summary>
    [Serializable]
    public class SimSnapshot
    {
        public const int CurrentFormatVersion = 1;

        public int FormatVersion = CurrentFormatVersion;
        public SetupSettings Setup;
        public SimParameters Params;
        public long StepCount;
        public int NextCellId;
        public bool Extinct;
        public float FoodAcc;
        public RngState WorldRng, SimRng;
        public CellSnap[] Cells;
        public float[] FoodX, FoodY;
        public float[] ToxX, ToxY, ToxVX, ToxVY, ToxS, ToxHalf;
        public ActiveEventSnap[] Events;
        public EventRecord[] EventLog;
        public SpeciesSnap[] Species;
        public CandidateSnap[] Pending;
        public int NextSpeciesId;
        public StatsSnap Stats;
    }

    public sealed partial class Simulation
    {
        static float[] Trim(float[] a, int n)
        {
            var r = new float[n];
            Array.Copy(a, r, n);
            return r;
        }

        public SimSnapshot ToSnapshot()
        {
            var s = new SimSnapshot
            {
                Setup = Setup.Clone(),
                Params = P,
                StepCount = StepCount,
                NextCellId = NextCellId,
                Extinct = Extinct,
                FoodAcc = foodAcc,
                WorldRng = WorldRng.GetState(),
                SimRng = SimRng.GetState(),
                FoodX = Trim(FoodX, FoodCount),
                FoodY = Trim(FoodY, FoodCount),
                ToxX = Trim(ToxX, ToxCount),
                ToxY = Trim(ToxY, ToxCount),
                ToxVX = Trim(ToxVX, ToxCount),
                ToxVY = Trim(ToxVY, ToxCount),
                ToxS = Trim(ToxS, ToxCount),
                ToxHalf = Trim(ToxHalf, ToxCount),
                NextSpeciesId = Species.NextId
            };

            s.Cells = new CellSnap[Cells.AliveCount];
            for (int i = 0; i < Cells.AliveCount; i++)
            {
                Cell c = Cells.Alive[i];
                s.Cells[i] = new CellSnap
                {
                    Id = c.Id, ParentId = c.ParentId, Generation = c.Generation, SpeciesId = c.SpeciesId,
                    X = c.X, Y = c.Y, Heading = c.Heading, Energy = c.Energy, Age = c.Age,
                    DrainBase = c.DrainBase, DrainTemperature = c.DrainTemperature, DrainToxin = c.DrainToxin,
                    DrainAge = c.DrainAge, BiteTimer = c.BiteTimer, BiteTargetId = c.BiteTargetId, MatedStep = c.MatedStep,
                    OutTurn = c.OutTurn, OutThrust = c.OutThrust, OutBite = c.OutBite, OutMate = c.OutMate,
                    Trait = (float[])c.G.Trait.Clone(),
                    Weights = (float[])c.G.Weights.Clone()
                };
            }

            s.Events = new ActiveEventSnap[Events.ActiveCount];
            for (int i = 0; i < Events.ActiveCount; i++)
            {
                ActiveEvent e = Events.Active[i];
                s.Events[i] = new ActiveEventSnap
                {
                    Spec = new EventSpec
                    {
                        Kind = e.Spec.Kind, X = e.Spec.X, Y = e.Spec.Y, Radius = e.Spec.Radius,
                        Intensity = e.Spec.Intensity, Duration = e.Spec.Duration, WholeWorld = e.Spec.WholeWorld
                    },
                    Age = e.Age, Accumulator = e.Accumulator, PlacedTime = e.PlacedTime
                };
            }
            s.EventLog = Events.Log.ToArray();

            s.Species = new SpeciesSnap[Species.Records.Count];
            for (int i = 0; i < Species.Records.Count; i++)
            {
                SpeciesRecord r = Species.Records[i];
                s.Species[i] = new SpeciesSnap
                {
                    Id = r.Id, ParentId = r.ParentId, Name = r.Name, BirthTime = r.BirthTime, BirthGeneration = r.BirthGeneration,
                    ExtinctTime = r.ExtinctTime, PeakPopulation = r.PeakPopulation, CurrentPopulation = r.CurrentPopulation,
                    LastPopulation = r.LastPopulation, EmptyChecks = r.EmptyChecks,
                    Trait = (float[])r.Centroid.Trait.Clone(), Weights = (float[])r.Centroid.Weights.Clone()
                };
            }
            s.Pending = new CandidateSnap[Species.Pending.Count];
            for (int i = 0; i < Species.Pending.Count; i++)
            {
                Candidate c = Species.Pending[i];
                s.Pending[i] = new CandidateSnap
                {
                    Size = c.Size, Trait = (float[])c.Centroid.Trait.Clone(), Weights = (float[])c.Centroid.Weights.Clone()
                };
            }

            s.Stats = new StatsSnap
            {
                Samples = Stats.Samples.ToArray(),
                IntervalSteps = Stats.IntervalSteps,
                TotalBirths = Stats.TotalBirths,
                MaxGenerationEver = Stats.MaxGenerationEver,
                PeakCells = Stats.PeakCells,
                DeathBinCount = Stats.DeathBinCount,
                RollIndex = Stats.RollIndex,
                SecBirths = Stats.SecBirths,
                SecDeaths = Stats.SecDeaths,
                TotalDeaths = (int[])Stats.TotalDeaths.Clone(),
                DeathBins = (int[])Stats.DeathBins.Clone(),
                BirthsPerSecond = (int[])Stats.BirthsPerSecond.Clone(),
                DeathsPerSecond = (int[])Stats.DeathsPerSecond.Clone()
            };
            return s;
        }

        /// <summary>Validates saved state before allocating a World, then rebuilds the Simulation.</summary>
        public static Simulation FromSnapshot(SimSnapshot s)
        {
            SnapshotValidation.Check(s);
            var sim = new Simulation(s.Setup, s.Params);
            sim.WorldRng.SetState(s.WorldRng);
            sim.SimRng.SetState(s.SimRng);
            sim.StepCount = s.StepCount;
            sim.NextCellId = s.NextCellId;
            sim.Extinct = s.Extinct;
            sim.foodAcc = s.FoodAcc;

            sim.FoodCount = s.FoodX.Length;
            Array.Copy(s.FoodX, sim.FoodX, sim.FoodCount);
            Array.Copy(s.FoodY, sim.FoodY, sim.FoodCount);
            sim.ToxCount = s.ToxX.Length;
            Array.Copy(s.ToxX, sim.ToxX, sim.ToxCount);
            Array.Copy(s.ToxY, sim.ToxY, sim.ToxCount);
            Array.Copy(s.ToxVX, sim.ToxVX, sim.ToxCount);
            Array.Copy(s.ToxVY, sim.ToxVY, sim.ToxCount);
            Array.Copy(s.ToxS, sim.ToxS, sim.ToxCount);
            Array.Copy(s.ToxHalf, sim.ToxHalf, sim.ToxCount);

            for (int i = 0; i < s.Cells.Length; i++)
            {
                CellSnap cs = s.Cells[i];
                Cell c = sim.Cells.Acquire();
                c.ResetState();
                c.Id = cs.Id; c.ParentId = cs.ParentId; c.Generation = cs.Generation; c.SpeciesId = cs.SpeciesId;
                c.X = cs.X; c.Y = cs.Y; c.Heading = cs.Heading; c.Energy = cs.Energy; c.Age = cs.Age;
                c.DrainBase = cs.DrainBase; c.DrainTemperature = cs.DrainTemperature; c.DrainToxin = cs.DrainToxin;
                c.DrainAge = cs.DrainAge; c.BiteTimer = cs.BiteTimer; c.BiteTargetId = cs.BiteTargetId; c.MatedStep = cs.MatedStep;
                c.OutTurn = cs.OutTurn; c.OutThrust = cs.OutThrust; c.OutBite = cs.OutBite; c.OutMate = cs.OutMate;
                Array.Copy(cs.Trait, c.G.Trait, Traits.Count);
                Array.Copy(cs.Weights, c.G.Weights, BrainLayout.WeightCount);
                c.Refresh(sim.P);
                sim.UpdateColor(c);
                sim.Cells.AddAlive(c);
            }

            for (int i = 0; i < s.Events.Length; i++)
            {
                ActiveEventSnap e = s.Events[i];
                sim.Events.Add(new ActiveEvent { Spec = e.Spec, Age = e.Age, Accumulator = e.Accumulator, PlacedTime = e.PlacedTime });
            }
            sim.Events.Log.AddRange(s.EventLog);

            sim.Species.Records.Clear();
            for (int i = 0; i < s.Species.Length; i++)
            {
                SpeciesSnap r = s.Species[i];
                var rec = new SpeciesRecord
                {
                    Id = r.Id, ParentId = r.ParentId, Name = r.Name, BirthTime = r.BirthTime, BirthGeneration = r.BirthGeneration,
                    ExtinctTime = r.ExtinctTime, PeakPopulation = r.PeakPopulation, CurrentPopulation = r.CurrentPopulation,
                    LastPopulation = r.LastPopulation > 0 ? r.LastPopulation : r.CurrentPopulation,
                    EmptyChecks = r.EmptyChecks
                };
                Array.Copy(r.Trait, rec.Centroid.Trait, Traits.Count);
                Array.Copy(r.Weights, rec.Centroid.Weights, BrainLayout.WeightCount);
                CellColor.Compute(rec.Centroid, sim.BrainMean, out rec.ColorR, out rec.ColorG, out rec.ColorB);
                sim.Species.Records.Add(rec);
            }
            sim.Species.Pending.Clear();
            for (int i = 0; i < s.Pending.Length; i++)
            {
                CandidateSnap c = s.Pending[i];
                var cand = new Candidate { Size = c.Size };
                Array.Copy(c.Trait, cand.Centroid.Trait, Traits.Count);
                Array.Copy(c.Weights, cand.Centroid.Weights, BrainLayout.WeightCount);
                sim.Species.Pending.Add(cand);
            }
            sim.Species.NextId = s.NextSpeciesId;

            StatsSnap st = s.Stats;
            sim.Stats.Samples.Clear();
            sim.Stats.Samples.AddRange(st.Samples);
            sim.Stats.IntervalSteps = st.IntervalSteps;
            sim.Stats.TotalBirths = st.TotalBirths;
            sim.Stats.MaxGenerationEver = st.MaxGenerationEver;
            sim.Stats.PeakCells = st.PeakCells;
            sim.Stats.DeathBinCount = st.DeathBinCount;
            sim.Stats.RollIndex = st.RollIndex;
            sim.Stats.SecBirths = st.SecBirths;
            sim.Stats.SecDeaths = st.SecDeaths;
            Array.Copy(st.TotalDeaths, sim.Stats.TotalDeaths, DeathCauses.Count);
            sim.Stats.DeathBins = (int[])st.DeathBins.Clone();
            Array.Copy(st.BirthsPerSecond, sim.Stats.BirthsPerSecond, 60);
            Array.Copy(st.DeathsPerSecond, sim.Stats.DeathsPerSecond, 60);
            // v1 saves did not retain the last nonzero population. Recover the
            // last recorded count where possible, never substitute a peak.
            foreach (SpeciesRecord record in sim.Species.Records)
            {
                if (record.LastPopulation > 0) continue;
                for (int i = sim.Stats.Samples.Count - 1; i >= 0 && record.LastPopulation == 0; i--)
                {
                    Sample sample = sim.Stats.Samples[i];
                    for (int k = 0; k < sample.SpeciesIds.Length; k++)
                        if (sample.SpeciesIds[k] == record.Id && sample.SpeciesPops[k] > 0) record.LastPopulation = sample.SpeciesPops[k];
                }
            }
            if (sim.Extinct && sim.Cells.AliveCount == 0 && sim.Species.LivingCount > 0)
            {
                sim.Species.Finish(sim.Time);
                int last = sim.Stats.Samples.Count - 1;
                if (last >= 0 && sim.Stats.Samples[last].Time == (float)sim.Time) sim.Stats.Samples.RemoveAt(last);
                sim.Stats.Sample(sim.Cells, sim.Species, sim.Time);
            }
            return sim;
        }

        // ---------------------------------------------------------- State hash

        /// <summary>64-bit FNV-1a over everything that affects the future of the Run. Used to prove save and load are exact.</summary>
        public ulong StateHash()
        {
            var h = new Hasher();
            h.Add(Setup.SeedText); h.Add(Setup.FoodAbundance); h.Add(Setup.TemperatureRange);
            h.Add(Setup.ZoneScale); h.Add(Setup.ToxinLevel); h.Add(Setup.MutationRate);
            h.Add(P.WorldWidth); h.Add(P.WorldHeight); h.Add(P.Dt); h.Add(P.FounderCount); h.Add(P.MaxCells);
            h.Add(P.PoolCapacity); h.Add(P.FounderStartEnergy);
            h.Add(P.FoodEnergy); h.Add(P.FoodRadius); h.Add(P.FoodRate); h.Add(P.MaxFood); h.Add(P.InitialFood);
            h.Add(P.FoodNoiseScale); h.Add(P.FoodSpawnTries);
            h.Add(P.ToxinHalfLifeCloud); h.Add(P.ToxinHalfLifeSpill); h.Add(P.MaxToxin); h.Add(P.ToxinDamping);
            h.Add(P.ToxinDrift); h.Add(P.ToxinRc); h.Add(P.ToxinG); h.Add(P.ToxinAccelCap);
            h.Add(P.ToxinMinStrength); h.Add(P.ToxinSenseStrength);
            h.Add(P.BaseSize); h.Add(P.BaseSense); h.Add(P.BaseResist); h.Add(P.BaseTolerance); h.Add(P.MoveCost);
            h.Add(P.AgeStart); h.Add(P.AgeCost); h.Add(P.TempStressCost); h.Add(P.ToxinCost); h.Add(P.ToxinExposure);
            h.Add(P.TurnRate); h.Add(P.DrainTimeConstant); h.Add(P.MaxEnergyPerSize);
            h.Add(P.DietFoodPenalty); h.Add(P.PredationRatio); h.Add(P.PredationMinDiet); h.Add(P.BiteOutputThreshold);
            h.Add(P.BiteTime); h.Add(P.BiteReach); h.Add(P.PredatorEnergyShare); h.Add(P.PredatorMaxShare);
            h.Add(P.ParentKeep); h.Add(P.ChildShare); h.Add(P.TraitMutationChance); h.Add(P.TraitMutationSd);
            h.Add(P.WeightMutationChance); h.Add(P.WeightMutationSd); h.Add(P.BigStepChance); h.Add(P.BigStepFactor);
            h.Add(P.WeightLimit); h.Add(P.FounderTraitSpread); h.Add(P.FounderWeightSd); h.Add(P.InnateWiring);
            h.Add(P.SexThreshold); h.Add(P.MateEnergyFraction); h.Add(P.MateCost); h.Add(P.MateChildShare);
            h.Add(P.MateDistMaxFactor); h.Add(P.MateDistMinFactor);
            h.Add(P.SpeciesTheta); h.Add(P.SpeciesCheckSeconds); h.Add(P.SpeciesMinGroup);
            h.Add(P.DistanceTraitWeight); h.Add(P.DistanceBrainWeight); h.Add(P.DistanceBrainDivisor); h.Add(P.CentroidKeep);
            h.Add(P.SampleSeconds); h.Add(P.MaxSamples); h.Add(P.DeathBinSeconds); h.Add(P.TraitBinHalfWindow);
            h.Add(P.MaxActiveEvents); h.Add(P.HeatRampUp); h.Add(P.HeatRampDown);
            h.Add(P.FamineRemoveShare); h.Add(P.FamineRejectShare); h.Add(P.BloomRate); h.Add(P.SpillParticlesPer10Radius);
            h.Add(StepCount);
            h.Add(NextCellId);
            h.Add(Extinct ? 1 : 0);
            h.Add(foodAcc);
            AddRng(ref h, WorldRng.GetState());
            AddRng(ref h, SimRng.GetState());

            h.Add(FoodCount);
            for (int i = 0; i < FoodCount; i++) { h.Add(FoodX[i]); h.Add(FoodY[i]); }
            h.Add(ToxCount);
            for (int i = 0; i < ToxCount; i++)
            {
                h.Add(ToxX[i]); h.Add(ToxY[i]); h.Add(ToxVX[i]); h.Add(ToxVY[i]); h.Add(ToxS[i]); h.Add(ToxHalf[i]);
            }
            h.Add(Cells.AliveCount);
            for (int i = 0; i < Cells.AliveCount; i++)
            {
                Cell c = Cells.Alive[i];
                h.Add(c.Id); h.Add(c.ParentId); h.Add(c.Generation); h.Add(c.SpeciesId);
                h.Add(c.X); h.Add(c.Y); h.Add(c.Heading); h.Add(c.Energy); h.Add(c.Age);
                h.Add(c.DrainBase); h.Add(c.DrainTemperature); h.Add(c.DrainToxin); h.Add(c.DrainAge);
                h.Add(c.BiteTimer); h.Add(c.BiteTargetId); h.Add(c.MatedStep);
                h.Add(c.OutTurn); h.Add(c.OutThrust); h.Add(c.OutBite); h.Add(c.OutMate);
                for (int t = 0; t < Traits.Count; t++) h.Add(c.G.Trait[t]);
                for (int w = 0; w < BrainLayout.WeightCount; w++) h.Add(c.G.Weights[w]);
            }
            h.Add(Events.ActiveCount);
            for (int i = 0; i < Events.ActiveCount; i++)
            {
                ActiveEvent e = Events.Active[i];
                h.Add((int)e.Spec.Kind); h.Add(e.Spec.X); h.Add(e.Spec.Y); h.Add(e.Spec.Radius);
                h.Add(e.Spec.Intensity); h.Add(e.Spec.Duration); h.Add(e.Spec.WholeWorld ? 1 : 0);
                h.Add(e.Age); h.Add(e.Accumulator); h.Add(e.PlacedTime);
            }
            h.Add(Events.Log.Count);
            foreach (EventRecord e in Events.Log)
            {
                h.Add(e.Time); h.Add((int)e.Kind); h.Add(e.X); h.Add(e.Y); h.Add(e.Radius);
                h.Add(e.Intensity); h.Add(e.Duration); h.Add(e.WholeWorld ? 1 : 0);
            }
            h.Add(Species.NextId);
            h.Add(Species.Records.Count);
            for (int i = 0; i < Species.Records.Count; i++)
            {
                SpeciesRecord r = Species.Records[i];
                h.Add(r.Id); h.Add(r.ParentId); h.Add(r.BirthTime); h.Add(r.ExtinctTime);
                h.Add(r.Name); h.Add(r.BirthGeneration);
                h.Add(r.PeakPopulation); h.Add(r.CurrentPopulation); h.Add(r.LastPopulation); h.Add(r.EmptyChecks);
                for (int t = 0; t < Traits.Count; t++) h.Add(r.Centroid.Trait[t]);
                for (int w = 0; w < BrainLayout.WeightCount; w++) h.Add(r.Centroid.Weights[w]);
            }
            h.Add(Species.Pending.Count);
            for (int i = 0; i < Species.Pending.Count; i++)
            {
                h.Add(Species.Pending[i].Size);
                for (int t = 0; t < Traits.Count; t++) h.Add(Species.Pending[i].Centroid.Trait[t]);
                for (int w = 0; w < BrainLayout.WeightCount; w++) h.Add(Species.Pending[i].Centroid.Weights[w]);
            }
            h.Add(Stats.Samples.Count);
            foreach (Sample s in Stats.Samples)
            {
                h.Add(s.Time); h.Add(s.Count); h.Add(s.MeanGeneration); h.Add(s.MaxGeneration); h.Add(s.Births);
                h.Add(s.Deaths); h.Add(s.LivingSpecies); h.Add(s.ExtinctSpecies); h.Add(s.SexualFraction);
                h.Add(s.TraitMean); h.Add(s.TraitSd); h.Add(s.Bins); h.Add(s.SpeciesIds); h.Add(s.SpeciesPops);
            }
            h.Add(Stats.IntervalSteps);
            h.Add(Stats.TotalBirths);
            h.Add(Stats.TotalDeaths); h.Add(Stats.MaxGenerationEver); h.Add(Stats.PeakCells);
            h.Add(Stats.DeathBinCount); h.Add(Stats.DeathBins); h.Add(Stats.RollIndex);
            h.Add(Stats.SecBirths); h.Add(Stats.SecDeaths); h.Add(Stats.BirthsPerSecond); h.Add(Stats.DeathsPerSecond);
            return h.Value;
        }

        static void AddRng(ref Hasher h, RngState r)
        {
            h.Add(r.S0); h.Add(r.S1); h.Add(r.S2); h.Add(r.S3);
            h.Add(r.HasSpare ? 1 : 0); h.Add(r.Spare);
        }

        struct Hasher
        {
            public ulong Value;
            ulong v;
            bool init;

            void Mix(ulong x)
            {
                if (!init) { v = 14695981039346656037UL; init = true; }
                for (int i = 0; i < 8; i++)
                {
                    v ^= (x >> (i * 8)) & 0xFF;
                    v *= 1099511628211UL;
                }
                Value = v;
            }

            public void Add(int x) { Mix(unchecked((ulong)(long)x)); }
            public void Add(long x) { Mix(unchecked((ulong)x)); }
            public void Add(float x) { Mix(unchecked((ulong)(uint)BitConverter.SingleToInt32Bits(x))); }
            public void Add(double x) { Mix(unchecked((ulong)BitConverter.DoubleToInt64Bits(x))); }
            public void Add(string x) { Add(x == null ? -1 : x.Length); if (x != null) foreach (char c in x) Add((int)c); }
            public void Add(int[] x) { Add(x.Length); foreach (int v in x) Add(v); }
            public void Add(float[] x) { Add(x.Length); foreach (float v in x) Add(v); }
        }
    }
}
