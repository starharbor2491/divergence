using System;
using System.Text;

namespace Evo.Core
{
    [Serializable]
    public class RngState
    {
        public int S0, S1, S2, S3;
        public bool HasSpare;
        public float Spare;
    }

    /// <summary>xoshiro128** generator. Never use System.Random or UnityEngine.Random in Evo.Core.</summary>
    public sealed class SeededRandom
    {
        uint s0, s1, s2, s3;
        bool hasSpare;
        float spare;

        public SeededRandom(uint seed)
        {
            uint x = seed;
            s0 = SplitMix32(ref x);
            s1 = SplitMix32(ref x);
            s2 = SplitMix32(ref x);
            s3 = SplitMix32(ref x);
            if ((s0 | s1 | s2 | s3) == 0) s0 = 1;
        }

        static uint SplitMix32(ref uint x)
        {
            x += 0x9E3779B9u;
            uint z = x;
            z = (z ^ (z >> 16)) * 0x85EBCA6Bu;
            z = (z ^ (z >> 13)) * 0xC2B2AE35u;
            return z ^ (z >> 16);
        }

        static uint Rotl(uint x, int k) { return (x << k) | (x >> (32 - k)); }

        public uint NextUInt()
        {
            uint result = Rotl(s1 * 5, 7) * 9;
            uint t = s1 << 9;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;
            s2 ^= t;
            s3 = Rotl(s3, 11);
            return result;
        }

        /// <summary>Uniform in [0,1) using 24 random bits.</summary>
        public float NextFloat01() { return (NextUInt() >> 8) * (1f / 16777216f); }

        public float NextRange(float min, float max) { return min + (max - min) * NextFloat01(); }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 1) return 0;
            return (int)(((ulong)NextUInt() * (ulong)maxExclusive) >> 32);
        }

        /// <summary>Marsaglia polar method. The cached second value is part of the saved state.</summary>
        public float NextGaussian()
        {
            if (hasSpare)
            {
                hasSpare = false;
                return spare;
            }
            float u, v, s;
            do
            {
                u = NextFloat01() * 2f - 1f;
                v = NextFloat01() * 2f - 1f;
                s = u * u + v * v;
            } while (s >= 1f || s == 0f);
            float mul = (float)Math.Sqrt(-2.0 * Math.Log(s) / s);
            spare = v * mul;
            hasSpare = true;
            return u * mul;
        }

        public RngState GetState()
        {
            return new RngState
            {
                S0 = unchecked((int)s0),
                S1 = unchecked((int)s1),
                S2 = unchecked((int)s2),
                S3 = unchecked((int)s3),
                HasSpare = hasSpare,
                Spare = spare
            };
        }

        public void SetState(RngState st)
        {
            s0 = unchecked((uint)st.S0);
            s1 = unchecked((uint)st.S1);
            s2 = unchecked((uint)st.S2);
            s3 = unchecked((uint)st.S3);
            hasSpare = st.HasSpare;
            spare = st.Spare;
        }
    }

    public static class SeedUtil
    {
        /// <summary>FNV-1a over the UTF-8 bytes of the text.</summary>
        public static uint FromText(string text)
        {
            if (text == null) text = "";
            uint h = 2166136261u;
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            for (int i = 0; i < bytes.Length; i++)
            {
                h ^= bytes[i];
                h *= 16777619u;
            }
            return h;
        }

    }
}
