using System;

namespace Evo.Core
{
    /// <summary>
    /// Perceptual Cell colour (DESIGN.md section 7). Zero deviation from the Founders is exactly white;
    /// hue comes from the direction of the deviation, chroma from its size, lightness stays at 0.78 or above.
    /// </summary>
    public static class CellColor
    {
        public const int Dim = Traits.ChartCount + BrainLayout.WeightCount; // 108
        public const uint ProjectionSeed = 0xC0102026u;
        public const float FullColourDeviation = 0.50f;

        static float[] px, py;

        static void EnsureProjection()
        {
            if (px != null) return;
            var rng = new SeededRandom(ProjectionSeed);
            px = MakeUnit(rng);
            py = MakeUnit(rng);
        }

        static float[] MakeUnit(SeededRandom rng)
        {
            var v = new float[Dim];
            double sum = 0;
            for (int i = 0; i < Dim; i++)
            {
                v[i] = rng.NextGaussian();
                sum += (double)v[i] * v[i];
            }
            float inv = (float)(1.0 / Math.Sqrt(sum));
            for (int i = 0; i < Dim; i++) v[i] *= inv;
            return v;
        }

        /// <summary>Deviation from the Founders as the two projected numbers (x, y) of DESIGN 7 steps 1 and 2.</summary>
        public static void Project(Genome g, float[] brainMean, out float x, out float y)
        {
            EnsureProjection();
            double dx = 0, dy = 0;
            float traitScale = (float)Math.Sqrt(0.6 / 8.0);
            for (int i = 0; i < Traits.ChartCount; i++)
            {
                float u = traitScale * (g.Trait[i] - Traits.FounderMean[i]) / (Traits.Max[i] - Traits.Min[i]);
                dx += px[i] * u;
                dy += py[i] * u;
            }
            float weightScale = (float)Math.Sqrt(0.4 / 100.0);
            for (int j = 0; j < BrainLayout.WeightCount; j++)
            {
                float u = weightScale * (g.Weights[j] - brainMean[j]) / 2f;
                dx += px[Traits.ChartCount + j] * u;
                dy += py[Traits.ChartCount + j] * u;
            }
            float k = (float)Math.Sqrt(Dim);
            x = k * (float)dx;
            y = k * (float)dy;
        }

        public static void Compute(Genome g, float[] brainMean, out float r, out float gr, out float b)
        {
            float x, y;
            Project(g, brainMean, out x, out y);
            float dist = (float)Math.Sqrt(x * x + y * y);
            float s = dist / FullColourDeviation;
            s = s < 0f ? 0f : (s > 1f ? 1f : s);
            float hue = (float)Math.Atan2(y, x);
            FromSaturation(s, hue, out r, out gr, out b);
        }

        /// <summary>OKLCH with L = 1 - 0.22 s, C = 0.16 s, gamut-mapped by reducing chroma. Output is sRGB 0..1.</summary>
        public static void FromSaturation(float s, float hueRadians, out float r, out float g, out float b)
        {
            float L = 1f - 0.22f * s;
            float C = 0.16f * s;
            float lr, lg, lb;
            OklchToLinear(L, C, hueRadians, out lr, out lg, out lb);
            if (!InGamut(lr, lg, lb))
            {
                float lo = 0f, hi = C;
                for (int i = 0; i < 30; i++)
                {
                    float mid = 0.5f * (lo + hi);
                    OklchToLinear(L, mid, hueRadians, out lr, out lg, out lb);
                    if (InGamut(lr, lg, lb)) lo = mid; else hi = mid;
                }
                OklchToLinear(L, lo, hueRadians, out lr, out lg, out lb);
            }
            r = Encode(lr);
            g = Encode(lg);
            b = Encode(lb);
        }

        static bool InGamut(float r, float g, float b)
        {
            const float lo = -0.00001f, hi = 1.00001f;
            return r >= lo && r <= hi && g >= lo && g <= hi && b >= lo && b <= hi;
        }

        public static void OklchToLinear(float L, float C, float H, out float r, out float g, out float b)
        {
            float a = C * (float)Math.Cos(H);
            float bb = C * (float)Math.Sin(H);
            float l_ = L + 0.3963377774f * a + 0.2158037573f * bb;
            float m_ = L - 0.1055613458f * a - 0.0638541728f * bb;
            float s_ = L - 0.0894841775f * a - 1.2914855480f * bb;
            float l = l_ * l_ * l_;
            float m = m_ * m_ * m_;
            float s = s_ * s_ * s_;
            r = 4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
            g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
            b = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;
        }

        public static void SrgbToOklab(float r, float g, float b, out float L, out float a, out float bb)
        {
            float lr = Decode(r), lg = Decode(g), lb = Decode(b);
            float l = 0.4122214708f * lr + 0.5363325363f * lg + 0.0514459929f * lb;
            float m = 0.2119034982f * lr + 0.6806995451f * lg + 0.1073969566f * lb;
            float s = 0.0883024619f * lr + 0.2817188376f * lg + 0.6299787005f * lb;
            float l_ = (float)Math.Pow(l, 1.0 / 3.0), m_ = (float)Math.Pow(m, 1.0 / 3.0), s_ = (float)Math.Pow(s, 1.0 / 3.0);
            L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
            a = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
            bb = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;
        }

        public static void OklabToSrgb(float L, float a, float b, out float r, out float g, out float bl)
        {
            float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
            float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
            float s_ = L - 0.0894841775f * a - 1.2914855480f * b;
            float l = l_ * l_ * l_, m = m_ * m_ * m_, s = s_ * s_ * s_;
            r = Encode(4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s);
            g = Encode(-1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s);
            bl = Encode(-0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s);
        }

        public static float Encode(float c)
        {
            if (c <= 0f) return 0f;
            if (c >= 1f) return 1f;
            float v = c <= 0.0031308f ? 12.92f * c : 1.055f * (float)Math.Pow(c, 1.0 / 2.4) - 0.055f;
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }

        public static float Decode(float c)
        {
            return c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4);
        }

        /// <summary>WCAG relative luminance of an sRGB colour.</summary>
        public static float Luminance(float r, float g, float b)
        {
            return 0.2126f * Decode(r) + 0.7152f * Decode(g) + 0.0722f * Decode(b);
        }

        public static float Contrast(float l1, float l2)
        {
            float hi = Math.Max(l1, l2), lo = Math.Min(l1, l2);
            return (hi + 0.05f) / (lo + 0.05f);
        }
    }
}
