using System;

namespace Evo.Core
{
    public static class TraitId
    {
        public const int Size = 0;
        public const int Speed = 1;
        public const int Sense = 2;
        public const int PreferredTemperature = 3;
        public const int TemperatureTolerance = 4;
        public const int ToxinResistance = 5;
        public const int Diet = 6;
        public const int ReproductionThreshold = 7;
        public const int SexualTendency = 8;
        public const int Choosiness = 9;
    }

    public static class Traits
    {
        public const int Count = 10;
        /// <summary>Traits 0 to 7 count towards Species distance, colour and charts.</summary>
        public const int ChartCount = 8;

        public static readonly string[] Names =
        {
            "Size", "Speed", "Sense range", "Preferred Temperature", "Temperature tolerance",
            "Toxin resistance", "Diet", "Reproduction threshold", "Sexual tendency", "Choosiness"
        };

        public static readonly float[] Min = { 0.5f, 0.5f, 2f, 0f, 0.05f, 0f, 0f, 0.5f, 0f, 0f };
        public static readonly float[] Max = { 3.0f, 4.0f, 15f, 1f, 0.5f, 1f, 1f, 0.95f, 1f, 1f };
        public static readonly float[] FounderMean = { 1.0f, 1.5f, 7f, 0.5f, 0.25f, 0.1f, 0.05f, 0.8f, 0f, 0.5f };

        public static float Range(int i) { return Max[i] - Min[i]; }
        public static float Normalise(int i, float v) { return (v - Min[i]) / (Max[i] - Min[i]); }
        public static float Denormalise(int i, float n) { return Min[i] + n * (Max[i] - Min[i]); }
        public static float Clamp(int i, float v) { return v < Min[i] ? Min[i] : (v > Max[i] ? Max[i] : v); }
    }

    public static class BrainLayout
    {
        public const int Inputs = 11;
        public const int Hidden = 6;
        public const int Outputs = 4;
        public const int W1Offset = 0;
        public const int B1Offset = Inputs * Hidden;                 // 66
        public const int W2Offset = B1Offset + Hidden;               // 72
        public const int B2Offset = W2Offset + Hidden * Outputs;     // 96
        public const int WeightCount = B2Offset + Outputs;           // 100

        public const int InFoodDir = 0, InFoodProx = 1, InPreyDir = 2, InPreyProx = 3, InThreatDir = 4, InThreatProx = 5,
            InToxinDir = 6, InToxinProx = 7, InTempMismatch = 8, InEnergy = 9, InBias = 10;
        public const int OutTurn = 0, OutThrust = 1, OutBite = 2, OutMate = 3;

        /// <summary>The innate Founder wiring with no noise (the Founder Brain mean).</summary>
        public static void InnateWiring(float scale, float[] dest)
        {
            Array.Clear(dest, 0, WeightCount);
            dest[W1Offset + InFoodDir * Hidden + 0] += 1.5f * scale;
            dest[W2Offset + 0 * Outputs + OutTurn] += 1.0f * scale;
            dest[W1Offset + InFoodProx * Hidden + 1] += 1.5f * scale;
            dest[W2Offset + 1 * Outputs + OutThrust] += 1.0f * scale;
            dest[B2Offset + OutThrust] += 0.3f * scale;
        }
    }

    /// <summary>The complete set of a Cell's inherited values: every Trait plus every Brain weight.</summary>
    public sealed class Genome
    {
        public readonly float[] Trait = new float[Traits.Count];
        public readonly float[] Weights = new float[BrainLayout.WeightCount];

        public void CopyFrom(Genome o)
        {
            Array.Copy(o.Trait, Trait, Traits.Count);
            Array.Copy(o.Weights, Weights, BrainLayout.WeightCount);
        }

        public float Normalised(int i) { return Traits.Normalise(i, Trait[i]); }

        public static void MakeFounder(Genome g, SeededRandom rng, SimParameters p, float[] brainMean)
        {
            for (int i = 0; i < Traits.Count; i++)
            {
                float v = Traits.FounderMean[i] + rng.NextGaussian() * p.FounderTraitSpread * Traits.Range(i);
                g.Trait[i] = Traits.Clamp(i, v);
            }
            g.Trait[TraitId.SexualTendency] = 0f;
            for (int j = 0; j < BrainLayout.WeightCount; j++)
                g.Weights[j] = rng.NextGaussian() * p.FounderWeightSd + brainMean[j];
        }

        /// <summary>Mutation (PLAN 8.9). Mutates all ten Traits (sexual Traits can evolve) and every Brain weight.</summary>
        public static void Mutate(Genome g, SeededRandom rng, SimParameters p, float rate)
        {
            if (rate <= 0f) return;
            for (int i = 0; i < Traits.Count; i++)
            {
                if (rng.NextFloat01() < p.TraitMutationChance)
                {
                    float step = rng.NextGaussian() * p.TraitMutationSd * Traits.Range(i) * rate;
                    if (rng.NextFloat01() < p.BigStepChance) step *= p.BigStepFactor;
                    g.Trait[i] = Traits.Clamp(i, g.Trait[i] + step);
                }
            }
            for (int j = 0; j < BrainLayout.WeightCount; j++)
            {
                if (rng.NextFloat01() < p.WeightMutationChance)
                {
                    float step = rng.NextGaussian() * p.WeightMutationSd * rate;
                    if (rng.NextFloat01() < p.BigStepChance) step *= p.BigStepFactor;
                    float w = g.Weights[j] + step;
                    g.Weights[j] = w < -p.WeightLimit ? -p.WeightLimit : (w > p.WeightLimit ? p.WeightLimit : w);
                }
            }
        }

        public static void Crossover(Genome a, Genome b, Genome child, SeededRandom rng)
        {
            for (int i = 0; i < Traits.Count; i++) child.Trait[i] = rng.NextFloat01() < 0.5f ? a.Trait[i] : b.Trait[i];
            for (int j = 0; j < BrainLayout.WeightCount; j++) child.Weights[j] = rng.NextFloat01() < 0.5f ? a.Weights[j] : b.Weights[j];
        }
    }

    public static class GeneticDistance
    {
        /// <summary>D = 0.6 Dtraits + 0.4 Dbrain (PLAN 9.2). Traits 8 and 9 never count.</summary>
        public static float Distance(Genome a, Genome b, SimParameters p)
        {
            float st = 0f;
            for (int i = 0; i < Traits.ChartCount; i++)
            {
                float d = (a.Trait[i] - b.Trait[i]) / (Traits.Max[i] - Traits.Min[i]);
                st += d * d;
            }
            float dTraits = (float)Math.Sqrt(st / Traits.ChartCount);
            float sb = 0f;
            for (int j = 0; j < BrainLayout.WeightCount; j++)
            {
                float d = a.Weights[j] - b.Weights[j];
                sb += d * d;
            }
            float dBrain = (float)Math.Sqrt(sb / BrainLayout.WeightCount) / p.DistanceBrainDivisor;
            return p.DistanceTraitWeight * dTraits + p.DistanceBrainWeight * dBrain;
        }
    }
}
