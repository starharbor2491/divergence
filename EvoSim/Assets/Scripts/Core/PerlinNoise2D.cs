using System;

namespace Evo.Core
{
    /// <summary>Seeded 2D gradient noise. Not Mathf.PerlinNoise so the core runs identically in tests and in the browser.</summary>
    public sealed class PerlinNoise2D
    {
        readonly byte[] perm = new byte[512];

        public PerlinNoise2D(SeededRandom rng)
        {
            var p = new byte[256];
            for (int i = 0; i < 256; i++) p[i] = (byte)i;
            for (int i = 255; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                byte t = p[i]; p[i] = p[j]; p[j] = t;
            }
            for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
        }

        static float Fade(float t) { return t * t * t * (t * (t * 6f - 15f) + 10f); }

        static float Grad(int h, float x, float y)
        {
            const float d = 0.70710678f;
            switch (h & 7)
            {
                case 0: return x;
                case 1: return -x;
                case 2: return y;
                case 3: return -y;
                case 4: return (x + y) * d;
                case 5: return (x - y) * d;
                case 6: return (y - x) * d;
                default: return -(x + y) * d;
            }
        }

        static float Lerp(float a, float b, float t) { return a + (b - a) * t; }

        /// <summary>Returns a value in 0..1 (typically 0.2 to 0.8).</summary>
        public float Sample(float x, float y)
        {
            int xf = (int)Math.Floor(x);
            int yf = (int)Math.Floor(y);
            int xi = xf & 255;
            int yi = yf & 255;
            float fx = x - xf;
            float fy = y - yf;
            float u = Fade(fx);
            float v = Fade(fy);
            int aa = perm[perm[xi] + yi];
            int ab = perm[perm[xi] + yi + 1];
            int ba = perm[perm[xi + 1] + yi];
            int bb = perm[perm[xi + 1] + yi + 1];
            float x1 = Lerp(Grad(aa, fx, fy), Grad(ba, fx - 1f, fy), u);
            float x2 = Lerp(Grad(ab, fx, fy - 1f), Grad(bb, fx - 1f, fy - 1f), u);
            float n = Lerp(x1, x2, v);
            float r = 0.5f + n;
            return r < 0f ? 0f : (r > 1f ? 1f : r);
        }
    }
}
