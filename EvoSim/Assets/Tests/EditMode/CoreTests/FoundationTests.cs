using System;
using System.Collections.Generic;
using Evo.Core;
using NUnit.Framework;

namespace Evo.Tests
{
    public class SeededRandomTests
    {
        [Test]
        public void SameSeedGivesSameSequence()
        {
            var a = new SeededRandom(123);
            var b = new SeededRandom(123);
            for (int i = 0; i < 1000; i++) Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void DifferentSeedsDiffer()
        {
            var a = new SeededRandom(1);
            var b = new SeededRandom(2);
            int same = 0;
            for (int i = 0; i < 100; i++) if (a.NextUInt() == b.NextUInt()) same++;
            Assert.Less(same, 3);
        }

        [Test]
        public void SavedStateContinuesTheSameSequenceIncludingCachedGaussian()
        {
            var a = new SeededRandom(77);
            a.NextGaussian(); // leaves a cached second value
            RngState st = a.GetState();
            Assert.IsTrue(st.HasSpare);
            var b = new SeededRandom(5);
            b.SetState(st);
            for (int i = 0; i < 200; i++) Assert.AreEqual(a.NextGaussian(), b.NextGaussian());
            for (int i = 0; i < 200; i++) Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void NextFloat01StaysInHalfOpenRange()
        {
            var r = new SeededRandom(9);
            for (int i = 0; i < 100000; i++)
            {
                float f = r.NextFloat01();
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
            }
        }

        [Test]
        public void GaussianHasMeanZeroAndUnitSpread()
        {
            var r = new SeededRandom(31);
            double sum = 0, sq = 0;
            const int n = 50000;
            for (int i = 0; i < n; i++) { double g = r.NextGaussian(); sum += g; sq += g * g; }
            Assert.AreEqual(0.0, sum / n, 0.03);
            Assert.AreEqual(1.0, sq / n, 0.05);
        }

        [Test]
        public void SeedTextHashIsStableAndTextSensitive()
        {
            Assert.AreEqual(SeedUtil.FromText("mild"), SeedUtil.FromText("mild"));
            Assert.AreNotEqual(SeedUtil.FromText("mild"), SeedUtil.FromText("mile"));
            Assert.AreEqual(2166136261u, SeedUtil.FromText(""));
        }
    }

    public class PerlinTests
    {
        [Test]
        public void SameSeedSameValuesAndRange()
        {
            var a = new PerlinNoise2D(new SeededRandom(4));
            var b = new PerlinNoise2D(new SeededRandom(4));
            for (int i = 0; i < 500; i++)
            {
                float x = i * 0.37f, y = i * 0.21f;
                float v = a.Sample(x, y);
                Assert.AreEqual(v, b.Sample(x, y));
                Assert.GreaterOrEqual(v, 0f);
                Assert.LessOrEqual(v, 1f);
            }
        }

        [Test]
        public void DifferentSeedsDiffer()
        {
            var a = new PerlinNoise2D(new SeededRandom(4));
            var b = new PerlinNoise2D(new SeededRandom(5));
            int diff = 0;
            for (int i = 0; i < 100; i++) if (Math.Abs(a.Sample(i * 0.53f + 0.2f, i * 0.31f + 0.4f) - b.Sample(i * 0.53f + 0.2f, i * 0.31f + 0.4f)) > 1e-4f) diff++;
            Assert.Greater(diff, 80);
        }

        [Test]
        public void FieldIsSmooth()
        {
            var a = new PerlinNoise2D(new SeededRandom(8));
            for (int i = 0; i < 500; i++)
            {
                float x = i * 0.173f, y = i * 0.091f;
                Assert.Less(Math.Abs(a.Sample(x, y) - a.Sample(x + 0.01f, y)), 0.05f);
            }
        }
    }

    public class GenomeTests
    {
        static Genome Founder(SeededRandom r, SimParameters p)
        {
            var mean = new float[BrainLayout.WeightCount];
            BrainLayout.InnateWiring(1f, mean);
            var g = new Genome();
            Genome.MakeFounder(g, r, p, mean);
            return g;
        }

        [Test]
        public void LayoutConstantsMatchThePlan()
        {
            Assert.AreEqual(100, BrainLayout.WeightCount);
            Assert.AreEqual(66, BrainLayout.B1Offset);
            Assert.AreEqual(72, BrainLayout.W2Offset);
            Assert.AreEqual(96, BrainLayout.B2Offset);
            Assert.AreEqual(10, Traits.Count);
        }

        [Test]
        public void FoundersStartNearTheMeanAndNeverSexual()
        {
            var p = new SimParameters();
            var r = new SeededRandom(3);
            for (int k = 0; k < 200; k++)
            {
                Genome g = Founder(r, p);
                Assert.AreEqual(0f, g.Trait[TraitId.SexualTendency]);
                for (int i = 0; i < Traits.Count; i++)
                {
                    Assert.GreaterOrEqual(g.Trait[i], Traits.Min[i]);
                    Assert.LessOrEqual(g.Trait[i], Traits.Max[i]);
                }
            }
        }

        [Test]
        public void MutationKeepsEverythingInRange()
        {
            var p = new SimParameters();
            var r = new SeededRandom(11);
            Genome g = Founder(r, p);
            for (int k = 0; k < 3000; k++)
            {
                Genome.Mutate(g, r, p, 3f);
                for (int i = 0; i < Traits.Count; i++)
                {
                    Assert.GreaterOrEqual(g.Trait[i], Traits.Min[i]);
                    Assert.LessOrEqual(g.Trait[i], Traits.Max[i]);
                }
                for (int j = 0; j < BrainLayout.WeightCount; j++)
                {
                    Assert.GreaterOrEqual(g.Weights[j], -4f);
                    Assert.LessOrEqual(g.Weights[j], 4f);
                }
            }
        }

        [Test]
        public void ZeroMutationRateCopiesExactly()
        {
            var p = new SimParameters();
            var r = new SeededRandom(12);
            Genome a = Founder(r, p);
            var b = new Genome();
            b.CopyFrom(a);
            Genome.Mutate(b, r, p, 0f);
            CollectionAssert.AreEqual(a.Trait, b.Trait);
            CollectionAssert.AreEqual(a.Weights, b.Weights);
        }

        [Test]
        public void MutationActuallyChangesThings()
        {
            var p = new SimParameters();
            var r = new SeededRandom(13);
            Genome a = Founder(r, p);
            var b = new Genome();
            b.CopyFrom(a);
            Genome.Mutate(b, r, p, 1f);
            int changed = 0;
            for (int j = 0; j < BrainLayout.WeightCount; j++) if (a.Weights[j] != b.Weights[j]) changed++;
            Assert.Greater(changed, 0);
        }

        [Test]
        public void CrossoverTakesEachValueFromAParent()
        {
            var p = new SimParameters();
            var r = new SeededRandom(14);
            Genome a = Founder(r, p), b = Founder(r, p), c = new Genome();
            Genome.Crossover(a, b, c, r);
            for (int i = 0; i < Traits.Count; i++) Assert.IsTrue(c.Trait[i] == a.Trait[i] || c.Trait[i] == b.Trait[i]);
            for (int j = 0; j < BrainLayout.WeightCount; j++) Assert.IsTrue(c.Weights[j] == a.Weights[j] || c.Weights[j] == b.Weights[j]);
        }

        [Test]
        public void NormaliseRoundTrips()
        {
            for (int i = 0; i < Traits.Count; i++)
            {
                float v = Traits.Min[i] + 0.37f * Traits.Range(i);
                Assert.AreEqual(v, Traits.Denormalise(i, Traits.Normalise(i, v)), 1e-5f);
                Assert.AreEqual(0f, Traits.Normalise(i, Traits.Min[i]), 1e-6f);
                Assert.AreEqual(1f, Traits.Normalise(i, Traits.Max[i]), 1e-6f);
            }
        }

        [Test]
        public void DistanceIsZeroSymmetricAndGrows()
        {
            var p = new SimParameters();
            var r = new SeededRandom(15);
            Genome a = Founder(r, p), b = Founder(r, p);
            Assert.AreEqual(0f, GeneticDistance.Distance(a, a, p));
            Assert.AreEqual(GeneticDistance.Distance(a, b, p), GeneticDistance.Distance(b, a, p), 1e-6f);
            var c = new Genome();
            c.CopyFrom(a);
            float last = 0f;
            for (int k = 1; k <= 5; k++)
            {
                c.Trait[TraitId.Speed] = Math.Min(Traits.Max[TraitId.Speed], a.Trait[TraitId.Speed] + 0.3f * k);
                float d = GeneticDistance.Distance(a, c, p);
                Assert.Greater(d, last);
                last = d;
            }
        }

        [Test]
        public void MatingTraitsNeverCountTowardsDistance()
        {
            var p = new SimParameters();
            var r = new SeededRandom(16);
            Genome a = Founder(r, p);
            var b = new Genome();
            b.CopyFrom(a);
            b.Trait[TraitId.SexualTendency] = 1f;
            b.Trait[TraitId.Choosiness] = 0f;
            Assert.AreEqual(0f, GeneticDistance.Distance(a, b, p));
        }
    }

    public class BrainTests
    {
        [Test]
        public void ForwardIsDeterministicAndBounded()
        {
            var r = new SeededRandom(2);
            var w = new float[BrainLayout.WeightCount];
            for (int i = 0; i < w.Length; i++) w[i] = r.NextGaussian();
            var brain = new Brain();
            for (int k = 0; k < 200; k++)
            {
                for (int i = 0; i < BrainLayout.Inputs; i++) brain.Inputs[i] = r.NextRange(-1f, 1f);
                brain.Forward(w);
                float[] first = (float[])brain.Outputs.Clone();
                brain.Forward(w);
                for (int o = 0; o < BrainLayout.Outputs; o++)
                {
                    Assert.AreEqual(first[o], brain.Outputs[o]);
                    Assert.GreaterOrEqual(brain.Outputs[o], -1f);
                    Assert.LessOrEqual(brain.Outputs[o], 1f);
                }
            }
        }

        [Test]
        public void ZeroWeightsGiveZeroOutputs()
        {
            var brain = new Brain();
            brain.Inputs[0] = 1f;
            brain.Forward(new float[BrainLayout.WeightCount]);
            for (int o = 0; o < BrainLayout.Outputs; o++) Assert.AreEqual(0f, brain.Outputs[o]);
        }

        [Test]
        public void InnateWiringSteersTowardsFood()
        {
            var w = new float[BrainLayout.WeightCount];
            BrainLayout.InnateWiring(1f, w);
            var brain = new Brain();
            brain.Inputs[BrainLayout.InFoodDir] = 1f;     // food to the left
            brain.Inputs[BrainLayout.InFoodProx] = 1f;
            brain.Inputs[BrainLayout.InBias] = 1f;
            brain.Forward(w);
            Assert.Greater(brain.Outputs[BrainLayout.OutTurn], 0.3f);
            Assert.Greater(brain.Outputs[BrainLayout.OutThrust], 0.5f);
            brain.Inputs[BrainLayout.InFoodDir] = -1f;
            brain.Forward(w);
            Assert.Less(brain.Outputs[BrainLayout.OutTurn], -0.3f);
        }

#if !UNITY_EDITOR
        [Test]
        public void ForwardPassAllocatesNothing()
        {
            var w = new float[BrainLayout.WeightCount];
            var brain = new Brain();
            brain.Forward(w);
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) brain.Forward(w);
            long a1 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 3000; i++) brain.Forward(w);
            long a2 = GC.GetAllocatedBytesForCurrentThread();
            // Measuring itself costs a few bytes; a per-call allocation would grow with the loop length.
            Assert.LessOrEqual((a2 - a1) - (a1 - a0), 64);
            Assert.Less(a2 - a1, 256);
        }
#endif
    }

