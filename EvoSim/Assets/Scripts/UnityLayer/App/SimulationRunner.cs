using System.Diagnostics;
using Evo.Core;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Turns real time into fixed simulation steps (PLAN 8.1). Each frame adds real time times speed to an accumulator and
    /// runs steps while the accumulator allows, but stops at a budget of real time, so the effective speed may fall below the selected one.
    /// </summary>
    public sealed class SimulationRunner
    {
        public const double BudgetMilliseconds = 10.0;
        public const float MaxFrameSeconds = 0.1f;
        public const float MaxBacklogSeconds = 1f;
        public static readonly int[] Speeds = { 1, 2, 5, 10 };

        readonly Stopwatch watch = new Stopwatch();
        float accumulator;

        /// <summary>Simulated seconds advanced per real second over the last second, for display.</summary>
        public float EffectiveSpeed { get; private set; }
        float windowSim, windowReal;

        public void Reset()
        {
            accumulator = 0f;
            EffectiveSpeed = 0f;
            windowSim = windowReal = 0f;
        }

        /// <summary>Advances the simulation. Returns the number of steps run.</summary>
        public int Advance(Simulation sim, float realSeconds, int speed)
        {
            if (realSeconds > MaxFrameSeconds) realSeconds = MaxFrameSeconds;
            accumulator += realSeconds * speed;
            float dt = sim.P.Dt;
            int steps = 0;
            watch.Reset();
            watch.Start();
            while (accumulator >= dt && !sim.Extinct)
            {
                sim.Step();
                accumulator -= dt;
                steps++;
                if (watch.Elapsed.TotalMilliseconds >= BudgetMilliseconds) break;
            }
            watch.Stop();
            if (accumulator > MaxBacklogSeconds) accumulator = MaxBacklogSeconds;
            if (sim.Extinct) accumulator = 0f;

            windowSim += steps * dt;
            windowReal += realSeconds;
            if (windowReal >= 1f)
            {
                EffectiveSpeed = windowSim / windowReal;
                windowSim = windowReal = 0f;
            }
            return steps;
        }
    }
}
