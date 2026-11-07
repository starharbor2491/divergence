using System;
using Evo.Core;
using NUnit.Framework;

namespace Evo.Tests
{
    public class SpeciesTests
    {
        static float Lo(int t, float f) { return Traits.Min[t] + f * (Traits.Max[t] - Traits.Min[t]); }

        /// <summary>All Cells share one Genome (cluster A); the tracker is rebuilt around it.</summary>
        static Simulation ClusterWorld(int n)
        {
            Simulation sim = TestHelpers.Quiet(n, p => p.MaxCells = 600);
            Cell first = sim.Cells.Alive[0];
            for (int t = 0; t < Traits.ChartCount; t++) first.G.Trait[t] = Lo(t, 0.2f);
            for (int i = 0; i < n; i++)
            {
                sim.Cells.Alive[i].G.CopyFrom(first.G);
                sim.Cells.Alive[i].SpeciesId = 1;
            }
            sim.Species.Clear();
            sim.Species.InitFirst(sim.Cells);
            return sim;
        }

        static void Shift(Simulation sim, int from, int count, int traitCount)
        {
            for (int i = from; i < from + count; i++)
                for (int t = 0; t < traitCount; t++) sim.Cells.Alive[i].G.Trait[t] = Lo(t, 0.8f);
        }

        static void Check(Simulation sim, int times)
        {
            for (int i = 0; i < times; i++) sim.Species.Check(sim.Cells, 100 + i);
        }

        [Test]
        public void OneClusterIsOneSpecies()
        {
            Simulation sim = ClusterWorld(40);
            Check(sim, 5);
            Assert.AreEqual(1, sim.Species.Records.Count);
            Assert.AreEqual(40, sim.Species.Get(1).CurrentPopulation);
        }

        [Test]
        public void TwoSeparatedClustersGiveTwoSpeciesWithTheRightParent()
        {
            Simulation sim = ClusterWorld(40);
            Shift(sim, 20, 20, 3);
            Check(sim, 1);
            Assert.AreEqual(1, sim.Species.Records.Count, "a group must be seen at two checks in a row");
            Check(sim, 1);
            Assert.AreEqual(2, sim.Species.Records.Count);
            SpeciesRecord child = sim.Species.Records[1];
            Assert.AreEqual(1, child.ParentId);
            Assert.AreEqual(20, child.CurrentPopulation);
            Assert.AreEqual(20, sim.Species.Get(1).CurrentPopulation);
            Assert.AreEqual(child.Id, sim.Cells.Alive[30].SpeciesId);
            Assert.AreEqual(1, sim.Cells.Alive[3].SpeciesId);
        }

        [Test]
        public void ADescendantSplitsFromItsNearestSpecies()
        {
            Simulation sim = ClusterWorld(60);
            Shift(sim, 20, 40, 3);
            Check(sim, 2);
            Shift(sim, 40, 20, 6);
            Check(sim, 2);
            Assert.AreEqual(3, sim.Species.Records.Count);
            Assert.AreEqual(2, sim.Species.Records[2].ParentId, "the new group is nearer to Species 2 than to Species 1");
        }

        [Test]
        public void AGroupSmallerThanTenNeverBecomesASpecies()
        {
            Simulation sim = ClusterWorld(40);
            Shift(sim, 30, 9, 3);
            Check(sim, 8);
            Assert.AreEqual(1, sim.Species.Records.Count);
        }

        [Test]
        public void ExtinctionIsRecordedAfterTwoEmptyChecks()
        {
            Simulation sim = ClusterWorld(40);
            Shift(sim, 20, 20, 3);
            Check(sim, 2);
            Assert.AreEqual(2, sim.Species.Records.Count);
            for (int t = 0; t < Traits.ChartCount; t++)
                for (int i = 20; i < 40; i++) sim.Cells.Alive[i].G.Trait[t] = Lo(t, 0.2f);
            sim.Species.Check(sim.Cells, 200);
            Assert.IsFalse(sim.Species.Records[1].Extinct, "one empty check is not extinction");
            sim.Species.Check(sim.Cells, 202);
            Assert.IsTrue(sim.Species.Records[1].Extinct);
            Assert.AreEqual(202.0, sim.Species.Records[1].ExtinctTime);
            Assert.AreEqual(1, sim.Species.LivingCount);
        }
    }

