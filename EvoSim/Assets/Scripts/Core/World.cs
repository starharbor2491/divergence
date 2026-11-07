using System;

namespace Evo.Core
{
    /// <summary>The generated environment: a smooth Temperature field and a Food richness field, both computed on demand.</summary>
    public sealed class World
    {
        public readonly float Width, Height;
        readonly PerlinNoise2D tempNoise, foodNoise;
        readonly SetupSettings setup;
        readonly SimParameters p;
        readonly EventSystem events;

        public World(SimParameters p, SetupSettings setup, SeededRandom worldRng, EventSystem events)
        {
            this.p = p;
            this.setup = setup;
            this.events = events;
            Width = p.WorldWidth;
            Height = p.WorldHeight;
            tempNoise = new PerlinNoise2D(worldRng);
            foodNoise = new PerlinNoise2D(worldRng);
        }

        static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }

        public float BaseTemperature(float x, float y)
        {
            float g = 0.5f - y / Height;
            float n = tempNoise.Sample(x * setup.ZoneScale / 60f, y * setup.ZoneScale / 60f);
            return Clamp01(0.5f + setup.TemperatureRange * (0.6f * g + 0.4f * (n - 0.5f)));
        }

        public float TemperatureAt(float x, float y)
        {
            float t = BaseTemperature(x, y);
            if (events.ActiveCount > 0) t += events.TemperatureOffset(x, y, p);
            return Clamp01(t);
        }

        public float FoodRichnessAt(float x, float y)
        {
            float n = foodNoise.Sample(x * p.FoodNoiseScale, y * p.FoodNoiseScale);
            return Clamp01(1.4f * n - 0.2f);
        }
    }
}
