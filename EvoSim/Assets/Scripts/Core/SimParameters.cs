using System;

namespace Evo.Core
{
    /// <summary>Every tunable number in one place (PLAN.md sections 6 to 11). Values marked TUNE in the plan live here.</summary>
    [Serializable]
    public class SimParameters
    {
        // World and time
        public float WorldWidth = 160f;
        public float WorldHeight = 90f;
        public float Dt = 0.05f;
        public int FounderCount = 120;
        public int MaxCells = 600;
        public int PoolCapacity = 700;
        public float FounderStartEnergy = 0.6f;

        // Food
        public float FoodEnergy = 20f;
        public float FoodRadius = 0.15f;
        public float FoodRate = 26f;
        public int MaxFood = 1500;
        public int InitialFood = 600;
        public float FoodNoiseScale = 1f / 40f;
        public int FoodSpawnTries = 20;

        // Toxin
        public float ToxinHalfLifeCloud = 900f;
        public float ToxinHalfLifeSpill = 90f;
        public int MaxToxin = 1500;
        public float ToxinDamping = 1.5f;
        public float ToxinDrift = 0.6f;
        public float ToxinRc = 3f;
        public float ToxinG = 2f;
        public float ToxinAccelCap = 3f;
        public float ToxinMinStrength = 0.05f;
        public float ToxinSenseStrength = 0.3f;

        // Cell energy (per second)
        public float BaseSize = 0.4f;
        public float BaseSense = 0.05f;
        public float BaseResist = 0.1f;
        public float BaseTolerance = 0.4f;
        public float MoveCost = 0.15f;
        public float AgeStart = 300f;
        public float AgeCost = 0.01f;
        public float TempStressCost = 8f;
        public float ToxinCost = 20f;
        public float ToxinExposure = 0.4f;
        public float TurnRate = 3f;
        public float DrainTimeConstant = 5f;
        public float MaxEnergyPerSize = 100f;

        // Eating
        public float DietFoodPenalty = 0.9f;
        public float PredationRatio = 1.2f;
        public float PredationMinDiet = 0.3f;
        public float BiteOutputThreshold = 0.3f;
        public float BiteTime = 0.5f;
        public float BiteReach = 0.1f;
        public float PredatorEnergyShare = 0.5f;
        public float PredatorMaxShare = 0.3f;

        // Reproduction and mutation
        public float ParentKeep = 0.5f;
        public float ChildShare = 0.4f;
        public float TraitMutationChance = 0.35f;
        public float TraitMutationSd = 0.04f;
        public float WeightMutationChance = 0.08f;
        public float WeightMutationSd = 0.25f;
        public float BigStepChance = 0.02f;
        public float BigStepFactor = 5f;
        public float WeightLimit = 4f;
        public float FounderTraitSpread = 0.05f;
        public float FounderWeightSd = 0.2f;
        public float InnateWiring = 1f;

        // Sexual reproduction (section 11)
        public float SexThreshold = 0.5f;
        public float MateEnergyFraction = 0.6f;
        public float MateCost = 0.3f;
        public float MateChildShare = 0.4f;
        public float MateDistMaxFactor = 2f;
        public float MateDistMinFactor = 0.25f;

        // Species
        public float SpeciesTheta = 0.15f;
        public float SpeciesCheckSeconds = 2f;
        public int SpeciesMinGroup = 10;
        public float DistanceTraitWeight = 0.6f;
        public float DistanceBrainWeight = 0.4f;
        public float DistanceBrainDivisor = 2f;
        public float CentroidKeep = 0.8f;

        // Statistics
        public float SampleSeconds = 1f;
        public int MaxSamples = 3600;
        public float DeathBinSeconds = 10f;
        public float TraitBinHalfWindow = 0.35f;

        // Events
        public int MaxActiveEvents = 8;
        public float HeatRampUp = 10f;
        public float HeatRampDown = 30f;
        public float FamineRemoveShare = 0.8f;
        public float FamineRejectShare = 0.9f;
        public float BloomRate = 40f;
        public float SpillParticlesPer10Radius = 300f;