    public class SpatialHashTests
    {
        [Test]
        public void QueryEqualsBruteForce()
        {
            var r = new SeededRandom(21);
            const int n = 700;
            var xs = new float[n];
            var ys = new float[n];
            for (int i = 0; i < n; i++) { xs[i] = r.NextRange(0f, 160f); ys[i] = r.NextRange(0f, 90f); }
            var hash = new SpatialHash(160f, 90f, 5f, n);
            hash.Build(xs, ys, n);
            var buf = new int[n];
            for (int q = 0; q < 200; q++)
            {
                float qx = r.NextRange(-5f, 165f), qy = r.NextRange(-5f, 95f), rad = r.NextRange(0.5f, 15f);
                int cnt = hash.Query(qx, qy, rad, buf);
                var got = new List<int>();
                for (int i = 0; i < cnt; i++) got.Add(buf[i]);
                var expect = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    float dx = xs[i] - qx, dy = ys[i] - qy;
                    if (dx * dx + dy * dy <= rad * rad) expect.Add(i);
                }
                got.Sort();
                CollectionAssert.AreEqual(expect, got);
            }
        }

        [Test]
        public void RebuildWithFewerItemsWorks()
        {
            var xs = new float[] { 1f, 2f, 3f, 150f };
            var ys = new float[] { 1f, 2f, 3f, 80f };
            var hash = new SpatialHash(160f, 90f, 5f, 10);
            hash.Build(xs, ys, 4);
            var buf = new int[10];
            Assert.AreEqual(3, hash.Query(2f, 2f, 5f, buf));
            hash.Build(xs, ys, 2);
            Assert.AreEqual(2, hash.Query(2f, 2f, 5f, buf));
            Assert.AreEqual(0, hash.Query(150f, 80f, 5f, buf));
        }
    }

    public class ColorTests
    {
        static readonly float[] BrainMean = MakeMean();

        static float[] MakeMean()
        {
            var m = new float[BrainLayout.WeightCount];
            BrainLayout.InnateWiring(1f, m);
            return m;
        }

        static Genome FounderMeanGenome()
        {
            var g = new Genome();
            Array.Copy(Traits.FounderMean, g.Trait, Traits.Count);
            Array.Copy(BrainMean, g.Weights, BrainLayout.WeightCount);
            return g;
        }

        [Test]
        public void ZeroDeviationIsWhite()
        {
            float r, g, b;
            CellColor.Compute(FounderMeanGenome(), BrainMean, out r, out g, out b);
            Assert.AreEqual(1f, r, 1f / 255f);
            Assert.AreEqual(1f, g, 1f / 255f);
            Assert.AreEqual(1f, b, 1f / 255f);
        }

        [Test]
        public void ColourIsContinuous()
        {
            var rng = new SeededRandom(5);
            for (int k = 0; k < 200; k++)
            {
                Genome a = FounderMeanGenome();
                for (int j = 0; j < BrainLayout.WeightCount; j++) a.Weights[j] += rng.NextGaussian() * 0.6f;
                var b = new Genome();
                b.CopyFrom(a);
                b.Weights[rng.NextInt(BrainLayout.WeightCount)] += 0.01f;
                float r1, g1, b1, r2, g2, b2;
                CellColor.Compute(a, BrainMean, out r1, out g1, out b1);
                CellColor.Compute(b, BrainMean, out r2, out g2, out b2);
                Assert.Less(Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2), 0.06f);
            }
        }

        [Test]
        public void NearGenomesAreNearerInColourThanFarGenomes()
        {
            var rng = new SeededRandom(6);
            Genome a = FounderMeanGenome();
            for (int j = 0; j < BrainLayout.WeightCount; j++) a.Weights[j] += rng.NextGaussian() * 0.8f;
            Genome near = new Genome(), far = new Genome();
            near.CopyFrom(a);
            far.CopyFrom(a);
            for (int j = 0; j < BrainLayout.WeightCount; j++)
            {
                near.Weights[j] += rng.NextGaussian() * 0.03f;
                far.Weights[j] += rng.NextGaussian() * 1.2f;
            }
            float ar, ag, ab, nr, ng, nb, fr, fg, fb;
            CellColor.Compute(a, BrainMean, out ar, out ag, out ab);
            CellColor.Compute(near, BrainMean, out nr, out ng, out nb);
            CellColor.Compute(far, BrainMean, out fr, out fg, out fb);
            float dn = Math.Abs(ar - nr) + Math.Abs(ag - ng) + Math.Abs(ab - nb);
            float df = Math.Abs(ar - fr) + Math.Abs(ag - fg) + Math.Abs(ab - fb);
            Assert.Less(dn, df);
        }

        [Test]
        public void ContrastAgainstPanelIsAtLeastThree()
        {
            float bg = CellColor.Luminance(0x17 / 255f, 0x1B / 255f, 0x22 / 255f);
            float min = 99f;
            for (int deg = 0; deg < 360; deg++)
            {
                for (int si = 3; si <= 10; si++)
                {
                    float r, g, b;
                    CellColor.FromSaturation(si / 10f, deg * (float)Math.PI / 180f, out r, out g, out b);
                    min = Math.Min(min, CellColor.Contrast(CellColor.Luminance(r, g, b), bg));
                }
            }
            Assert.GreaterOrEqual(min, 3f);
        }

        [Test]
        public void OutputsAreAlwaysValidSrgb()
        {
            for (int deg = 0; deg < 360; deg += 3)
            {
                for (int si = 0; si <= 20; si++)
                {
                    float r, g, b;
                    CellColor.FromSaturation(si / 20f, deg * (float)Math.PI / 180f, out r, out g, out b);
                    foreach (float c in new[] { r, g, b })
                    {
                        Assert.IsFalse(float.IsNaN(c));
                        Assert.GreaterOrEqual(c, 0f);
                        Assert.LessOrEqual(c, 1f);
                    }
                }
            }
        }

        [Test]
        public void ZeroSaturationIsExactlyWhite()
        {
            float r, g, b;
            CellColor.FromSaturation(0f, 1.2f, out r, out g, out b);
            Assert.AreEqual(1f, r, 1e-3f);
            Assert.AreEqual(1f, g, 1e-3f);
            Assert.AreEqual(1f, b, 1e-3f);
        }
    }
}
