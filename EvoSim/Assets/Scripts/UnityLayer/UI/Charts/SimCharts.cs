using System;
using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>The x values of the statistics samples, in simulated seconds or in mean Generation.</summary>
    public sealed class SampleAxis
    {
        public float[] Xs = new float[0];
        public int Count;
        public bool Generation;

        public void Build(List<Sample> samples, bool generation)
        {
            Generation = generation;
            Count = samples.Count;
            if (Xs.Length < Count) Xs = new float[Mathf.Max(Count, Xs.Length * 2)];
            for (int i = 0; i < Count; i++)
            {
                Xs[i] = generation ? samples[i].MeanGeneration : samples[i].Time;
            }
        }

        public float Min { get { float v = Count > 0 ? Xs[0] : 0f; for (int i = 1; i < Count; i++) v = Mathf.Min(v, Xs[i]); return v; } }
        public float Max { get { float v = Count > 0 ? Xs[0] : 1f; for (int i = 1; i < Count; i++) v = Mathf.Max(v, Xs[i]); return v; } }

        public float TimeToX(List<Sample> samples, double t)
        {
            if (!Generation) return (float)t;
            for (int i = 0; i < Count; i++) if (samples[i].Time >= t) return Xs[i];
            return float.NaN;
        }
    }

    public abstract class SimChart : ChartElement
    {
        protected Simulation sim;
        protected readonly SampleAxis axis = new SampleAxis();
        protected bool byGeneration;

        public void Bind(Simulation s)
        {
            InvalidateHover(true);
            sim = s;
            Refresh();
        }

        public bool ByGeneration
        {
            get { return byGeneration; }
            set { byGeneration = value; Refresh(); }
        }

        public virtual void Refresh()
        {
            InvalidateHover();
            if (sim == null) return;
            axis.Build(sim.Stats.Samples, byGeneration);
            SetEvents(sim.Events.Log);
            eventTimeToX = t => axis.TimeToX(sim.Stats.Samples, t);
            Redraw();
        }

        protected override bool IsValidIndex(int index) { return sim != null && index >= 0 && index < axis.Count && index < sim.Stats.Samples.Count; }

        protected void SetXAxis()
        {
            if (byGeneration)
            {
                xMin = Mathf.Floor(axis.Min);
                xMax = Mathf.Max(xMin + 1f, Mathf.Ceil(axis.Max));
                NiceTicks(xMin, xMax, 5, xTicks);
                xFormat = v => v.ToString("0");
            }
            else
            {
                xMin = 0f;
                xMax = Mathf.Max(60f, axis.Max);
                NiceTicks(xMin, xMax, 5, xTicks);
                xFormat = v => TimeTick(v);
            }
        }

        protected int Stride(int count, int maxPoints)
        {
            return Mathf.Max(1, count / maxPoints);
        }

        protected override int SnapIndex(Vector2 local)
        {
            if (axis.Count == 0) return -1;
            float xv = xMin + (local.x - plot.xMin) / plot.width * (xMax - xMin);
            return Nearest(axis.Xs, axis.Count, xv);
        }

        protected override float IndexX(int index) { return X(axis.Xs[Mathf.Clamp(index, 0, axis.Count - 1)]); }

        protected string XTitle(int index)
        {
            Sample s = sim.Stats.Samples[index];
            return byGeneration ? "Generation " + Ui.F(s.MeanGeneration, 1) + "  (" + Ui.Mmss(s.Time) + ")" : "Time " + Ui.Mmss(s.Time);
        }
    }

    /// <summary>Mean line with a one standard deviation band plus a subtle Founder baseline reference.</summary>
    public sealed class TraitLineChart : SimChart
    {
        int trait;

        public int Trait
        {
            get { return trait; }
            set { trait = value; Refresh(); }
        }

        protected override void ComputeAxes()
        {
            var samples = sim.Stats.Samples;
            SetXAxis();
            float lo = float.MaxValue, hi = float.MinValue;
            for (int i = 0; i < axis.Count; i++)
            {
                if (samples[i].Count == 0) continue;
                lo = Mathf.Min(lo, samples[i].TraitMean[trait] - samples[i].TraitSd[trait]);
                hi = Mathf.Max(hi, samples[i].TraitMean[trait] + samples[i].TraitSd[trait]);
            }
            if (lo == float.MaxValue) { lo = Traits.Min[trait]; hi = Traits.Max[trait]; }
            lo = Mathf.Max(lo, Traits.Min[trait]);
            hi = Mathf.Min(hi, Traits.Max[trait]);
            float pad = Mathf.Max(0.02f * Traits.Range(trait), (hi - lo) * 0.12f);
            yMin = Mathf.Max(Traits.Min[trait], lo - pad);
            yMax = Mathf.Min(Traits.Max[trait], hi + pad);
            if (yMax - yMin < 0.05f * Traits.Range(trait))
            {
                float mid = (yMax + yMin) * 0.5f;
                yMin = mid - 0.025f * Traits.Range(trait);
                yMax = mid + 0.025f * Traits.Range(trait);
            }
            NiceTicks(yMin, yMax, 4, yTicks);
            yFormat = v => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }

        protected override void DrawData(Painter2D p)
        {
            // Founder average reference line so students immediately see divergence from Founders
            float founder = Traits.FounderMean[trait];
            if (founder >= yMin && founder <= yMax)
            {
                float fy = Y(founder);
                Color refCol = DesignTokens.WithAlpha(DesignTokens.Text3, 0.32f);
                for (float x = plot.xMin; x < plot.xMax; x += 10f)
                    Segment(p, new Vector2(x, fy), new Vector2(Mathf.Min(x + 5f, plot.xMax), fy), refCol, 1f);
            }

            var samples = sim.Stats.Samples;
            int n = axis.Count;
            while (n > 0 && samples[n - 1].Count == 0) n--;
            if (n < 2) return;
            int stride = Stride(n, 700);
            p.fillColor = DesignTokens.WithAlpha(DesignTokens.ChartSeries1, 0.22f);
            p.BeginPath();
            bool first = true;
            for (int i = 0; i < n; i += stride)
            {
                Vector2 v = new Vector2(X(axis.Xs[i]), Y(Mathf.Clamp(samples[i].TraitMean[trait] + samples[i].TraitSd[trait], yMin, yMax)));
                if (first) { p.MoveTo(v); first = false; } else p.LineTo(v);
            }
            p.LineTo(new Vector2(X(axis.Xs[n - 1]), Y(Mathf.Clamp(samples[n - 1].TraitMean[trait] + samples[n - 1].TraitSd[trait], yMin, yMax))));
            for (int i = n - 1; i >= 0; i -= stride)
                p.LineTo(new Vector2(X(axis.Xs[i]), Y(Mathf.Clamp(samples[i].TraitMean[trait] - samples[i].TraitSd[trait], yMin, yMax))));
            p.LineTo(new Vector2(X(axis.Xs[0]), Y(Mathf.Clamp(samples[0].TraitMean[trait] - samples[0].TraitSd[trait], yMin, yMax))));
            p.ClosePath();
            p.Fill();

            // Soft glow stroke + crisp 2.2px curve
            p.lineJoin = LineJoin.Round;
            p.lineCap = LineCap.Round;
            p.lineWidth = 4.5f;
            p.strokeColor = DesignTokens.WithAlpha(DesignTokens.ChartSeries1, 0.20f);
            StrokeMeanPath(p, samples, n, stride);

            p.lineWidth = 2.2f;
            p.strokeColor = DesignTokens.ChartSeries1;
            StrokeMeanPath(p, samples, n, stride);
        }

        void StrokeMeanPath(Painter2D p, List<Sample> samples, int n, int stride)
        {
            p.BeginPath();
            bool first = true;
            for (int i = 0; i < n; i += stride)
            {
                Vector2 v = new Vector2(X(axis.Xs[i]), Y(samples[i].TraitMean[trait]));
                if (first) { p.MoveTo(v); first = false; } else p.LineTo(v);
            }
            p.LineTo(new Vector2(X(axis.Xs[n - 1]), Y(samples[n - 1].TraitMean[trait])));
            p.Stroke();
        }

        protected override void DrawHover(Painter2D p, int index)
        {
            if (!IsValidIndex(index) || sim.Stats.Samples[index].Count == 0) return;
            HoverDot(p, new Vector2(X(axis.Xs[index]), Y(sim.Stats.Samples[index].TraitMean[trait])), DesignTokens.ChartSeries1);
        }

        protected override void FillHover(int index, List<ReadoutRow> rows, out string title)
        {
            Sample s = sim.Stats.Samples[index];
            title = XTitle(index);
            if (s.Count == 0)
            {
                rows.Add(new ReadoutRow { Name = "No living Cells", Value = "—" });
                return;
            }
            rows.Add(new ReadoutRow { Swatch = DesignTokens.ChartSeries1, Name = "mean", Value = Ui.F(s.TraitMean[trait], 2) });
            rows.Add(new ReadoutRow { Name = "standard deviation", Value = Ui.F(s.TraitSd[trait], 2) });
            rows.Add(new ReadoutRow { Name = "founder start", Value = Ui.F(Traits.FounderMean[trait], 2) });
            rows.Add(new ReadoutRow { Name = "cells", Value = Ui.Count(s.Count) });
        }
    }

    /// <summary>Common drawing for stacked areas with a crisp dark gap between fills.</summary>
    public abstract class StackedChart : SimChart
    {
        protected float[][] cumulative = new float[0][];
        protected Color[] layerColors = new Color[0];
        protected string[] layerNames = new string[0];
        protected int layerCount;
        protected virtual bool OmitEmptyPopulation { get { return false; } }

        protected void EnsureLayers(int layers, int samples)
        {
            layerCount = layers;
            if (cumulative.Length < layers) Array.Resize(ref cumulative, layers);
            for (int l = 0; l < layers; l++)
                if (cumulative[l] == null || cumulative[l].Length < samples) cumulative[l] = new float[Mathf.Max(samples, 64) * 2];
            if (layerColors.Length < layers) layerColors = new Color[layers];
            if (layerNames.Length < layers) layerNames = new string[layers];
        }

        protected override void DrawData(Painter2D p)
        {
            int n = axis.Count;
            if (OmitEmptyPopulation)
                while (n > 0 && sim.Stats.Samples[n - 1].Count == 0) n--;
            if (n < 2 || layerCount == 0) return;
            int stride = Stride(n, 600);
            for (int l = 0; l < layerCount; l++)
            {
                float[] top = cumulative[l];
                float[] bottom = l > 0 ? cumulative[l - 1] : null;
                p.fillColor = layerColors[l];
                p.BeginPath();
                bool first = true;
                for (int i = 0; i < n; i += stride)
                {
                    Vector2 v = new Vector2(X(axis.Xs[i]), Y(top[i]));
                    if (first) { p.MoveTo(v); first = false; } else p.LineTo(v);
                }
                p.LineTo(new Vector2(X(axis.Xs[n - 1]), Y(top[n - 1])));
                p.LineTo(new Vector2(X(axis.Xs[n - 1]), Y(bottom != null ? bottom[n - 1] : yMin)));
                for (int i = n - 1; i >= 0; i -= stride)
                    p.LineTo(new Vector2(X(axis.Xs[i]), Y(bottom != null ? bottom[i] : yMin)));
                p.LineTo(new Vector2(X(axis.Xs[0]), Y(bottom != null ? bottom[0] : yMin)));
                p.ClosePath();
                p.Fill();
            }
            p.lineWidth = 1.6f;
            p.strokeColor = DesignTokens.Bg0;
            for (int l = 0; l < layerCount; l++)
            {
                float[] top = cumulative[l];
                p.BeginPath();
                bool first = true;
                for (int i = 0; i < n; i += stride)
                {
                    Vector2 v = new Vector2(X(axis.Xs[i]), Y(top[i]));
                    if (first) { p.MoveTo(v); first = false; } else p.LineTo(v);
                }
                p.LineTo(new Vector2(X(axis.Xs[n - 1]), Y(top[n - 1])));
                p.Stroke();
            }
        }

        protected override void DrawHover(Painter2D p, int index)
        {
            for (int l = 0; l < layerCount; l++)
            {
                float lo = l > 0 ? cumulative[l - 1][index] : 0f;
                float mid = (lo + cumulative[l][index]) * 0.5f;
                if (cumulative[l][index] - lo <= 0f) continue;
                HoverDot(p, new Vector2(X(axis.Xs[index]), Y(mid)), layerColors[l]);
            }
        }
    }

    /// <summary>Share of living Cells in each of the six bins of one Trait.</summary>
    public sealed class TraitFrequencyChart : StackedChart
    {
        protected override bool OmitEmptyPopulation { get { return true; } }
        int trait;

        public int Trait
        {
            get { return trait; }
            set { trait = value; Refresh(); }
        }

        public static string BinRange(int trait, int bin, SimParameters p)
        {
            float lo = bin == 0 ? Traits.Min[trait] : TraitBins.EdgeLow(trait, bin, p);
            float hi = bin == TraitBins.Count - 1 ? Traits.Max[trait] : TraitBins.EdgeLow(trait, bin + 1, p);
            lo = Mathf.Clamp(lo, Traits.Min[trait], Traits.Max[trait]);
            hi = Mathf.Clamp(hi, Traits.Min[trait], Traits.Max[trait]);
            if (hi - lo < 0.005f) return "out of range";
            return Ui.F(lo, 2) + " to " + Ui.F(hi, 2);
        }

        protected override void ComputeAxes()
        {
            var samples = sim.Stats.Samples;
            SetXAxis();
            yMin = 0f; yMax = 1f;
            NiceTicks(0f, 1f, 4, yTicks);
            yFormat = v => Mathf.RoundToInt(v * 100f) + "%";
            EnsureLayers(TraitBins.Count, axis.Count);
            for (int b = 0; b < TraitBins.Count; b++)
            {
                layerColors[b] = DesignTokens.Bins[b];
                layerNames[b] = BinRange(trait, b, sim.P);
            }
            for (int i = 0; i < axis.Count; i++)
            {
                Sample s = samples[i];
                float run = 0f;
                float inv = s.Count > 0 ? 1f / s.Count : 0f;
                for (int b = 0; b < TraitBins.Count; b++)
                {
                    run += s.Bins[trait * TraitBins.Count + b] * inv;
                    cumulative[b][i] = Mathf.Min(1f, run);
                }
            }
        }

        protected override void FillHover(int index, List<ReadoutRow> rows, out string title)
        {
            Sample s = sim.Stats.Samples[index];
            title = XTitle(index);
            if (s.Count == 0)
            {
                rows.Add(new ReadoutRow { Name = "No living Cells", Value = "—" });
                return;
            }
            for (int b = TraitBins.Count - 1; b >= 0; b--)
            {
                float share = s.Count > 0 ? (float)s.Bins[trait * TraitBins.Count + b] / s.Count : 0f;
                rows.Add(new ReadoutRow { Swatch = DesignTokens.Bins[b], Name = layerNames[b], Value = Mathf.RoundToInt(share * 100f) + "%" });
            }
        }
    }

    /// <summary>Population per Species: the 20 largest by peak, the rest as Other.</summary>
    public sealed class SpeciesChart : StackedChart
    {
        public const int MaxLayers = 20;
        int[] layerIds = new int[MaxLayers + 1];
        readonly Dictionary<int, int> layerOf = new Dictionary<int, int>();
        readonly List<SpeciesRecord> order = new List<SpeciesRecord>();
        public bool HasOther { get { return sim != null && sim.Species.Records.Count > order.Count; } }
        public bool IsAggregated(int id) { return HasOther && !layerOf.ContainsKey(id); }
        public int OtherPopulation
        {
            get
            {
                int count = 0;
                if (sim != null)
                    for (int i = 0; i < sim.Cells.AliveCount; i++)
                        if (IsAggregated(sim.Cells.Alive[i].SpeciesId)) count++;
                return count;
            }
        }

        public override void Refresh()
        {
            if (sim != null) SelectSpecies();
            base.Refresh();
        }

        void SelectSpecies()
        {
            order.Clear();
            order.AddRange(sim.Species.Records);
            if (order.Count > MaxLayers)
            {
                order.Sort((a, b) => b.PeakPopulation.CompareTo(a.PeakPopulation));
                order.RemoveRange(MaxLayers, order.Count - MaxLayers);
            }
            order.Sort((a, b) => a.Id.CompareTo(b.Id));
            layerOf.Clear();
            for (int l = 0; l < order.Count; l++) layerOf[order[l].Id] = l;
        }

        protected override void ComputeAxes()
        {
            var samples = sim.Stats.Samples;
            SetXAxis();
            bool hasOther = sim.Species.Records.Count > order.Count;
            int layers = order.Count + (hasOther ? 1 : 0);
            EnsureLayers(layers, axis.Count);
            for (int l = 0; l < order.Count; l++)
            {
                layerOf[order[l].Id] = l;
                layerIds[l] = order[l].Id;
                layerColors[l] = new Color(order[l].ColorR, order[l].ColorG, order[l].ColorB, 1f);
                layerNames[l] = order[l].Name;
            }
            if (hasOther) { layerColors[order.Count] = DesignTokens.Text3; layerNames[order.Count] = "Other"; layerIds[order.Count] = -1; }

            float maxTotal = 1f;
            for (int i = 0; i < axis.Count; i++)
            {
                Sample s = samples[i];
                for (int l = 0; l < layers; l++) cumulative[l][i] = 0f;
                for (int k = 0; k < s.SpeciesIds.Length; k++)
                {
                    int layer;
                    if (!layerOf.TryGetValue(s.SpeciesIds[k], out layer)) layer = layers - 1;
                    if (layer >= 0 && layer < layers) cumulative[layer][i] += s.SpeciesPops[k];
                }
                for (int l = 1; l < layers; l++) cumulative[l][i] += cumulative[l - 1][i];
                if (layers > 0) maxTotal = Mathf.Max(maxTotal, cumulative[layers - 1][i]);
            }
            yMin = 0f;
            yMax = Mathf.Max(10f, maxTotal * 1.05f);
            NiceTicks(0f, yMax, 4, yTicks);
            if (yTicks.Count > 0) yMax = Mathf.Max(yMax, yTicks[yTicks.Count - 1]);
            yFormat = v => Ui.Count(Mathf.RoundToInt(v));
        }

        protected override void FillHover(int index, List<ReadoutRow> rows, out string title)
        {
            Sample s = sim.Stats.Samples[index];
            title = XTitle(index);
            for (int l = 0; l < layerCount; l++)
            {
                float lo = l > 0 ? cumulative[l - 1][index] : 0f;
                int v = Mathf.RoundToInt(cumulative[l][index] - lo);
                if (v > 0) rows.Add(new ReadoutRow { Swatch = layerColors[l], Name = layerNames[l], Value = Ui.Count(v) });
            }
            rows.Reverse();
            rows.Add(new ReadoutRow { Name = "cells", Value = Ui.Count(s.Count) });
        }
    }

    /// <summary>Deaths by cause in 10 s bins, stacked. Shows the latest 60 bins.</summary>
    public sealed class DeathsChart : SimChart
    {
        const int Window = 60;
        int firstBin, bins;
        readonly float[] binX = new float[Window];

        public static readonly string[] CauseNames = { "eaten", "temperature", "old age", "starved", "poisoned" };

        protected override void ComputeAxes()
        {
            int total = sim.Stats.DeathBinCount;
            int current = (int)(sim.Time / sim.P.DeathBinSeconds) + 1;
            bins = Mathf.Min(Window, Mathf.Max(current, total));
            firstBin = Mathf.Max(0, Mathf.Max(current, total) - bins);
            float maxStack = 5f;
            for (int b = 0; b < bins; b++)
            {
                int sum = 0;
                for (int c = 0; c < 5; c++) sum += Count(firstBin + b, c);
                maxStack = Mathf.Max(maxStack, sum);
                binX[b] = (firstBin + b) * sim.P.DeathBinSeconds;
            }
            xMin = firstBin * sim.P.DeathBinSeconds;
            xMax = xMin + bins * sim.P.DeathBinSeconds;
            yMin = 0f;
            yMax = maxStack * 1.1f;
            NiceTicks(0f, yMax, 4, yTicks);
            if (yTicks.Count > 0) yMax = Mathf.Max(yMax, yTicks[yTicks.Count - 1]);
            NiceTicks(xMin, xMax, 5, xTicks);
            xFormat = v => TimeTick(v);
            yFormat = v => Mathf.RoundToInt(v).ToString();
            eventTimeToX = t => (float)t;
            SetEvents(sim.Events.Log);
        }

        int Count(int bin, int cause)
        {
            int idx = bin * 5 + cause;
            return bin < sim.Stats.DeathBinCount && idx < sim.Stats.DeathBins.Length ? sim.Stats.DeathBins[idx] : 0;
        }

        protected override void DrawData(Painter2D p)
        {
            float barW = plot.width / Mathf.Max(1, bins) * 0.76f;
            for (int b = 0; b < bins; b++)
            {
                float cx = X(binX[b] + sim.P.DeathBinSeconds * 0.5f);
                float acc = 0f;
                for (int c = 0; c < 5; c++)
                {
                    int v = Count(firstBin + b, c);
                    if (v == 0) continue;
                    float y0 = Y(acc), y1 = Y(acc + v);
                    acc += v;
                    p.fillColor = DesignTokens.Causes[c];
                    p.BeginPath();
                    p.MoveTo(new Vector2(cx - barW * 0.5f, y0));
                    p.LineTo(new Vector2(cx + barW * 0.5f, y0));
                    p.LineTo(new Vector2(cx + barW * 0.5f, y1));
                    p.LineTo(new Vector2(cx - barW * 0.5f, y1));
                    p.ClosePath();
                    p.Fill();
                    p.lineWidth = 1.5f;
                    p.strokeColor = DesignTokens.Bg0;
                    p.BeginPath();
                    p.MoveTo(new Vector2(cx - barW * 0.5f, y1));
                    p.LineTo(new Vector2(cx + barW * 0.5f, y1));
                    p.Stroke();
                }
            }
        }

        protected override int SnapIndex(Vector2 local)
        {
            if (bins == 0) return -1;
            int b = Mathf.FloorToInt((local.x - plot.xMin) / plot.width * bins);
            return Mathf.Clamp(b, 0, bins - 1);
        }

        protected override bool IsValidIndex(int index) { return index >= 0 && index < bins; }

        protected override float IndexX(int index) { return X(binX[index] + sim.P.DeathBinSeconds * 0.5f); }

        protected override void FillHover(int index, List<ReadoutRow> rows, out string title)
        {
            title = Ui.Mmss(binX[index]) + " to " + Ui.Mmss(binX[index] + sim.P.DeathBinSeconds);
            for (int c = 4; c >= 0; c--)
                rows.Add(new ReadoutRow { Swatch = DesignTokens.Causes[c], Name = CauseNames[c], Value = Ui.Count(Count(firstBin + index, c)) });
        }
    }

    /// <summary>Evolutionary cladogram of Species with smooth curved divergence forks. Clicking a line highlights that Species.</summary>
    public sealed class TreeChart : SimChart
    {
        public event Action<int> SpeciesClicked;

        readonly List<SpeciesRecord> rows = new List<SpeciesRecord>();
        readonly Dictionary<int, int> rowOf = new Dictionary<int, int>();
        float rowHeight;
        int maxPeak = 1;

        public TreeChart()
        {
            HoverClicked += i => { if (i >= 0 && i < rows.Count && SpeciesClicked != null) SpeciesClicked(rows[i].Id); };
        }

        protected override bool UseCrosshair { get { return false; } }
        protected override bool IsValidIndex(int index) { return index >= 0 && index < rows.Count; }

        public override void Refresh()
        {
            if (sim != null)
                style.height = Mathf.Max(DesignTokens.TreeMinHeight, MarginTop + MarginBottom + sim.Species.Records.Count * DesignTokens.TreeRowHeight);
            base.Refresh();
        }

        void Walk(SpeciesRecord s, Dictionary<int, List<SpeciesRecord>> children)
        {
            rowOf[s.Id] = rows.Count;
            rows.Add(s);
            List<SpeciesRecord> kids;
            if (children.TryGetValue(s.Id, out kids)) foreach (SpeciesRecord k in kids) Walk(k, children);
        }

        protected override void ComputeAxes()
        {
            rows.Clear(); rowOf.Clear();
            var children = new Dictionary<int, List<SpeciesRecord>>();
            var roots = new List<SpeciesRecord>();
            maxPeak = 1;
            foreach (SpeciesRecord s in sim.Species.Records)
            {
                maxPeak = Mathf.Max(maxPeak, s.PeakPopulation);
                List<SpeciesRecord> list;
                if (s.ParentId == 0 || sim.Species.Get(s.ParentId) == null) roots.Add(s);
                else
                {
                    if (!children.TryGetValue(s.ParentId, out list)) { list = new List<SpeciesRecord>(); children[s.ParentId] = list; }
                    list.Add(s);
                }
            }
            foreach (SpeciesRecord r in roots) Walk(r, children);
            xMin = 0f;
            xMax = Mathf.Max(60f, (float)sim.Time);
            yMin = 0f; yMax = 1f;
            NiceTicks(xMin, xMax, 5, xTicks);
            xFormat = v => TimeTick(v);
            rowHeight = DesignTokens.TreeRowHeight;
            eventTimeToX = t => (float)t;
        }

        float RowY(int row) { return plot.yMin + rowHeight * (row + 0.5f); }

        float EndTime(SpeciesRecord s) { return s.Extinct ? (float)s.ExtinctTime : (float)sim.Time; }

        protected override void AddExtraLabels()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                Label l = GetLabel(96f);
                Ui.Type(l, FontRole.Medium, DesignTokens.TypeCaptionSize);
                l.text = rows[i].Name;
                l.style.unityTextAlign = TextAnchor.MiddleLeft;
                l.style.color = rows[i].Extinct ? DesignTokens.Text3 : DesignTokens.Text2;
                float x = Mathf.Min(X(EndTime(rows[i])) + 8f, plot.xMax - 96f);
                l.style.left = x;
                l.style.top = RowY(i) - DesignTokens.TypeCaptionLine * 0.5f - (x < X(EndTime(rows[i])) ? DesignTokens.TypeCaptionLine * 0.7f : 0f);
            }
        }

        protected override void DrawData(Painter2D p)
        {
            p.lineCap = LineCap.Round;
            // Smooth evolutionary divergence forks first
            for (int i = 0; i < rows.Count; i++)
            {
                int parentRow;
                if (rows[i].ParentId != 0 && rowOf.TryGetValue(rows[i].ParentId, out parentRow))
                {
                    float bx = X((float)rows[i].BirthTime);
                    float py = RowY(parentRow), cy = RowY(i);
                    float curve = Mathf.Min(10f, Mathf.Max(2f, X(EndTime(rows[i])) - bx));
                    p.lineWidth = 1.6f;
                    p.strokeColor = DesignTokens.WithAlpha(DesignTokens.Text2, 0.55f);
                    p.BeginPath();
                    p.MoveTo(new Vector2(bx, py));
                    p.BezierCurveTo(new Vector2(bx, Mathf.Lerp(py, cy, 0.65f)), new Vector2(bx, cy), new Vector2(bx + curve * 0.25f, cy));
                    p.Stroke();
                }
            }
            for (int i = 0; i < rows.Count; i++)
            {
                SpeciesRecord s = rows[i];
                if (i == hoverIndex)
                {
                    p.fillColor = DesignTokens.WithAlpha(DesignTokens.Bg3, 0.45f);
                    p.BeginPath();
                    float ry = RowY(i) - rowHeight * 0.46f;
                    p.MoveTo(new Vector2(plot.xMin, ry));
                    p.LineTo(new Vector2(plot.xMax, ry));
                    p.LineTo(new Vector2(plot.xMax, ry + rowHeight * 0.92f));
                    p.LineTo(new Vector2(plot.xMin, ry + rowHeight * 0.92f));
                    p.ClosePath();
                    p.Fill();
                }

                int pop = s.Extinct ? s.LastPopulation : s.CurrentPopulation;
                float width = Mathf.Lerp(1.6f, 6.4f, Mathf.Clamp01(pop / (float)maxPeak));
                Color c = new Color(s.ColorR, s.ColorG, s.ColorB, s.Extinct ? 0.62f : 1f);
                float y = RowY(i);
                float x0 = X((float)s.BirthTime), x1 = X(EndTime(s));
                Segment(p, new Vector2(x0, y), new Vector2(Mathf.Max(x0 + 1f, x1), y), c, width);

                // Speciation origin node
                p.fillColor = c;
                p.BeginPath(); p.Arc(new Vector2(x0, y), Mathf.Max(2.4f, width * 0.55f), Angle.Degrees(0), Angle.Degrees(360)); p.Fill();

                if (s.Extinct)
                {
                    Segment(p, new Vector2(x1, y - 4.5f), new Vector2(x1, y + 4.5f), DesignTokens.Text3, 1.4f);
                }
            }
        }

        protected override int SnapIndex(Vector2 local)
        {
            if (rows.Count == 0) return -1;
            int row = Mathf.FloorToInt((local.y - plot.yMin) / rowHeight);
            return row >= 0 && row < rows.Count ? row : -1;
        }

        protected override void FillHover(int index, List<ReadoutRow> r, out string title)
        {
            SpeciesRecord s = rows[index];
            title = s.Name;
            r.Add(new ReadoutRow { Swatch = new Color(s.ColorR, s.ColorG, s.ColorB, 1f), Name = "born", Value = Ui.Mmss(s.BirthTime) });
            r.Add(new ReadoutRow { Name = "birth generation", Value = s.BirthGeneration.ToString() });
            r.Add(new ReadoutRow { Name = "peak population", Value = Ui.Count(s.PeakPopulation) });
            r.Add(new ReadoutRow { Name = s.Extinct ? "died out" : "population now", Value = s.Extinct ? Ui.Mmss(s.ExtinctTime) : Ui.Count(s.CurrentPopulation) });
        }
    }
}
