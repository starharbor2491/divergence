using System;
using Evo.Core;
using NUnit.Framework;

namespace Evo.Tests
{
    public class EnergyTests
    {
        static float ExpectedDrain(SimParameters p, Cell c, float thrust01)
        {
            float size = c.G.Trait[TraitId.Size];
            float speed = thrust01 * c.G.Trait[TraitId.Speed];
            float basis = 0.4f * (float)Math.Pow(size, 0.75) + 0.05f * c.G.Trait[TraitId.Sense]
                          + p.BaseResist * c.G.Trait[TraitId.ToxinResistance] + 0.4f * c.G.Trait[TraitId.TemperatureTolerance];
            return basis + 0.15f * speed * speed * (float)Math.Sqrt(size);
        }

        [Test]
        public void BaseAndMovementDrainMatchTheFormula()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            c.Energy = 50f;
            c.Refresh(sim.P);
            float expected = ExpectedDrain(sim.P, c, 0.5f) * sim.P.Dt;
            sim.Step();
            Assert.AreEqual(50f - expected, c.Energy, 1e-4f);
        }

        [Test]
        public void TemperatureStressIsEightTimesTheExcess()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            c.G.Trait[TraitId.PreferredTemperature] = 0.0f;
            c.G.Trait[TraitId.TemperatureTolerance] = 0.05f;
            c.Energy = 50f;
            c.Refresh(sim.P);
            float stress = Math.Abs(0.5f - 0f) - 0.05f;
            float expected = (ExpectedDrain(sim.P, c, 0.5f) + 8f * stress) * sim.P.Dt;
            sim.Step();
            Assert.AreEqual(50f - expected, c.Energy, 1e-4f);
        }

        [Test]
        public void ToxinDrainFollowsExposureAndResistance()
        {
            Simulation sim = TestHelpers.Quiet(1, p => { p.ToxinDrift = 0f; p.ToxinG = 0f; });
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.G.Trait[TraitId.ToxinResistance] = 0.5f;
            c.Energy = 50f;
            c.Refresh(sim.P);
            sim.ToxCount = 1;
            sim.ToxX[0] = 80.5f; sim.ToxY[0] = 45f; sim.ToxVX[0] = 0; sim.ToxVY[0] = 0; sim.ToxS[0] = 1f; sim.ToxHalf[0] = 1e9f;
            float rt = 2.5f + c.Radius;
            float exposure = Math.Min(1f, sim.P.ToxinExposure * (1f - 0.5f / rt));
            float tox = sim.P.ToxinCost * exposure * (1f - 0.9f * 0.5f);
            float expected = (ExpectedDrain(sim.P, c, 0f) + tox) * sim.P.Dt;
            sim.Step();
            Assert.AreEqual(50f - expected, c.Energy, 2e-3f);
        }

        [Test]
        public void OldAgeAddsExtraDrainAfterThreeHundredSeconds()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            c.Age = 400f;
            c.Energy = 50f;
            c.Refresh(sim.P);
            float expected = (ExpectedDrain(sim.P, c, 0.5f) + 0.01f * 100f) * sim.P.Dt;
            sim.Step();
            Assert.AreEqual(50f - expected, c.Energy, 1e-4f);
        }

        [Test]
        public void CauseOfDeathIsTheLargestDrainGroup()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            c.G.Trait[TraitId.PreferredTemperature] = 0.0f;
            c.G.Trait[TraitId.TemperatureTolerance] = 0.05f;
            c.Energy = 0.01f;
            c.Refresh(sim.P);
            for (int i = 0; i < 400; i++) { c.DrainTemperature = 3.6f; c.DrainBase = 1f; if (!c.Alive) break; sim.Step(); }
            Assert.AreEqual(0, sim.Cells.AliveCount);
            Assert.AreEqual(1, sim.Stats.TotalDeaths[(int)DeathCause.Temperature]);
        }
    }

    public class EatingTests
    {
        [Test]
        public void FoodGivesTwentyTimesOneMinusPointNineDiet()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.G.Trait[TraitId.Diet] = 0.5f;
            c.Energy = 30f;
            c.Refresh(sim.P);
            sim.FoodCount = 1;
            sim.FoodX[0] = 80.1f; sim.FoodY[0] = 45f;
            float noFood = 30f;
            // drain for one step with thrust 0
            float size = c.G.Trait[TraitId.Size];
            float drain = (0.4f * (float)Math.Pow(size, 0.75) + 0.05f * c.G.Trait[TraitId.Sense]
                           + sim.P.BaseResist * c.G.Trait[TraitId.ToxinResistance] + 0.4f * c.G.Trait[TraitId.TemperatureTolerance]) * sim.P.Dt;
            sim.Step();
            Assert.AreEqual(noFood - drain + 20f * (1f - 0.9f * 0.5f), c.Energy, 1e-3f);
            Assert.AreEqual(0, sim.FoodCount);
        }

        [Test]
        public void EnergyFromFoodIsCappedAtMaximum()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.G.Trait[TraitId.ReproductionThreshold] = 0.95f;
            c.Refresh(sim.P);
            c.Energy = c.Emax - 1f;
            sim.FoodCount = 1;
            sim.FoodX[0] = 80.1f; sim.FoodY[0] = 45f;
            sim.Step();
            Assert.LessOrEqual(c.Energy, c.Emax);
        }

        static Simulation PredatorPrey(float predatorSize, float preySize, float diet, out Cell predator, out Cell prey)
        {
            Simulation sim = TestHelpers.Quiet(2);
            predator = sim.Cells.Alive[0];
            prey = sim.Cells.Alive[1];
            TestHelpers.Tame(predator, 80f, 45f);
            TestHelpers.Tame(prey, 81f, 45f);
            TestHelpers.Freeze(predator);
            TestHelpers.Freeze(prey);
            predator.G.Weights[BrainLayout.B2Offset + BrainLayout.OutBite] = 3f;
            predator.G.Trait[TraitId.Size] = predatorSize;
            predator.G.Trait[TraitId.Diet] = diet;
            predator.G.Trait[TraitId.ReproductionThreshold] = 0.95f;
            prey.G.Trait[TraitId.Size] = preySize;
            prey.G.Trait[TraitId.ReproductionThreshold] = 0.95f;
            predator.Refresh(sim.P);
            prey.Refresh(sim.P);
            predator.Energy = 20f;
            prey.Energy = 40f;
            return sim;
        }

        [Test]
        public void PredatorEatsPreyAfterHalfASecondOfContact()
        {
            Cell predator, prey;
            Simulation sim = PredatorPrey(2f, 0.5f, 1f, out predator, out prey);
            float preyEnergy = prey.Energy, preyEmax = prey.Emax;
            for (int i = 0; i < 8; i++) sim.Step();
            Assert.IsTrue(prey.Alive, "needs a full 0.5 s of contact");
            for (int i = 0; i < 6; i++) sim.Step();
            Assert.AreEqual(1, sim.Stats.TotalDeaths[(int)DeathCause.Eaten]);
            Assert.AreEqual(1, sim.Cells.AliveCount);
            Assert.Greater(predator.Energy, 20f + 0.9f * (0.5f * (preyEnergy - 1f) + 0.3f * preyEmax) - 3f);
        }

        [Test]
        public void NoPredationWithoutTheSizeRatio()
        {
            Cell predator, prey;
            Simulation sim = PredatorPrey(1f, 0.95f, 1f, out predator, out prey);
            TestHelpers.RunSteps(sim, 40);
            Assert.AreEqual(0, sim.Stats.TotalDeaths[(int)DeathCause.Eaten]);
        }

        [Test]
        public void NoPredationWithLowDiet()
        {
            Cell predator, prey;
            Simulation sim = PredatorPrey(2f, 0.5f, 0.1f, out predator, out prey);
            TestHelpers.RunSteps(sim, 40);
            Assert.AreEqual(0, sim.Stats.TotalDeaths[(int)DeathCause.Eaten]);
        }

        [Test]
        public void NoPredationWhenOutOfReach()
        {
            Cell predator, prey;
            Simulation sim = PredatorPrey(2f, 0.5f, 1f, out predator, out prey);
            prey.X = 86f;
            TestHelpers.RunSteps(sim, 40);
            Assert.AreEqual(0, sim.Stats.TotalDeaths[(int)DeathCause.Eaten]);
        }
    }

    public class ReproductionTests
    {
        [Test]
        public void DivisionSplitsEnergyFiftyFortyTenAndAddsAGeneration()
        {
            Simulation sim = TestHelpers.Quiet(1, p => p.TraitMutationChance = 0f);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.G.Trait[TraitId.ReproductionThreshold] = 0.8f;
            c.G.Trait[TraitId.Size] = 1f;
            c.Refresh(sim.P);
            c.Energy = 90f;
            sim.Step();
            Assert.AreEqual(2, sim.Cells.AliveCount);
            Cell child = sim.Cells.Alive[1];
            Assert.AreEqual(1, child.Generation);
            Assert.AreEqual(c.Id, child.ParentId);
            float before = c.Energy / 0.5f; // energy right before the split
            Assert.AreEqual(0.4f * before, child.Energy, 1e-3f);
            Assert.AreEqual(90f, before + 0.9f, 1f); // only the step's small drain separates 90 from `before`
            Assert.AreEqual(0.1f * before, before - c.Energy - child.Energy, 1e-3f);
            float dist = (float)Math.Sqrt((child.X - c.X) * (child.X - c.X) + (child.Y - c.Y) * (child.Y - c.Y));
            Assert.AreEqual(c.Radius, dist, 1e-3f);
        }

        [Test]
        public void NoDivisionBelowThresholdOrAtThePopulationCap()
        {
            Simulation sim = TestHelpers.Quiet(1, p => p.MaxCells = 1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.G.Trait[TraitId.ReproductionThreshold] = 0.8f; c.G.Trait[TraitId.Size] = 1f; c.Refresh(sim.P);
            c.Energy = 99f;
            sim.Step();
            Assert.AreEqual(1, sim.Cells.AliveCount, "cap of 1");
            Simulation sim2 = TestHelpers.Quiet(1);
            Cell d = sim2.Cells.Alive[0];
            TestHelpers.Tame(d, 80f, 45f);
            TestHelpers.Freeze(d);
            d.G.Trait[TraitId.ReproductionThreshold] = 0.8f; d.G.Trait[TraitId.Size] = 1f; d.Refresh(sim2.P);
            d.Energy = 70f;
            sim2.Step();
            Assert.AreEqual(1, sim2.Cells.AliveCount, "below threshold");
        }

        [Test]
        public void ChildIsAMutatedCopy()
        {
            Simulation sim = TestHelpers.Quiet(1);
            Cell c = sim.Cells.Alive[0];
            TestHelpers.Tame(c, 80f, 45f);
            TestHelpers.Freeze(c);
            c.Energy = 90f;
            sim.Step();
            Cell child = sim.Cells.Alive[1];
            int same = 0;
            for (int j = 0; j < BrainLayout.WeightCount; j++) if (child.G.Weights[j] == c.G.Weights[j]) same++;
            Assert.Greater(same, 60);
            Assert.Less(same, BrainLayout.WeightCount);
        }

        static Simulation Pair(float choosiness, out Cell a, out Cell b)
        {
            Simulation sim = TestHelpers.Quiet(2, p => p.TraitMutationChance = 0f);
            a = sim.Cells.Alive[0];
            b = sim.Cells.Alive[1];
            TestHelpers.Tame(a, 80f, 45f);
            TestHelpers.Tame(b, 81f, 45f);
            TestHelpers.Freeze(a);
            TestHelpers.Freeze(b);
            foreach (Cell c in new[] { a, b })
            {
                c.G.Trait[TraitId.SexualTendency] = 1f;
                c.G.Trait[TraitId.Choosiness] = choosiness;
                c.G.Trait[TraitId.ReproductionThreshold] = 0.8f;
                c.G.Trait[TraitId.Size] = 1f;
                c.Refresh(sim.P);
                c.Energy = 95f;
            }
            return sim;
        }

        [Test]
        public void SexualPartnersPayThirtyPercentAndTheChildGetsFortyPercentOfThat()
        {
            Cell a, b;
            Simulation sim = Pair(0f, out a, out b);
            // make them identical so they are surely compatible
            b.G.CopyFrom(a.G);
            b.G.Trait[TraitId.Choosiness] = 0f;
            sim.Step();
            Assert.AreEqual(3, sim.Cells.AliveCount);
            Cell child = sim.Cells.Alive[2];
            Assert.AreEqual(1, child.Generation);
            Assert.AreEqual(0.4f * (0.3f * 95f * 2f), child.Energy, 0.2f);
            Assert.AreEqual(95f * 0.7f, b.Energy, 0.2f);
        }

        [Test]
        public void ChoosyCellsRefuseDistantRelatives()
        {
            Cell a, b;
            Simulation sim = Pair(1f, out a, out b);
            for (int j = 0; j < BrainLayout.WeightCount; j++) b.G.Weights[j] = a.G.Weights[j] + 0.5f;
            Assert.Greater(GeneticDistance.Distance(a.G, b.G, sim.P), 0.0375f);
            sim.Step();
            Assert.AreEqual(2, sim.Cells.AliveCount);
        }

        [Test]
        public void CrossoverChildTakesEachValueFromAParent()
        {
            Cell a, b;
            Simulation sim = Pair(0f, out a, out b);
            for (int j = 0; j < BrainLayout.WeightCount; j++) b.G.Weights[j] = a.G.Weights[j] + 0.01f * (j + 1);
            sim.P.WeightMutationChance = 0f;
            sim.Step();
            Assert.AreEqual(3, sim.Cells.AliveCount);
            Cell child = sim.Cells.Alive[2];
            for (int j = 0; j < BrainLayout.WeightCount; j++)
                Assert.IsTrue(child.G.Weights[j] == a.G.Weights[j] || child.G.Weights[j] == b.G.Weights[j]);
        }
    }

    public class ToxinTests
    {
        static Simulation Setup(Action<SimParameters> tweak)
        {
            Simulation sim = TestHelpers.Quiet(1, tweak);
            sim.Cells.Alive[0].Alive = false;
            sim.Cells.CompactDead();
            return sim;
        }

        static void Put(Simulation sim, int i, float x, float y, float half)
        {
            sim.ToxX[i] = x; sim.ToxY[i] = y; sim.ToxVX[i] = 0f; sim.ToxVY[i] = 0f; sim.ToxS[i] = 1f; sim.ToxHalf[i] = half;
        }

        static float Separation(Simulation sim)
        {
            float dx = sim.ToxX[1] - sim.ToxX[0], dy = sim.ToxY[1] - sim.ToxY[0];
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        [Test]
        public void AttractionKeepsNearbyParticlesTogether()
        {
            foreach (bool attract in new[] { true, false })
            {
                Simulation sim = Setup(p => { p.ToxinG = attract ? 2f : 0f; p.ToxinDrift = 0.6f; });
                sim.ToxCount = 2;
                Put(sim, 0, 60f, 45f, 1e9f);
                Put(sim, 1, 61f, 45f, 1e9f);
                for (int i = 0; i < 200; i++) sim.StepToxinForTest();
                if (attract) Assert.Less(Separation(sim), 6f);
                else Assert.Greater(Separation(sim), 0f);
            }
            // compare the two runs on identical random streams
            Simulation a = Setup(p => { p.ToxinG = 2f; });
            Simulation b = Setup(p => { p.ToxinG = 0f; });
            foreach (Simulation s in new[] { a, b })
            {
                s.ToxCount = 2;
                Put(s, 0, 60f, 45f, 1e9f);
                Put(s, 1, 61f, 45f, 1e9f);
                for (int i = 0; i < 300; i++) s.StepToxinForTest();
            }
            Assert.Less(Separation(a), Separation(b));
        }

        [Test]
        public void AccelerationIsCapped()
        {
            Simulation sim = Setup(p => { p.ToxinDrift = 0f; });
            int n = 200;
            sim.ToxCount = n;
            for (int i = 0; i < n; i++) Put(sim, i, 60f + (i % 10) * 0.05f, 45f + (i / 10) * 0.05f, 1e9f);
            sim.StepToxinForTest();
            for (int i = 0; i < n; i++)
            {
                float v = (float)Math.Sqrt(sim.ToxVX[i] * sim.ToxVX[i] + sim.ToxVY[i] * sim.ToxVY[i]);
                Assert.LessOrEqual(v, 3f * sim.P.Dt + 1e-4f);
            }
        }

        [Test]
        public void StrengthFadesByHalfLifeAndWeakParticlesAreRemoved()
        {
            Simulation sim = Setup(p => { p.ToxinDrift = 0f; });
            sim.ToxCount = 2;
            Put(sim, 0, 20f, 20f, 10f);
            Put(sim, 1, 120f, 70f, 1000f);
            for (int i = 0; i < 200; i++) sim.StepToxinForTest();   // 10 s
            Assert.AreEqual(0.5f, sim.ToxS[0], 0.01f);
            for (int i = 0; i < 800; i++) sim.StepToxinForTest();   // 40 s more: 2^-5 = 0.031 < 0.05
            Assert.AreEqual(1, sim.ToxCount);
        }

        [Test]
        public void ParticlesReflectAtTheWalls()
        {
            Simulation sim = Setup(p => { p.ToxinDrift = 0f; p.ToxinG = 0f; });
            sim.ToxCount = 1;
            Put(sim, 0, 0.05f, 45f, 1e9f);
            sim.ToxVX[0] = -5f;
            for (int i = 0; i < 100; i++) sim.StepToxinForTest();
            Assert.GreaterOrEqual(sim.ToxX[0], 0f);
            Assert.LessOrEqual(sim.ToxX[0], sim.World.Width);
        }

        [Test]
        public void ParticleCapIsRespected()
        {
            Simulation sim = TestHelpers.Quiet(20);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill, X = 80, Y = 45, Radius = 60f });
            sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill, X = 80, Y = 45, Radius = 60f });
            Assert.LessOrEqual(sim.ToxCount, sim.P.MaxToxin);
            Assert.AreEqual(sim.P.MaxToxin, sim.ToxCount);
        }

        [Test]
        public void StartingToxinFollowsTheSetting()
        {
            var none = Simulation.Create(new SetupSettings { SeedText = "x", ToxinLevel = 0f }, new SimParameters());
            Assert.AreEqual(0, none.ToxCount);
            var some = Simulation.Create(new SetupSettings { SeedText = "x", ToxinLevel = 0.5f }, new SimParameters());
            Assert.Greater(some.ToxCount, 250);
            Assert.LessOrEqual(some.ToxCount, 300);
        }
    }

    public class EventTests
    {
        [Test]
        public void EnvelopeRampsHoldsAndFades()
        {
            var p = new SimParameters();
            var e = new ActiveEvent { Spec = new EventSpec { Kind = EventKind.HeatWave, Duration = 90f } };
            e.Age = 0f; Assert.AreEqual(0f, e.Envelope(p), 1e-4f);
            e.Age = 5f; Assert.AreEqual(0.5f, e.Envelope(p), 1e-4f);
            e.Age = 50f; Assert.AreEqual(1f, e.Envelope(p), 1e-4f);
            e.Age = 115f; Assert.AreEqual(0.5f, e.Envelope(p), 1e-4f);
            Assert.AreEqual(130f, e.TotalLife(p), 1e-4f);
        }

        [Test]
        public void WeightIsOneInsideZeroOutsideAndSmoothBetween()
        {
            var e = new ActiveEvent { Spec = new EventSpec { Kind = EventKind.HeatWave, X = 50, Y = 50, Radius = 20f } };
            Assert.AreEqual(1f, e.Weight(50f, 50f), 1e-5f);
            Assert.AreEqual(1f, e.Weight(50f + 12f, 50f), 1e-5f);
            Assert.AreEqual(0f, e.Weight(50f + 20f, 50f), 1e-5f);
            Assert.AreEqual(0f, e.Weight(100f, 50f), 1e-5f);
            Assert.AreEqual(0.5f, e.Weight(50f + 16f, 50f), 1e-4f);
            e.Spec.WholeWorld = true;
            Assert.AreEqual(1f, e.Weight(1000f, 1000f));
        }

        [Test]
        public void HeatWaveRaisesTemperatureThenLetsItReturn()
        {
            Simulation sim = TestHelpers.Quiet(20, p => p.FoodRate = 26f);
            Assert.AreEqual(0.5f, sim.World.TemperatureAt(80f, 45f), 1e-4f);
            Assert.IsTrue(sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, X = 80, Y = 45, Radius = 20, Intensity = 0.3f, Duration = 60f }));
            TestHelpers.RunSteps(sim, 205);
            Assert.AreEqual(0.8f, sim.World.TemperatureAt(80f, 45f), 1e-3f);
            Assert.AreEqual(0.5f, sim.World.TemperatureAt(140f, 10f), 1e-4f);
            TestHelpers.RunSteps(sim, 20 * 100);
            Assert.AreEqual(0, sim.Events.ActiveCount);
            Assert.AreEqual(0.5f, sim.World.TemperatureAt(80f, 45f), 1e-4f);
        }

        [Test]
        public void IceAgeLowersTemperature()
        {
            Simulation sim = TestHelpers.Quiet(20);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.IceAge, WholeWorld = true, Intensity = 0.4f, Duration = 60f });
            TestHelpers.RunSteps(sim, 205);
            Assert.AreEqual(0.1f, sim.World.TemperatureAt(10f, 10f), 1e-3f);
        }

        [Test]
        public void FamineRemovesMostFoodAndSlowsNewFood()
        {
            var p = new SimParameters { FounderCount = 1, InitialFood = 600, FoodRate = 0f };
            var sim = Simulation.Create(new SetupSettings { SeedText = "famine", ToxinLevel = 0f }, p);
            sim.Cells.Alive[0].Alive = false;
            int before = sim.FoodCount;
            Assert.IsTrue(sim.PlaceEvent(new EventSpec { Kind = EventKind.Famine, X = 80, Y = 45, Radius = 200f, Duration = 60f }));
            Assert.Less(sim.FoodCount, before * 0.3f);
            Assert.Greater(sim.FoodCount, before * 0.1f);
        }

        [Test]
        public void FamineRejectsNewFoodInsideTheArea()
        {
            var p = new SimParameters { FounderCount = 1, InitialFood = 0, FoodRate = 200f };
            var sim = Simulation.Create(new SetupSettings { SeedText = "famine2", ToxinLevel = 0f }, p);
            var ref2 = Simulation.Create(new SetupSettings { SeedText = "famine2", ToxinLevel = 0f }, p);
            sim.Cells.Alive[0].Energy = 100000f; ref2.Cells.Alive[0].Energy = 100000f;
            sim.PlaceEvent(new EventSpec { Kind = EventKind.Famine, X = 80, Y = 45, Radius = 200f, Duration = 60f });
            TestHelpers.RunSteps(sim, 100);
            TestHelpers.RunSteps(ref2, 100);
            Assert.Less(sim.FoodCount, ref2.FoodCount * 0.25f);
        }

        [Test]
        public void FoodBloomAddsFoodUniformlyInsideTheArea()
        {
            var p = new SimParameters { FounderCount = 1, InitialFood = 0, FoodRate = 0f };
            var sim = Simulation.Create(new SetupSettings { SeedText = "bloom", ToxinLevel = 0f }, p);
            sim.Cells.Alive[0].Energy = 100000f;
            sim.PlaceEvent(new EventSpec { Kind = EventKind.FoodBloom, X = 40, Y = 40, Radius = 10f, Duration = 60f });
            TestHelpers.RunSteps(sim, 100);
            Assert.Greater(sim.FoodCount, 150);
            for (int i = 0; i < sim.FoodCount; i++)
            {
                float dx = sim.FoodX[i] - 40f, dy = sim.FoodY[i] - 40f;
                Assert.LessOrEqual(dx * dx + dy * dy, 10.01f * 10.01f);
            }
        }

        [Test]
        public void ToxinSpillAddsParticlesInProportionToRadius()
        {
            Simulation sim = TestHelpers.Quiet(20);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill, X = 80, Y = 45, Radius = 20f });
            Assert.AreEqual(600, sim.ToxCount);
            Assert.AreEqual(sim.P.ToxinHalfLifeSpill, sim.ToxHalf[0]);
        }

        [Test]
        public void AtMostEightEventsAreActiveAndEveryEventIsLogged()
        {
            Simulation sim = TestHelpers.Quiet(20);
            for (int i = 0; i < 8; i++) Assert.IsTrue(sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, X = 10 * i, Y = 20, Radius = 10, Duration = 60f }));
            Assert.IsFalse(sim.PlaceEvent(new EventSpec { Kind = EventKind.IceAge, X = 10, Y = 20, Radius = 10, Duration = 60f }));
            Assert.AreEqual(8, sim.Events.Log.Count);
            Assert.AreEqual(EventKind.HeatWave, sim.Events.Log[0].Kind);
        }
    }
}
