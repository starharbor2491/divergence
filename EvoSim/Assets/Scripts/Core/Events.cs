using System;

namespace Evo.Core
{
    public enum EventKind
    {
        HeatWave = 0,
        IceAge = 1,
        Famine = 2,
        FoodBloom = 3,
        ToxinSpill = 4
    }

    public static class EventInfo
    {
        public static readonly string[] Names = { "Heat wave", "Ice age", "Famine", "Food bloom", "Toxin spill" };
        public static readonly string[] Letters = { "H", "I", "F", "B", "T" };
        public static readonly string[] Ids = { "heat-wave", "ice-age", "famine", "food-bloom", "toxin-spill" };

        public static bool HasIntensity(EventKind k) { return k == EventKind.HeatWave || k == EventKind.IceAge; }
        public static bool HasDuration(EventKind k) { return k != EventKind.ToxinSpill; }

        public static float DefaultIntensity(EventKind k) { return 0.3f; }
        public static float MinIntensity { get { return 0.1f; } }
        public static float MaxIntensity { get { return 0.5f; } }

        public static float MinDuration(EventKind k) { return k == EventKind.HeatWave || k == EventKind.IceAge ? 30f : 30f; }
        public static float MaxDuration(EventKind k) { return k == EventKind.HeatWave || k == EventKind.IceAge ? 180f : 120f; }
        public static float DefaultDuration(EventKind k) { return k == EventKind.HeatWave || k == EventKind.IceAge ? 90f : 60f; }
    }

    /// <summary>What the Player chose in the Event dialog.</summary>
    [Serializable]
    public class EventSpec
    {
        public EventKind Kind;
        public float X, Y;
        public float Radius = 20f;
        public float Intensity = 0.3f;
        public float Duration = 60f;
        public bool WholeWorld;
    }

    /// <summary>A placed Event that is still active.</summary>
    [Serializable]
    public class ActiveEvent
    {
        public EventSpec Spec;
        public float Age;
        public float Accumulator;
        public double PlacedTime;

        public float TotalLife(SimParameters p)
        {
            if (Spec.Kind == EventKind.HeatWave || Spec.Kind == EventKind.IceAge)
                return p.HeatRampUp + Spec.Duration + p.HeatRampDown;
            return Spec.Duration;
        }

        public float Envelope(SimParameters p)
        {
            if (Spec.Kind == EventKind.HeatWave || Spec.Kind == EventKind.IceAge)
            {
                if (Age < p.HeatRampUp) return Age / p.HeatRampUp;
                if (Age < p.HeatRampUp + Spec.Duration) return 1f;
                float e = 1f - (Age - p.HeatRampUp - Spec.Duration) / p.HeatRampDown;
                return e < 0f ? 0f : e;
            }
            return 1f;
        }

        /// <summary>Influence weight w = 1 - smoothstep(0.6 r, r, d); 1 everywhere for Whole World.</summary>
        public float Weight(float x, float y)
        {
            if (Spec.WholeWorld) return 1f;
            float dx = x - Spec.X, dy = y - Spec.Y;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            float e0 = 0.6f * Spec.Radius, e1 = Spec.Radius;
            if (d <= e0) return 1f;
            if (d >= e1) return 0f;
            float t = (d - e0) / (e1 - e0);
            return 1f - t * t * (3f - 2f * t);
        }
    }

    /// <summary>The permanent log of Events, drawn as markers on charts.</summary>
    [Serializable]
    public class EventRecord
    {
        public double Time;
        public EventKind Kind;
        public float X, Y, Radius, Intensity, Duration;
        public bool WholeWorld;
    }

    public sealed class EventSystem
    {
        public readonly ActiveEvent[] Active = new ActiveEvent[16];
        public int ActiveCount;
        public readonly System.Collections.Generic.List<EventRecord> Log = new System.Collections.Generic.List<EventRecord>();

        public void Clear()
        {
            for (int i = 0; i < Active.Length; i++) Active[i] = null;
            ActiveCount = 0;
            Log.Clear();
        }

        public bool CanAdd(SimParameters p) { return ActiveCount < p.MaxActiveEvents; }

        public void Add(ActiveEvent e)
        {
            Active[ActiveCount++] = e;
        }

        public void Update(float dt, SimParameters p)
        {
            int w = 0;
            for (int i = 0; i < ActiveCount; i++)
            {
                ActiveEvent e = Active[i];
                e.Age += dt;
                if (e.Age < e.TotalLife(p)) Active[w++] = e;
            }
            for (int i = w; i < ActiveCount; i++) Active[i] = null;
            ActiveCount = w;
        }

        public float TemperatureOffset(float x, float y, SimParameters p)
        {
            float sum = 0f;
            for (int i = 0; i < ActiveCount; i++)
            {
                ActiveEvent e = Active[i];
                if (e.Spec.Kind == EventKind.HeatWave) sum += e.Spec.Intensity * e.Envelope(p) * e.Weight(x, y);
                else if (e.Spec.Kind == EventKind.IceAge) sum -= e.Spec.Intensity * e.Envelope(p) * e.Weight(x, y);
            }
            return sum;
        }

        public bool HasTemperatureEvent()
        {
            for (int i = 0; i < ActiveCount; i++)
                if (Active[i].Spec.Kind == EventKind.HeatWave || Active[i].Spec.Kind == EventKind.IceAge) return true;
            return false;
        }
    }
}
