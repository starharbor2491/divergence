using System;

namespace Evo.Core
{
    /// <summary>Order is the stacking order of the Deaths chart (DESIGN.md 2.2), bottom to top.</summary>
    public enum DeathCause
    {
        Eaten = 0,
        Temperature = 1,
        OldAge = 2,
        Starved = 3,
        Poisoned = 4
    }

    public static class DeathCauses
    {
        public const int Count = 5;
        public static readonly string[] Names = { "eaten", "temperature", "old age", "starved", "poisoned" };
    }

    /// <summary>One simulated Cell. Instances are pooled and reused; arrays are allocated once.</summary>
    public sealed class Cell
    {
        public int Id;
        public int ParentId;
        public int Generation;
        public int SpeciesId;
        public bool Alive;
        public DeathCause Cause;

        public float X, Y, Heading;
        public float Energy;
        public float Age;

        // Derived from the Genome by Refresh()
        public float Radius;
        public float Emax;
        public float BaseCost;
        public float ColorR, ColorG, ColorB;

        public readonly Genome G = new Genome();

        // Rolling drain averages (energy per second) used to name the cause of death
        public float DrainBase, DrainTemperature, DrainToxin, DrainAge;

        public float BiteTimer;
        public int BiteTargetId;
        public long MatedStep;

        // Latest Brain outputs (for the inspector)
        public float OutTurn, OutThrust, OutBite, OutMate;

        public int PoolIndex;

        public void Refresh(SimParameters p)
        {
            float size = G.Trait[TraitId.Size];
            Radius = 0.5f * size;
            Emax = p.MaxEnergyPerSize * size;
            BaseCost = p.BaseSize * (float)Math.Pow(size, 0.75)
                       + p.BaseSense * G.Trait[TraitId.Sense]
                       + p.BaseResist * G.Trait[TraitId.ToxinResistance]
                       + p.BaseTolerance * G.Trait[TraitId.TemperatureTolerance];
        }

        public void ResetState()
        {
            Alive = true;
            Cause = DeathCause.Starved;
            Age = 0f;
            DrainBase = DrainTemperature = DrainToxin = DrainAge = 0f;
            BiteTimer = 0f;
            BiteTargetId = -1;
            MatedStep = -1;
            OutTurn = OutThrust = OutBite = OutMate = 0f;
        }
    }

    /// <summary>Preallocated Cells with a free list and a dense, ordered alive list.</summary>
    public sealed class CellPool
    {
        public readonly Cell[] All;
        readonly int[] free;
        int freeCount;
        public Cell[] Alive;
        public int AliveCount;

        public CellPool(int capacity)
        {
            All = new Cell[capacity];
            free = new int[capacity];
            Alive = new Cell[capacity];
            for (int i = 0; i < capacity; i++)
            {
                All[i] = new Cell { PoolIndex = i };
                free[i] = capacity - 1 - i;
            }
            freeCount = capacity;
        }

        public Cell Acquire()
        {
            if (freeCount == 0) return null;
            return All[free[--freeCount]];
        }

        public void Release(Cell c)
        {
            c.Alive = false;
            free[freeCount++] = c.PoolIndex;
        }

        public void AddAlive(Cell c)
        {
            Alive[AliveCount++] = c;
        }

        /// <summary>Removes dead Cells while keeping the order of the others. Dead Cells go back to the pool.</summary>
        public int CompactDead()
        {
            int w = 0;
            int removed = 0;
            for (int i = 0; i < AliveCount; i++)
            {
                Cell c = Alive[i];
                if (c.Alive) Alive[w++] = c;
                else
                {
                    Release(c);
                    removed++;
                }
            }
            for (int i = w; i < AliveCount; i++) Alive[i] = null;
            AliveCount = w;
            return removed;
        }

        public void Clear()
        {
            for (int i = 0; i < AliveCount; i++) Alive[i] = null;
            AliveCount = 0;
            for (int i = 0; i < All.Length; i++)
            {
                All[i].Alive = false;
                free[i] = All.Length - 1 - i;
            }
            freeCount = All.Length;
        }

        public Cell FindById(int id)
        {
            for (int i = 0; i < AliveCount; i++) if (Alive[i].Id == id) return Alive[i];
            return null;
        }
    }
}
