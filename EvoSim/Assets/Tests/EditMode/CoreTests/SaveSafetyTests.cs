using System;
using System.Reflection;
using Evo.Core;
using NUnit.Framework;

namespace Evo.Tests
{
    public class SaveSafetyTests
    {
        [Test]
        public void EveryScalarParameterRejectsNonFiniteOrOversizedValues()
        {
            var parameters = new SimParameters();
            foreach (FieldInfo field in typeof(SimParameters).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object original = field.GetValue(parameters);
                field.SetValue(parameters, field.FieldType == typeof(float) ? (object)float.NaN : (object)int.MaxValue);
                Assert.IsFalse(parameters.Validate(), field.Name);
                field.SetValue(parameters, original);
            }
        }

        [Test]
        public void SupportedCapacitySnapshotLoadsWithoutRunningALongSimulation()
        {
            Simulation sim = Simulation.Create(new SetupSettings { ToxinLevel = 0 },
                new SimParameters { FounderCount = 600, InitialFood = 1500, MaxActiveEvents = 16 });
            sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill, WholeWorld = true, Radius = 60 });
            for (int i = 0; i < 16; i++) sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave });
            Sample sample = sim.Stats.Samples[0];
            for (int i = 1; i < sim.P.MaxSamples; i++) sim.Stats.Samples.Add(sample);
            Simulation loaded = Simulation.FromSnapshot(sim.ToSnapshot());
            Assert.AreEqual(600, loaded.Cells.AliveCount);
            Assert.AreEqual(1500, loaded.FoodCount);
            Assert.AreEqual(1500, loaded.ToxCount);
            Assert.AreEqual(16, loaded.Events.ActiveCount);
            Assert.AreEqual(3600, loaded.Stats.Samples.Count);
            Assert.AreEqual(sim.StateHash(), loaded.StateHash());
        }

        [Test]
        public void SmallPopulationCapacityStillSupportsTheDefaultSpeciesThreshold()
        {
            Simulation sim = TestHelpers.Quiet(1, p => p.MaxCells = 1);
            Assert.AreEqual(sim.StateHash(), Simulation.FromSnapshot(sim.ToSnapshot()).StateHash());
        }

        [TestCase("interval")]
        [TestCase("trait-mean")]
        [TestCase("trait-sd")]
        [TestCase("bins")]
        [TestCase("sample-deaths")]
        [TestCase("sample-null")]
        [TestCase("species-pops")]
        [TestCase("sample-species-id")]
        [TestCase("roll-index")]
        [TestCase("roll-array")]
        [TestCase("death-bins")]
        [TestCase("death-bin-count")]
        [TestCase("negative-counter")]
        [TestCase("counter-overflow")]
        [TestCase("pool")]
        [TestCase("world-size")]
        [TestCase("dt")]
        [TestCase("parameter-nan")]
        [TestCase("divisor")]
        [TestCase("setup")]
        [TestCase("rng-zero")]
        [TestCase("rng-spare")]
        [TestCase("food-length")]
        [TestCase("toxin-length")]
        [TestCase("cell-null")]
        [TestCase("cell-traits")]
        [TestCase("cell-weights")]
        [TestCase("cell-value")]
        [TestCase("cell-id")]
        [TestCase("cell-species")]
        [TestCase("next-species-id")]
        [TestCase("species-parent")]
        [TestCase("species-genome")]
        [TestCase("pending")]
        [TestCase("event-enum")]
        [TestCase("event-spec")]
        [TestCase("event-capacity")]
        [TestCase("step")]
        [TestCase("extinct-with-cells")]
        [TestCase("version")]
        public void InvalidSnapshotsAreRejectedBeforeConstruction(string field)
        {
            Simulation sim = TestHelpers.Quiet(2);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave });
            SimSnapshot s = sim.ToSnapshot();
            switch (field)
            {
                case "interval": s.Stats.IntervalSteps = 0; break;
                case "trait-mean": s.Stats.Samples[0].TraitMean = new float[0]; break;
                case "trait-sd": s.Stats.Samples[0].TraitSd[0] = float.NaN; break;
                case "bins": s.Stats.Samples[0].Bins[0] = 999; break;
                case "sample-deaths": s.Stats.Samples[0].Deaths = null; break;
                case "sample-null": s.Stats.Samples[0] = null; break;
                case "species-pops": s.Stats.Samples[0].SpeciesPops = new int[0]; break;
                case "sample-species-id": s.Stats.Samples[0].SpeciesIds[0] = 999; break;
                case "roll-index": s.Stats.RollIndex = 60; break;
                case "roll-array": s.Stats.BirthsPerSecond = new int[0]; break;
                case "death-bins": s.Stats.DeathBins = new int[6]; break;
                case "death-bin-count": s.Stats.DeathBinCount = int.MaxValue; break;
                case "negative-counter": s.Stats.TotalBirths = -1; break;
                case "counter-overflow": s.Stats.TotalDeaths[0] = s.Stats.TotalDeaths[1] = int.MaxValue; break;
                case "pool": s.Params.PoolCapacity = int.MaxValue; break;
                case "world-size": s.Params.WorldWidth = float.MaxValue; break;
                case "dt": s.Params.Dt = 0; break;
                case "parameter-nan": s.Params.ToxinDamping = float.NaN; break;
                case "divisor": s.Params.DistanceBrainDivisor = 0; break;
                case "setup": s.Setup.ZoneScale = float.PositiveInfinity; break;
                case "rng-zero": s.SimRng = new RngState(); break;
                case "rng-spare": s.WorldRng.Spare = float.NaN; break;
                case "food-length": s.FoodY = new float[1]; break;
                case "toxin-length": s.ToxHalf = new float[1]; break;
                case "cell-null": s.Cells[0] = null; break;
                case "cell-traits": s.Cells[0].Trait[0] = 0; break;
                case "cell-weights": s.Cells[0].Weights = new float[1]; break;
                case "cell-value": s.Cells[0].X = float.NaN; break;
                case "cell-id": s.Cells[1].Id = s.Cells[0].Id; break;
                case "cell-species": s.Cells[0].SpeciesId = 999; break;
                case "next-species-id": s.NextSpeciesId = int.MaxValue; break;
                case "species-parent": s.Species[0].ParentId = 1; break;
                case "species-genome": s.Species[0].Trait = null; break;
                case "pending": s.Pending = new[] { new CandidateSnap { Size = int.MaxValue } }; break;
                case "event-enum": s.EventLog[0].Kind = (EventKind)999; break;
                case "event-spec": s.Events[0].Spec = null; break;
                case "event-capacity": s.Events = new ActiveEventSnap[17]; break;
                case "step": s.StepCount = long.MaxValue; break;
                case "extinct-with-cells": s.Extinct = true; break;
                case "version": s.FormatVersion = 99; break;
            }
            Assert.Throws<FormatException>(() => Simulation.FromSnapshot(s), field);
        }

        [Test]
        public void EveryParameterAndSetupFieldParticipatesInTheHash()
        {
            Simulation sim = TestHelpers.Quiet(1);
            foreach (object settings in new object[] { sim.P, sim.Setup })
            {
                foreach (FieldInfo field in settings.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    object before = field.GetValue(settings);
                    ulong hash = sim.StateHash();
                    object changed = field.FieldType == typeof(float) ? (object)((float)before + 0.125f) :
                        field.FieldType == typeof(int) ? (object)((int)before + 1) : (object)((string)before + "x");
                    field.SetValue(settings, changed);
                    Assert.AreNotEqual(hash, sim.StateHash(), field.Name);
                    field.SetValue(settings, before);
                }
            }
        }

        [Test]
        public void HashIncludesBothRngStreamsAndPreviouslyOmittedSavedState()
        {
            Simulation sim = TestHelpers.Quiet(1);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave });
            var candidate = new Candidate { Size = 1 };
            candidate.Centroid.CopyFrom(sim.Cells.Alive[0].G);
            sim.Species.Pending.Add(candidate);
            Action[] changes = {
                () => sim.SimRng.NextUInt(), () => sim.WorldRng.NextUInt(),
                () => sim.Cells.Alive[0].OutTurn += 0.1f, () => sim.Cells.Alive[0].OutThrust += 0.1f,
                () => sim.Cells.Alive[0].OutBite += 0.1f, () => sim.Cells.Alive[0].OutMate += 0.1f,
                () => sim.Events.Active[0].PlacedTime += 1, () => sim.Events.Log[0].Radius += 1,
                () => sim.Species.Records[0].Name += "x", () => sim.Species.Records[0].BirthGeneration++,
                () => candidate.Centroid.Weights[0] += 0.1f, () => sim.Stats.Samples[0].TraitMean[0] += 0.1f,
                () => sim.Stats.Samples[0].Bins[0]++, () => sim.Stats.DeathBins[0]++,
                () => sim.Stats.BirthsPerSecond[0]++, () => sim.Stats.DeathsPerSecond[0]++,
                () => sim.Stats.RollIndex++, () => sim.Stats.SecBirths++, () => sim.Stats.SecDeaths++,
                () => sim.Stats.MaxGenerationEver++, () => sim.Stats.PeakCells++
            };
            foreach (Action change in changes)
            {
                ulong hash = sim.StateHash();
                change();
                Assert.AreNotEqual(hash, sim.StateHash(), change.Method.Name);
            }
        }

        [TestCase(20)]
        [TestCase(21)]
        [TestCase(3600)]
        public void ExtinctionAtTheDecimationBoundaryKeepsTheTerminalSample(int max)
        {
            Simulation sim = TestHelpers.Quiet(1, p => p.MaxSamples = max);
            for (int i = 1; i < max - 1; i++)
                sim.Stats.Sample(sim.Cells, sim.Species, i);
            sim.StepCount = (long)((max - 2) / sim.P.Dt);
            sim.Cells.Alive[0].Energy = 0;
            sim.Step();
            Sample last = sim.Stats.Samples[sim.Stats.Samples.Count - 1];
            Assert.IsTrue(sim.Extinct);
            Assert.AreEqual((float)sim.Time, last.Time);
            Assert.AreEqual(0, last.Count);
            Assert.AreEqual(0, last.LivingSpecies);
            Assert.Less(sim.Stats.Samples.Count, max);
            Simulation loaded = Simulation.FromSnapshot(sim.ToSnapshot());
            Assert.AreEqual(sim.StateHash(), loaded.StateHash());
        }
    }
}
