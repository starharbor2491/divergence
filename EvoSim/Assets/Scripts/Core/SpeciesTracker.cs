using System;
using System.Collections.Generic;

namespace Evo.Core
{
    [Serializable]
    public class SpeciesRecord
    {
        public int Id;
        public int ParentId;           // 0 when there is no parent
        public string Name;
        public double BirthTime;
        public int BirthGeneration;
        public double ExtinctTime = -1;   // negative while alive
        public int PeakPopulation;
        public int CurrentPopulation;
        public int LastPopulation;     // last nonzero count observed at a Species check
        public int EmptyChecks;
        public float ColorR = 1f, ColorG = 1f, ColorB = 1f;
        public readonly Genome Centroid = new Genome();

        public bool Extinct { get { return ExtinctTime >= 0; } }
    }

    /// <summary>A group of unassigned Cells seen once; it becomes a Species if it is still there at the next check.</summary>
    public sealed class Candidate
    {
        public readonly Genome Centroid = new Genome();
        public int Size;
    }

    public sealed class SpeciesTracker
    {
        readonly SimParameters p;
        public readonly List<SpeciesRecord> Records = new List<SpeciesRecord>();
        public List<Candidate> Pending = new List<Candidate>();
        public int NextId = 1;

        // scratch, allocated once
        class Group
        {
            public readonly Genome Leader = new Genome();
            public readonly double[] SumT = new double[Traits.Count];
            public readonly double[] SumW = new double[BrainLayout.WeightCount];
            public double SumGen;
            public int Count;
        }

        const int MaxGroups = 48;
        readonly Group[] groups = new Group[MaxGroups];
        int groupCount;
        int[] cellGroup = new int[1024];
        readonly float[] brainMean;

        public SpeciesTracker(SimParameters p, float[] brainMean)
        {
            this.p = p;
            this.brainMean = brainMean;
            for (int i = 0; i < MaxGroups; i++) groups[i] = new Group();
        }

        public void Clear()
        {
            Records.Clear();
            Pending.Clear();
            NextId = 1;
        }

        /// <summary>Species 1 is made from the Founders (all Founders already carry SpeciesId 1).</summary>
        public void InitFirst(CellPool cells)
        {
            var first = new SpeciesRecord { Id = NextId++, ParentId = 0, Name = "Species 1", BirthTime = 0 };
            MeanGenome(cells, null, first.Centroid, out first.BirthGeneration);
            SetColor(first);
            first.CurrentPopulation = cells.AliveCount;
            first.PeakPopulation = cells.AliveCount;
            first.LastPopulation = cells.AliveCount;
            Records.Add(first);
        }

        public SpeciesRecord Get(int id)
        {
            for (int i = 0; i < Records.Count; i++) if (Records[i].Id == id) return Records[i];
            return null;
        }