        public bool Validate()
        {
            float[] values = {
                WorldWidth, WorldHeight, Dt, FounderStartEnergy, FoodEnergy, FoodRadius, FoodRate, FoodNoiseScale,
                ToxinHalfLifeCloud, ToxinHalfLifeSpill, ToxinDamping, ToxinDrift, ToxinRc, ToxinG, ToxinAccelCap,
                ToxinMinStrength, ToxinSenseStrength, BaseSize, BaseSense, BaseResist, BaseTolerance, MoveCost,
                AgeStart, AgeCost, TempStressCost, ToxinCost, ToxinExposure, TurnRate, DrainTimeConstant, MaxEnergyPerSize,
                DietFoodPenalty, PredationRatio, PredationMinDiet, BiteOutputThreshold, BiteTime, BiteReach,
                PredatorEnergyShare, PredatorMaxShare, ParentKeep, ChildShare, TraitMutationChance, TraitMutationSd,
                WeightMutationChance, WeightMutationSd, BigStepChance, BigStepFactor, WeightLimit, FounderTraitSpread,
                FounderWeightSd, InnateWiring, SexThreshold, MateEnergyFraction, MateCost, MateChildShare,
                MateDistMaxFactor, MateDistMinFactor, SpeciesTheta, SpeciesCheckSeconds, DistanceTraitWeight,
                DistanceBrainWeight, DistanceBrainDivisor, CentroidKeep, SampleSeconds, DeathBinSeconds,
                TraitBinHalfWindow, HeatRampUp, HeatRampDown, FamineRemoveShare, FamineRejectShare, BloomRate,
                SpillParticlesPer10Radius
            };
            foreach (float value in values)
                if (!SnapshotValidation.Range(value, 0, 10000)) return false;
            float[] shares = { FounderStartEnergy, DietFoodPenalty, PredationMinDiet, BiteOutputThreshold,
                PredatorEnergyShare, PredatorMaxShare, ParentKeep, ChildShare, TraitMutationChance,
                WeightMutationChance, BigStepChance, SexThreshold, MateEnergyFraction, MateCost,
                MateChildShare, CentroidKeep, FamineRemoveShare, FamineRejectShare, ToxinMinStrength, ToxinSenseStrength };
            foreach (float share in shares) if (share > 1) return false;
            // Saved Runs must fit the shipped renderer and bounded Core work buffers.
            return SnapshotValidation.Range(Dt, 0.001, 1) &&
                SnapshotValidation.Range(WorldWidth, 20, 1000) && SnapshotValidation.Range(WorldHeight, 20, 1000) &&
                FounderCount >= 0 && FounderCount <= MaxCells && MaxCells >= 1 && MaxCells <= 600 &&
                PoolCapacity >= MaxCells && PoolCapacity <= 700 && MaxFood >= 0 && MaxFood <= 1500 &&
                InitialFood >= 0 && InitialFood <= MaxFood && FoodSpawnTries >= 1 && FoodSpawnTries <= 100 &&
                MaxToxin >= 0 && MaxToxin <= 1500 && MaxSamples >= 4 && MaxSamples <= 3600 &&
                MaxActiveEvents >= 0 && MaxActiveEvents <= 16 && SpeciesMinGroup >= 1 && SpeciesMinGroup <= 600 &&
                ToxinHalfLifeCloud > 0 && ToxinHalfLifeSpill > 0 && MaxEnergyPerSize > 0 && DrainTimeConstant >= Dt &&
                DistanceBrainDivisor > 0 && SpeciesTheta > 0 && TraitBinHalfWindow > 0 &&
                HeatRampUp > 0 && HeatRampDown > 0 && DeathBinSeconds >= 0.1f &&
                SampleSeconds >= Dt && SpeciesCheckSeconds >= Dt && FoodRate <= 1000 && BloomRate <= 1000 &&
                WeightLimit <= 100 && InnateWiring <= 100 && FounderWeightSd <= 100;
        }
    }

    [Serializable]
    public class SetupSettings
    {
        public string SeedText = "mild";
        public float FoodAbundance = 1f;
        public float TemperatureRange = 0.6f;
        public float ZoneScale = 1f;
        public float ToxinLevel = 0.3f;
        public float MutationRate = 1f;

        public SetupSettings Clone()
        {
            return (SetupSettings)MemberwiseClone();
        }
    }

    public static class Presets
    {
        public const string Custom = "Custom";

        public static readonly string[] Names = { "Mild world", "Cold gradient", "Toxic basin", "Rapid evolution" };

        public static SetupSettings Get(string name)
        {
            switch (name)
            {
                case "Mild world":
                    return new SetupSettings { SeedText = "mild", FoodAbundance = 1.0f, TemperatureRange = 0.4f, ZoneScale = 1.0f, ToxinLevel = 0.2f, MutationRate = 1.0f };
                case "Cold gradient":
                    return new SetupSettings { SeedText = "gradient", FoodAbundance = 0.9f, TemperatureRange = 1.0f, ZoneScale = 1.0f, ToxinLevel = 0.1f, MutationRate = 1.0f };
                case "Toxic basin":
                    return new SetupSettings { SeedText = "basin", FoodAbundance = 1.2f, TemperatureRange = 0.5f, ZoneScale = 1.5f, ToxinLevel = 0.9f, MutationRate = 1.0f };
                case "Rapid evolution":
                    return new SetupSettings { SeedText = "rapid", FoodAbundance = 1.45f, TemperatureRange = 0.85f, ZoneScale = 1.35f, ToxinLevel = 0.25f, MutationRate = 2.0f };
                default:
                    return null;
            }
        }

        /// <summary>Returns the Preset name that exactly matches the settings, or "Custom".</summary>
        public static string Match(SetupSettings s)
        {
            for (int i = 0; i < Names.Length; i++)
            {
                SetupSettings p = Get(Names[i]);
                if (p.SeedText == s.SeedText
                    && Math.Abs(p.FoodAbundance - s.FoodAbundance) < 0.0001f
                    && Math.Abs(p.TemperatureRange - s.TemperatureRange) < 0.0001f
                    && Math.Abs(p.ZoneScale - s.ZoneScale) < 0.0001f
                    && Math.Abs(p.ToxinLevel - s.ToxinLevel) < 0.0001f
                    && Math.Abs(p.MutationRate - s.MutationRate) < 0.0001f)
                    return Names[i];
            }
            return Custom;
        }
    }
}