    public class StatsTests
    {
        [Test]
        public void SamplingIsBoundedAndDeterministic()
        {
            Func<Simulation> make = () =>
            {
                var p = new SimParameters { MaxSamples = 20, FounderCount = 20 };
                return Simulation.Create(new SetupSettings { SeedText = "stats" }, p);
            };
            Simulation a = make(), b = make();
            int initial = a.Stats.IntervalSteps;
            TestHelpers.RunSteps(a, 3000);
            TestHelpers.RunSteps(b, 3000);
            Assert.Less(a.Stats.Samples.Count, 20);
            Assert.Greater(a.Stats.IntervalSteps, initial);
            Assert.AreEqual(a.Stats.Samples.Count, b.Stats.Samples.Count);
            Assert.AreEqual(a.StateHash(), b.StateHash());
            for (int i = 1; i < a.Stats.Samples.Count; i++)
                Assert.Greater(a.Stats.Samples[i].Time, a.Stats.Samples[i - 1].Time);
        }

        [Test]
        public void EverySampleHasConsistentArrays()
        {
            Simulation sim = Simulation.Create(new SetupSettings { SeedText = "stats2" }, new SimParameters { FounderCount = 30 });
            TestHelpers.RunSteps(sim, 1200);
            foreach (Sample s in sim.Stats.Samples)
            {
                Assert.AreEqual(Traits.ChartCount * TraitBins.Count, s.Bins.Length);
                int pop = 0;
                foreach (int v in s.SpeciesPops) pop += v;
                Assert.LessOrEqual(pop, s.Count);
                for (int t = 0; t < Traits.ChartCount; t++)
                {
                    int sum = 0;
                    for (int b = 0; b < TraitBins.Count; b++) sum += s.Bins[t * TraitBins.Count + b];
                    Assert.AreEqual(s.Count, sum);
                }
            }
        }
    }

