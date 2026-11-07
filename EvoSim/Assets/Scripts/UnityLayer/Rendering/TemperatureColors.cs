using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>The Temperature tint ramp: cold, neutral, hot, interpolated in OKLab (DESIGN 2.2). Shared by the World and the legend.</summary>
    public static class TemperatureColors
    {
        const int Steps = 256;
        static Color32[] lut;

        static void Build()
        {
            if (lut != null) return;
            lut = new Color32[Steps];
            Lab cold = ToLab(DesignTokens.TempCold), mid = ToLab(DesignTokens.TempMid), hot = ToLab(DesignTokens.TempHot);
            for (int i = 0; i < Steps; i++)
            {
                float t = i / (float)(Steps - 1);
                Lab a = t < 0.5f ? cold : mid, b = t < 0.5f ? mid : hot;
                float u = t < 0.5f ? t * 2f : (t - 0.5f) * 2f;
                float r, g, bl;
                CellColor.OklabToSrgb(Mathf.Lerp(a.L, b.L, u), Mathf.Lerp(a.A, b.A, u), Mathf.Lerp(a.B, b.B, u), out r, out g, out bl);
                lut[i] = new Color32((byte)Mathf.RoundToInt(r * 255f), (byte)Mathf.RoundToInt(g * 255f), (byte)Mathf.RoundToInt(bl * 255f), 255);
            }
        }

        struct Lab { public float L, A, B; }

        static Lab ToLab(Color c)
        {
            var l = new Lab();
            CellColor.SrgbToOklab(c.r, c.g, c.b, out l.L, out l.A, out l.B);
            return l;
        }

        public static Color32 At(float temperature)
        {
            Build();
            int i = Mathf.Clamp(Mathf.RoundToInt(temperature * (Steps - 1)), 0, Steps - 1);
            return lut[i];
        }
    }
}
