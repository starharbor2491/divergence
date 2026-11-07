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
        public int Id, ParentId, BirthGeneration, PeakPopulation, CurrentPopulation, EmptyChecks;
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
                    EmptyChecks = r.EmptyChecks,
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

        /// <summary>Rebuilds a Simulation from a snapshot. The caller checks FormatVersion first.</summary>
        public static Simulation FromSnapshot(SimSnapshot s)
        {
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
            return sim;
        }

        // ---------------------------------------------------------- State hash

        /// <summary>64-bit FNV-1a over everything that affects the future of the Run. Used to prove save and load are exact.</summary>
        public ulong StateHash()
        {
            var h = new Hasher();
            h.Add(StepCount);
            h.Add(NextCellId);
            h.Add(Extinct ? 1 : 0);
            h.Add(foodAcc);
            AddRng(h, WorldRng.GetState());
            AddRng(h, SimRng.GetState());

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
                for (int t = 0; t < Traits.Count; t++) h.Add(c.G.Trait[t]);
                for (int w = 0; w < BrainLayout.WeightCount; w++) h.Add(c.G.Weights[w]);
            }
            h.Add(Events.ActiveCount);
            for (int i = 0; i < Events.ActiveCount; i++)
            {
                ActiveEvent e = Events.Active[i];
                h.Add((int)e.Spec.Kind); h.Add(e.Spec.X); h.Add(e.Spec.Y); h.Add(e.Spec.Radius);
                h.Add(e.Spec.Intensity); h.Add(e.Spec.Duration); h.Add(e.Spec.WholeWorld ? 1 : 0);
                h.Add(e.Age); h.Add(e.Accumulator);
            }
            h.Add(Events.Log.Count);
            h.Add(Species.NextId);
            h.Add(Species.Records.Count);
            for (int i = 0; i < Species.Records.Count; i++)
            {
                SpeciesRecord r = Species.Records[i];
                h.Add(r.Id); h.Add(r.ParentId); h.Add(r.BirthTime); h.Add(r.ExtinctTime);
                h.Add(r.PeakPopulation); h.Add(r.CurrentPopulation); h.Add(r.EmptyChecks);
                for (int t = 0; t < Traits.Count; t++) h.Add(r.Centroid.Trait[t]);
                for (int w = 0; w < BrainLayout.WeightCount; w++) h.Add(r.Centroid.Weights[w]);
            }
            h.Add(Species.Pending.Count);
            for (int i = 0; i < Species.Pending.Count; i++)
            {
                h.Add(Species.Pending[i].Size);
                for (int t = 0; t < Traits.Count; t++) h.Add(Species.Pending[i].Centroid.Trait[t]);
            }
            h.Add(Stats.Samples.Count);
            h.Add(Stats.IntervalSteps);
            h.Add(Stats.TotalBirths);
            for (int i = 0; i < DeathCauses.Count; i++) h.Add(Stats.TotalDeaths[i]);
            return h.Value;
        }

        static void AddRng(Hasher h, RngState r)
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
        }
    }
}
