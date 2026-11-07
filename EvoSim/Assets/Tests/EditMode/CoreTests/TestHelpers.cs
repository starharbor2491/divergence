using System;
using Evo.Core;

namespace Evo.Tests
{
    public static class TestHelpers
    {
        /// <summary>A small, quiet World: no Food stream, no Toxin, uniform Temperature 0.5, innate wiring off.</summary>
        public static Simulation Quiet(int founders, Action<SimParameters> tweak = null, string seed = "test")
        {
            var p = new SimParameters { FounderCount = founders, InitialFood = 0, FoodRate = 0f, InnateWiring = 0f };
            if (tweak != null) tweak(p);
            var setup = new SetupSettings { SeedText = seed, ToxinLevel = 0f, TemperatureRange = 0f };
            return Simulation.Create(setup, p);
        }

        /// <summary>Makes a Cell sit still with a neutral Brain (all weights 0 gives thrust 0.5).</summary>
        public static void Tame(Cell c, float x, float y)
        {
            Array.Clear(c.G.Weights, 0, BrainLayout.WeightCount);
            c.G.Trait[TraitId.PreferredTemperature] = 0.5f;
            c.G.Trait[TraitId.TemperatureTolerance] = 0.25f;
            c.G.Trait[TraitId.Diet] = 0f;
            c.G.Trait[TraitId.SexualTendency] = 0f;
            c.X = x;
            c.Y = y;
        }

        /// <summary>Stops a Cell from moving: thrust output is pushed to -1.</summary>
        public static void Freeze(Cell c)
        {
            c.G.Weights[BrainLayout.B2Offset + BrainLayout.OutThrust] = -9f;
        }

        public static void Refresh(Simulation sim, Cell c)
        {
            c.Refresh(sim.P);
        }

        public static void RunSteps(Simulation sim, int n)
        {
            for (int i = 0; i < n; i++) sim.Step();
        }
    }
}