    public class PersistenceTests
    {
        static Simulation Busy(string seed)
        {
            var sim = Simulation.Create(new SetupSettings { SeedText = seed, ToxinLevel = 0.5f }, new SimParameters { FounderCount = 40 });
            TestHelpers.RunSteps(sim, 300);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, X = 60, Y = 40, Radius = 30, Intensity = 0.3f, Duration = 30 });
            sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill, X = 100, Y = 50, Radius = 20, Intensity = 0.5f, Duration = 20 });
            TestHelpers.RunSteps(sim, 200);
            return sim;
        }

        [Test]
        public void SnapshotRoundTripGivesAnEqualStateHash()
        {
            Simulation a = Busy("persist");
            Simulation b = Simulation.FromSnapshot(a.ToSnapshot());
            Assert.AreEqual(a.StateHash(), b.StateHash());
        }

        [Test]
        public void LoadThenRunEqualsContinuingTheOriginal()
        {
            Simulation a = Busy("persist2");
            Simulation b = Simulation.FromSnapshot(a.ToSnapshot());
            for (int block = 0; block < 6; block++)
            {
                TestHelpers.RunSteps(a, 250);
                TestHelpers.RunSteps(b, 250);
                Assert.AreEqual(a.StateHash(), b.StateHash(), "diverged in block " + block);
            }
            Assert.AreEqual(a.Cells.AliveCount, b.Cells.AliveCount);
            Assert.AreEqual(a.Species.Records.Count, b.Species.Records.Count);
        }

        [Test]
        public void SnapshotDoesNotShareStateWithTheRunningSimulation()
        {
            Simulation a = Busy("persist3");
            SimSnapshot snap = a.ToSnapshot();
            ulong before = Simulation.FromSnapshot(snap).StateHash();
            TestHelpers.RunSteps(a, 300);
            Assert.AreEqual(before, Simulation.FromSnapshot(snap).StateHash());
        }

        [Test]
        public void FormatVersionIsOne()
        {
            Assert.AreEqual(1, new SimSnapshot().FormatVersion);
        }
    }

    public class WorldGenerationTests
    {
        static ulong HashAfter(string seed, int steps, float toxin = 0.3f)
        {
            var sim = Simulation.Create(new SetupSettings { SeedText = seed, ToxinLevel = toxin }, new SimParameters());
            TestHelpers.RunSteps(sim, steps);
            return sim.StateHash();
        }

        [Test]
        public void SameSeedGivesTheSameFoundersFoodToxinAndHash()
        {
            var s = new SetupSettings { SeedText = "same" };
            Simulation a = Simulation.Create(s, new SimParameters());
            Simulation b = Simulation.Create(s, new SimParameters());
            Assert.AreEqual(a.Cells.AliveCount, b.Cells.AliveCount);
            Assert.AreEqual(a.FoodCount, b.FoodCount);
            Assert.AreEqual(a.ToxCount, b.ToxCount);
            for (int i = 0; i < a.Cells.AliveCount; i++)
            {
                Assert.AreEqual(a.Cells.Alive[i].X, b.Cells.Alive[i].X);
                Assert.AreEqual(a.Cells.Alive[i].G.Trait[0], b.Cells.Alive[i].G.Trait[0]);
            }
            Assert.AreEqual(HashAfter("same", 200), HashAfter("same", 200));
        }

        [Test]
        public void DifferentSeedsDiffer()
        {
            Assert.AreNotEqual(HashAfter("one", 200), HashAfter("two", 200));
        }

        [Test]
        public void ToxinLevelZeroMeansNoToxin()
        {
            var sim = Simulation.Create(new SetupSettings { SeedText = "x", ToxinLevel = 0f }, new SimParameters());
            Assert.AreEqual(0, sim.ToxCount);
        }

        [Test]
        public void FoundersArePale()
        {
            var sim = Simulation.Create(new SetupSettings { SeedText = "white" }, new SimParameters());
            Assert.Greater(sim.Cells.AliveCount, 0);
            foreach (Cell c in sim.Cells.Alive)
            {
                if (c == null) break;
                // Founders differ slightly from each other, so they are pale rather than exactly white.
                Assert.GreaterOrEqual(Math.Min(c.ColorR, Math.Min(c.ColorG, c.ColorB)), 0.6f);
            }
        }
    }

    public class ExportAndPresetTests
    {
        [Test]
        public void CsvHeadersMatchThePlan()
        {
            Assert.AreEqual(
                "time_s,mean_generation,max_generation,population,species_count,births_total," +
                "deaths_starved,deaths_eaten,deaths_poisoned,deaths_temperature,deaths_old_age," +
                "size_mean,size_sd,speed_mean,speed_sd,sense_range_mean,sense_range_sd," +
                "preferred_temperature_mean,preferred_temperature_sd,temperature_tolerance_mean,temperature_tolerance_sd," +
                "toxin_resistance_mean,toxin_resistance_sd,diet_mean,diet_sd,reproduction_threshold_mean,reproduction_threshold_sd",
                CsvExport.TraitsHeader());
            Assert.AreEqual("id,parent_id,name,birth_time_s,birth_generation,extinct_time_s,peak_population,current_population",
                CsvExport.SpeciesHeader());
        }

        [Test]
        public void CsvRowsHaveTheSameColumnCountAsTheHeader()
        {
            Simulation sim = Simulation.Create(new SetupSettings { SeedText = "csv" }, new SimParameters { FounderCount = 30 });
            TestHelpers.RunSteps(sim, 600);
            string[] lines = CsvExport.TraitsOverTime(sim).Split('\n');
            int cols = lines[0].Split(',').Length;
            Assert.Greater(lines.Length, 3);
            for (int i = 1; i < lines.Length; i++)
                if (lines[i].Length > 0) Assert.AreEqual(cols, lines[i].Split(',').Length);
            StringAssert.DoesNotContain("\r", CsvExport.Species(sim));
            StringAssert.DoesNotContain(",,,", CsvExport.TraitsOverTime(sim));
        }

        [Test]
        public void PresetsAreRecognisedAndAnyChangeMakesThemCustom()
        {
            foreach (string name in Presets.Names)
            {
                SetupSettings s = Presets.Get(name);
                Assert.AreEqual(name, Presets.Match(s));
                s.ToxinLevel += 0.05f;
                Assert.AreEqual(Presets.Custom, Presets.Match(s));
            }
            Assert.IsNull(Presets.Get("nope"));
        }

        [Test]
        public void EveryPresetRunsForAMinuteWithoutExtinction()
        {
            foreach (string name in Presets.Names)
            {
                var sim = Simulation.Create(Presets.Get(name), new SimParameters());
                TestHelpers.RunSteps(sim, 1200);
                Assert.Greater(sim.Cells.AliveCount, 0, name);
            }
        }
    }
}
