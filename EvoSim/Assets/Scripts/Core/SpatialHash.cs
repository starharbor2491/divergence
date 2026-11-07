using System;

namespace Evo.Core
{
    /// <summary>
    /// Lookup helper rebuilt by counting sort into preallocated arrays. The buckets are not World state.
    /// </summary>
    public sealed class SpatialHash
    {
        readonly int cols, rows;
        readonly float inv, width, height;
        readonly int[] start;
        readonly int[] items;
        readonly int[] bucketOf;
        float[] xs, ys;
        int count;

        public SpatialHash(float width, float height, float bucketSize, int capacity)
        {
            this.width = width;
            this.height = height;
            inv = 1f / bucketSize;
            cols = (int)Math.Ceiling(width / bucketSize);
            rows = (int)Math.Ceiling(height / bucketSize);
            start = new int[cols * rows + 1];
            items = new int[capacity];
            bucketOf = new int[capacity];
        }

        int Bx(float x) { int b = (int)(x * inv); return b < 0 ? 0 : (b >= cols ? cols - 1 : b); }
        int By(float y) { int b = (int)(y * inv); return b < 0 ? 0 : (b >= rows ? rows - 1 : b); }

        public void Build(float[] x, float[] y, int n)
        {
            xs = x;
            ys = y;
            count = n;
            Array.Clear(start, 0, start.Length);
            for (int i = 0; i < n; i++)
            {
                int b = By(y[i]) * cols + Bx(x[i]);
                bucketOf[i] = b;
                start[b + 1]++;
            }
            for (int b = 0; b < cols * rows; b++) start[b + 1] += start[b];
            // Fill using a moving cursor stored in the same array, then restore.
            for (int i = 0; i < n; i++)
            {
                int b = bucketOf[i];
                items[start[b]++] = i;
            }
            for (int b = cols * rows; b > 0; b--) start[b] = start[b - 1];
            start[0] = 0;
        }

        /// <summary>Writes indices of items within radius r of (x,y) into results; returns the count. Stops when results is full.</summary>
        public int Query(float x, float y, float r, int[] results)
        {
            int n = 0;
            float r2 = r * r;
            int x0 = Bx(x - r), x1 = Bx(x + r), y0 = By(y - r), y1 = By(y + r);
            for (int by = y0; by <= y1; by++)
            {
                for (int bx = x0; bx <= x1; bx++)
                {
                    int b = by * cols + bx;
                    int e = start[b + 1];
                    for (int k = start[b]; k < e; k++)
                    {
                        int i = items[k];
                        float dx = xs[i] - x, dy = ys[i] - y;
                        if (dx * dx + dy * dy <= r2)
                        {
                            if (n >= results.Length) return n;
                            results[n++] = i;
                        }
                    }
                }
            }
            return n;
        }
    }
}