        public int LivingCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Records.Count; i++) if (!Records[i].Extinct) n++;
                return n;
            }
        }

        public int ExtinctCount { get { return Records.Count - LivingCount; } }

        void SetColor(SpeciesRecord s)
        {
            CellColor.Compute(s.Centroid, brainMean, out s.ColorR, out s.ColorG, out s.ColorB);
        }

        SpeciesRecord NearestLiving(Genome g, out float dist)
        {
            SpeciesRecord best = null;
            dist = float.MaxValue;
            for (int i = 0; i < Records.Count; i++)
            {
                SpeciesRecord s = Records[i];
                if (s.Extinct) continue;
                float d = GeneticDistance.Distance(g, s.Centroid, p);
                if (d < dist) { dist = d; best = s; }
            }
            return best;
        }

        public void Check(CellPool cells, double time)
        {
            int n = cells.AliveCount;
            if (n == 0) { Finish(time); return; }

            if (Records.Count == 0)
            {
                // The first Species is made from the Founders.
                var first = new SpeciesRecord { Id = NextId++, ParentId = 0, Name = "Species 1", BirthTime = 0, BirthGeneration = 0 };
                MeanGenome(cells, null, first.Centroid, out first.BirthGeneration);
                SetColor(first);
                Records.Add(first);
            }

            // 2. nearest living centroid within theta
            if (cellGroup.Length < n) cellGroup = new int[n * 2];
            groupCount = 0;
            for (int i = 0; i < n; i++)
            {
                Cell c = cells.Alive[i];
                float dist;
                SpeciesRecord s = NearestLiving(c.G, out dist);
                if (s != null && dist <= p.SpeciesTheta)
                {
                    c.SpeciesId = s.Id;
                    cellGroup[i] = -1;
                }
                else cellGroup[i] = -2; // unassigned
            }

            // 3. greedy leader clustering of the unassigned Cells
            for (int i = 0; i < n; i++)
            {
                if (cellGroup[i] != -2) continue;
                Cell c = cells.Alive[i];
                int found = -1;
                float bestD = float.MaxValue;
                for (int g = 0; g < groupCount; g++)
                {
                    float d = GeneticDistance.Distance(c.G, groups[g].Leader, p);
                    if (d <= p.SpeciesTheta && d < bestD) { bestD = d; found = g; }
                }
                if (found < 0)
                {
                    if (groupCount >= MaxGroups) { cellGroup[i] = -3; continue; }
                    found = groupCount++;
                    Group ng = groups[found];
                    ng.Leader.CopyFrom(c.G);
                    Array.Clear(ng.SumT, 0, ng.SumT.Length);
                    Array.Clear(ng.SumW, 0, ng.SumW.Length);
                    ng.SumGen = 0;
                    ng.Count = 0;
                }
                Group gr = groups[found];
                for (int t = 0; t < Traits.Count; t++) gr.SumT[t] += c.G.Trait[t];
                for (int w = 0; w < BrainLayout.WeightCount; w++) gr.SumW[w] += c.G.Weights[w];
                gr.SumGen += c.Generation;
                gr.Count++;
                cellGroup[i] = found;
            }

            // 3/4. groups of at least 10 that were also seen at the previous check become new Species
            var newPending = new List<Candidate>();
            var promoted = new int[MaxGroups];
            for (int g = 0; g < groupCount; g++) promoted[g] = 0;
            var tmp = new Genome();
            for (int g = 0; g < groupCount; g++)
            {
                Group gr = groups[g];
                if (gr.Count < p.SpeciesMinGroup) continue;
                for (int t = 0; t < Traits.Count; t++) tmp.Trait[t] = (float)(gr.SumT[t] / gr.Count);
                for (int w = 0; w < BrainLayout.WeightCount; w++) tmp.Weights[w] = (float)(gr.SumW[w] / gr.Count);

                int match = -1;
                for (int k = 0; k < Pending.Count; k++)
                {
                    if (GeneticDistance.Distance(tmp, Pending[k].Centroid, p) <= p.SpeciesTheta) { match = k; break; }
                }
                if (match >= 0)
                {
                    float dist;
                    SpeciesRecord parent = NearestLiving(tmp, out dist);
                    var sp = new SpeciesRecord
                    {
                        Id = NextId++,
                        ParentId = parent != null ? parent.Id : 0,
                        BirthTime = time,
                        BirthGeneration = (int)Math.Round(gr.SumGen / gr.Count)
                    };
                    sp.Name = "Species " + sp.Id;
                    sp.Centroid.CopyFrom(tmp);
                    SetColor(sp);
                    Records.Add(sp);
                    promoted[g] = sp.Id;
                    Pending.RemoveAt(match);
                }
                else
                {
                    var cand = new Candidate { Size = gr.Count };
                    cand.Centroid.CopyFrom(tmp);
                    newPending.Add(cand);
                }
            }
            Pending = newPending;

            // Assign the grouped Cells
            for (int i = 0; i < n; i++)
            {
                int g = cellGroup[i];
                if (g >= 0 && promoted[g] != 0) cells.Alive[i].SpeciesId = promoted[g];
                else if (g < -1 || g >= 0)
                {
                    // 5. leftovers: nearest living Species, for display only
                    float dist;
                    SpeciesRecord s = NearestLiving(cells.Alive[i].G, out dist);
                    if (s != null) cells.Alive[i].SpeciesId = s.Id;
                }
            }

            // 6. update centroids, populations; 7. extinction
            for (int r = 0; r < Records.Count; r++)
            {
                SpeciesRecord s = Records[r];
                if (s.Extinct) continue;
                s.CurrentPopulation = 0;
            }
            for (int i = 0; i < n; i++)
            {
                SpeciesRecord s = Get(cells.Alive[i].SpeciesId);
                if (s != null) s.CurrentPopulation++;
            }
            for (int r = 0; r < Records.Count; r++)
            {
                SpeciesRecord s = Records[r];
                if (s.Extinct) continue;
                if (s.CurrentPopulation > s.PeakPopulation) s.PeakPopulation = s.CurrentPopulation;
                if (s.CurrentPopulation == 0)
                {
                    s.EmptyChecks++;
                    if (s.EmptyChecks >= 2) s.ExtinctTime = time;
                    continue;
                }
                s.EmptyChecks = 0;
                s.LastPopulation = s.CurrentPopulation;
                UpdateCentroid(cells, s);
            }
        }

        /// <summary>No future check is possible after terminal extinction.</summary>
        public void Finish(double time)
        {
            foreach (SpeciesRecord s in Records)
            {
                if (s.Extinct) continue;
                if (s.CurrentPopulation > 0) s.LastPopulation = s.CurrentPopulation;
                s.CurrentPopulation = 0;
                s.EmptyChecks = 2;
                s.ExtinctTime = time;
            }
            Pending.Clear();
        }

        void UpdateCentroid(CellPool cells, SpeciesRecord s)
        {
            var mean = new Genome();
            int gen;
            int cnt = MeanGenome(cells, s, mean, out gen);
            if (cnt == 0) return;
            float keep = p.CentroidKeep;
            for (int t = 0; t < Traits.Count; t++) s.Centroid.Trait[t] = keep * s.Centroid.Trait[t] + (1f - keep) * mean.Trait[t];
            for (int w = 0; w < BrainLayout.WeightCount; w++) s.Centroid.Weights[w] = keep * s.Centroid.Weights[w] + (1f - keep) * mean.Weights[w];
            SetColor(s);
        }

        /// <summary>Mean Genome of the living Cells of a Species (all Cells when s is null).</summary>
        static int MeanGenome(CellPool cells, SpeciesRecord s, Genome dest, out int meanGeneration)
        {
            var st = new double[Traits.Count];
            var sw = new double[BrainLayout.WeightCount];
            double sg = 0;
            int cnt = 0;
            for (int i = 0; i < cells.AliveCount; i++)
            {
                Cell c = cells.Alive[i];
                if (s != null && c.SpeciesId != s.Id) continue;
                for (int t = 0; t < Traits.Count; t++) st[t] += c.G.Trait[t];
                for (int w = 0; w < BrainLayout.WeightCount; w++) sw[w] += c.G.Weights[w];
                sg += c.Generation;
                cnt++;
            }
            meanGeneration = 0;
            if (cnt == 0) return 0;
            for (int t = 0; t < Traits.Count; t++) dest.Trait[t] = (float)(st[t] / cnt);
            for (int w = 0; w < BrainLayout.WeightCount; w++) dest.Weights[w] = (float)(sw[w] / cnt);
            meanGeneration = (int)Math.Round(sg / cnt);
            return cnt;
        }
    }
}
