using System;

namespace Evo.Core
{
    /// <summary>
    /// The whole model: World, Food, Toxin, Cells, Events, Species and statistics.
    /// Deterministic: the same Seed, settings and sequence of Player inputs give the same state.
    /// No UnityEngine, DateTime or System.Random anywhere in Evo.Core.
    /// </summary>
    public sealed partial class Simulation
    {
        public readonly SimParameters P;
        public readonly SetupSettings Setup;
        public readonly SeededRandom WorldRng;
        public readonly SeededRandom SimRng;
        public readonly EventSystem Events = new EventSystem();
        public readonly World World;
        public readonly CellPool Cells;
        public readonly float[] BrainMean = new float[BrainLayout.WeightCount];
        public readonly SpeciesTracker Species;
        public readonly StatsRecorder Stats;

        public long StepCount;
        public int NextCellId;
        public bool Extinct;
        public double Time { get { return StepCount * (double)P.Dt; } }

        // Food (static pellets)
        public float[] FoodX, FoodY;
        public int FoodCount;
        bool[] foodEaten;
        int eatenCount;
        float foodAcc;

        // Toxin particles
        public float[] ToxX, ToxY, ToxVX, ToxVY, ToxS, ToxHalf;
        public int ToxCount;

        readonly SpatialHash cellHash, foodHash, toxHash;
        readonly float[] cellX, cellY;
        readonly int[] qbuf = new int[2048];
        readonly Brain brain = new Brain();
        readonly Cell[] newborns;
        int newCount;
        int speciesCheckSteps, secondSteps;

        public Simulation(SetupSettings setup, SimParameters p)
        {
            P = p;
            Setup = setup.Clone();
            uint seed = SeedUtil.FromText(Setup.SeedText);
            WorldRng = new SeededRandom(seed);
            SimRng = new SeededRandom(seed ^ 0xA5C3_9E77u);
            World = new World(P, Setup, WorldRng, Events);
            Cells = new CellPool(P.PoolCapacity);
            BrainLayout.InnateWiring(P.InnateWiring, BrainMean);
            Species = new SpeciesTracker(P, BrainMean);
            Stats = new StatsRecorder(P);

            FoodX = new float[P.MaxFood]; FoodY = new float[P.MaxFood]; foodEaten = new bool[P.MaxFood];
            ToxX = new float[P.MaxToxin]; ToxY = new float[P.MaxToxin];
            ToxVX = new float[P.MaxToxin]; ToxVY = new float[P.MaxToxin];
            ToxS = new float[P.MaxToxin]; ToxHalf = new float[P.MaxToxin];

            cellX = new float[P.PoolCapacity]; cellY = new float[P.PoolCapacity];
            cellHash = new SpatialHash(P.WorldWidth, P.WorldHeight, 5f, P.PoolCapacity);
            foodHash = new SpatialHash(P.WorldWidth, P.WorldHeight, 5f, P.MaxFood);
            toxHash = new SpatialHash(P.WorldWidth, P.WorldHeight, 5f, P.MaxToxin);
            newborns = new Cell[P.PoolCapacity];
            speciesCheckSteps = Math.Max(1, (int)Math.Round(P.SpeciesCheckSeconds / P.Dt));
            secondSteps = Math.Max(1, (int)Math.Round(1f / P.Dt));
        }

        /// <summary>Creates a new Run: initial Food, Toxin clouds and Founders, all from the world stream.</summary>
        public static Simulation Create(SetupSettings setup, SimParameters p)
        {
            var s = new Simulation(setup, p);
            s.InitNewWorld();
            return s;
        }

        void InitNewWorld()
        {
            for (int i = 0; i < P.InitialFood; i++) SpawnFood(WorldRng, false);
            InitToxin();
            for (int i = 0; i < P.FounderCount; i++)
            {
                Cell c = Cells.Acquire();
                c.ResetState();
                c.Id = NextCellId++;
                c.ParentId = -1;
                c.Generation = 0;
                c.SpeciesId = 1;
                Genome.MakeFounder(c.G, WorldRng, P, BrainMean);
                c.Refresh(P);
                c.X = WorldRng.NextRange(1f, World.Width - 1f);
                c.Y = WorldRng.NextRange(1f, World.Height - 1f);
                c.Heading = WorldRng.NextRange(-3.14159265f, 3.14159265f);
                c.Energy = P.FounderStartEnergy * c.Emax;
                UpdateColor(c);
                Cells.AddAlive(c);
            }
            Species.InitFirst(Cells);
            Stats.Sample(Cells, Species, 0);
        }

        void InitToxin()
        {
            float level = Setup.ToxinLevel;
            if (level <= 0f) return;
            int clouds = 3 + (int)Math.Round(level * 5f);
            int total = (int)Math.Round(600f * level);
            int per = total / clouds;
            for (int c = 0; c < clouds; c++)
            {
                float cx = WorldRng.NextRange(10f, World.Width - 10f);
                float cy = WorldRng.NextRange(10f, World.Height - 10f);
                float cr = WorldRng.NextRange(6f, 14f);
                for (int k = 0; k < per && ToxCount < P.MaxToxin; k++)
                {
                    float x = Clamp(cx + WorldRng.NextGaussian() * cr * 0.5f, 0f, World.Width);
                    float y = Clamp(cy + WorldRng.NextGaussian() * cr * 0.5f, 0f, World.Height);
                    AddToxin(x, y, P.ToxinHalfLifeCloud);
                }
            }
        }

        static float Clamp(float v, float lo, float hi) { return v < lo ? lo : (v > hi ? hi : v); }

        void AddToxin(float x, float y, float halfLife)
        {
            if (ToxCount >= P.MaxToxin) return;
            int i = ToxCount++;
            ToxX[i] = x; ToxY[i] = y; ToxVX[i] = 0f; ToxVY[i] = 0f; ToxS[i] = 1f; ToxHalf[i] = halfLife;
        }

        void UpdateColor(Cell c)
        {
            CellColor.Compute(c.G, BrainMean, out c.ColorR, out c.ColorG, out c.ColorB);
        }

        // ---------------------------------------------------------------- Food

        /// <summary>Rejection sampling by richness. Returns false when a Famine rejected the final candidate.</summary>
        bool SpawnFood(SeededRandom rng, bool applyFamine)
        {
            if (FoodCount >= P.MaxFood) return false;
            float x = 0, y = 0;
            for (int t = 0; t < P.FoodSpawnTries; t++)
            {
                x = rng.NextFloat01() * World.Width;
                y = rng.NextFloat01() * World.Height;
                if (rng.NextFloat01() < World.FoodRichnessAt(x, y)) break;
            }
            // A Famine discards the finished pellet, so its area gets (1 - share * weight) of the normal rate.
            if (applyFamine)
            {
                for (int e = 0; e < Events.ActiveCount; e++)
                {
                    ActiveEvent ev = Events.Active[e];
                    if (ev.Spec.Kind != EventKind.Famine) continue;
                    float w = ev.Weight(x, y);
                    if (w > 0f && rng.NextFloat01() < P.FamineRejectShare * w) return false;
                }
            }
            AddFood(x, y);
            return true;
        }

        void AddFood(float x, float y)
        {
            if (FoodCount >= P.MaxFood) return;
            FoodX[FoodCount] = x; FoodY[FoodCount] = y; foodEaten[FoodCount] = false;
            FoodCount++;
        }

        void SpawnFoodStreams(float dt)
        {
            foodAcc += P.FoodRate * Setup.FoodAbundance * dt;
            while (foodAcc >= 1f)
            {
                foodAcc -= 1f;
                if (FoodCount >= P.MaxFood) { foodAcc = 0f; break; }
                SpawnFood(SimRng, true);
            }
            for (int e = 0; e < Events.ActiveCount; e++)
            {
                ActiveEvent ev = Events.Active[e];
                if (ev.Spec.Kind != EventKind.FoodBloom) continue;
                ev.Accumulator += P.BloomRate * dt;
                while (ev.Accumulator >= 1f)
                {
                    ev.Accumulator -= 1f;
                    if (FoodCount >= P.MaxFood) { ev.Accumulator = 0f; break; }
                    float x, y;
                    RandomPointInEvent(ev.Spec, SimRng, out x, out y);
                    AddFood(x, y);
                }
            }
        }

        void RandomPointInEvent(EventSpec s, SeededRandom rng, out float x, out float y)
        {
            if (s.WholeWorld)
            {
                x = rng.NextFloat01() * World.Width;
                y = rng.NextFloat01() * World.Height;
                return;
            }
            float r = s.Radius * (float)Math.Sqrt(rng.NextFloat01());
            float a = rng.NextFloat01() * 6.2831853f;
            x = Clamp(s.X + r * (float)Math.Cos(a), 0f, World.Width);
            y = Clamp(s.Y + r * (float)Math.Sin(a), 0f, World.Height);
        }

        void CompactFood()
        {
            if (eatenCount == 0) return;
            int w = 0;
            for (int i = 0; i < FoodCount; i++)
            {
                if (foodEaten[i]) continue;
                FoodX[w] = FoodX[i]; FoodY[w] = FoodY[i];
                w++;
            }
            for (int i = 0; i < FoodCount; i++) foodEaten[i] = false;
            FoodCount = w;
            eatenCount = 0;
        }

        // --------------------------------------------------------------- Events

        public bool CanPlaceEvent(EventKind kind)
        {
            return kind == EventKind.ToxinSpill || Events.CanAdd(P);
        }

        /// <summary>Applies an Event at the current time. Returns false if too many Events are active.</summary>
        public bool PlaceEvent(EventSpec spec)
        {
            if (Extinct) return false;
            if (!CanPlaceEvent(spec.Kind)) return false;
            var copy = new EventSpec
            {
                Kind = spec.Kind, X = spec.X, Y = spec.Y, Radius = spec.Radius,
                Intensity = spec.Intensity, Duration = spec.Duration, WholeWorld = spec.WholeWorld
            };
            if (copy.Kind == EventKind.ToxinSpill)
            {
                int count = (int)Math.Round(P.SpillParticlesPer10Radius * copy.Radius / 10f);
                for (int i = 0; i < count && ToxCount < P.MaxToxin; i++)
                {
                    float x, y;
                    RandomPointInEvent(copy, SimRng, out x, out y);
                    AddToxin(x, y, P.ToxinHalfLifeSpill);
                }
            }
            else
            {
                var ev = new ActiveEvent { Spec = copy, Age = 0f, Accumulator = 0f, PlacedTime = Time };
                if (copy.Kind == EventKind.Famine)
                {
                    for (int i = FoodCount - 1; i >= 0; i--)
                    {
                        if (ev.Weight(FoodX[i], FoodY[i]) <= 0f) continue;
                        if (SimRng.NextFloat01() < P.FamineRemoveShare)
                        {
                            FoodCount--;
                            FoodX[i] = FoodX[FoodCount]; FoodY[i] = FoodY[FoodCount];
                        }
                    }
                }
                Events.Add(ev);
            }
            Events.Log.Add(new EventRecord
            {
                Time = Time, Kind = copy.Kind, X = copy.X, Y = copy.Y, Radius = copy.Radius,
                Intensity = copy.Intensity, Duration = copy.Duration, WholeWorld = copy.WholeWorld
            });
            return true;
        }

        // --------------------------------------------------------------- Toxin

        /// <summary>Runs only the Toxin physics for one fixed step (used by tests).</summary>
        public void StepToxinForTest() => StepToxin(P.Dt);

        void StepToxin(float dt)
        {
            int n = ToxCount;
            if (n == 0) return;
            toxHash.Build(ToxX, ToxY, n);
            float damp = (float)Math.Exp(-P.ToxinDamping * dt);
            float drift = P.ToxinDrift * (float)Math.Sqrt(dt);
            float rc = P.ToxinRc;
            for (int i = 0; i < n; i++)
            {
                float vx = ToxVX[i] * damp + SimRng.NextGaussian() * drift;
                float vy = ToxVY[i] * damp + SimRng.NextGaussian() * drift;
                float x = ToxX[i], y = ToxY[i];
                float ax = 0f, ay = 0f;
                int k = toxHash.Query(x, y, rc, qbuf);
                for (int q = 0; q < k; q++)
                {
                    int j = qbuf[q];
                    if (j == i) continue;
                    float dx = ToxX[j] - x, dy = ToxY[j] - y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float f = P.ToxinG / ((d + 0.5f) * (d + 0.5f));
                    ax += f * dx;
                    ay += f * dy;
                }
                float am = (float)Math.Sqrt(ax * ax + ay * ay);
                if (am > P.ToxinAccelCap) { float s = P.ToxinAccelCap / am; ax *= s; ay *= s; }
                vx += ax * dt;
                vy += ay * dt;
                ToxVX[i] = vx; ToxVY[i] = vy;
            }
            // Positions update after all velocities so that attraction used a consistent snapshot.
            for (int i = 0; i < n; i++)
            {
                float x = ToxX[i] + ToxVX[i] * dt;
                float y = ToxY[i] + ToxVY[i] * dt;
                if (x < 0f) { x = -x; ToxVX[i] = -ToxVX[i]; }
                else if (x > World.Width) { x = 2f * World.Width - x; ToxVX[i] = -ToxVX[i]; }
                if (y < 0f) { y = -y; ToxVY[i] = -ToxVY[i]; }
                else if (y > World.Height) { y = 2f * World.Height - y; ToxVY[i] = -ToxVY[i]; }
                ToxX[i] = Clamp(x, 0f, World.Width);
                ToxY[i] = Clamp(y, 0f, World.Height);
                ToxS[i] *= (float)Math.Pow(0.5, dt / ToxHalf[i]);
            }
            int w = 0;
            for (int i = 0; i < n; i++)
            {
                if (ToxS[i] < P.ToxinMinStrength) continue;
                if (w != i)
                {
                    ToxX[w] = ToxX[i]; ToxY[w] = ToxY[i]; ToxVX[w] = ToxVX[i]; ToxVY[w] = ToxVY[i];
                    ToxS[w] = ToxS[i]; ToxHalf[w] = ToxHalf[i];
                }
                w++;
            }
            ToxCount = w;
        }

        // ---------------------------------------------------------------- Step

        public void Step()
        {
            if (Extinct) return;
            float dt = P.Dt;

            Events.Update(dt, P);
            SpawnFoodStreams(dt);
            StepToxin(dt);

            int n = Cells.AliveCount;
            for (int i = 0; i < n; i++) { cellX[i] = Cells.Alive[i].X; cellY[i] = Cells.Alive[i].Y; }
            cellHash.Build(cellX, cellY, n);
            foodHash.Build(FoodX, FoodY, FoodCount);
            toxHash.Build(ToxX, ToxY, ToxCount);

            newCount = 0;
            for (int i = 0; i < n; i++)
            {
                Cell c = Cells.Alive[i];
                if (c.Alive) StepCell(c, dt);
            }

            Cells.CompactDead();
            for (int i = 0; i < newCount; i++) { Cells.AddAlive(newborns[i]); newborns[i] = null; }
            newCount = 0;
            CompactFood();

            StepCount++;
            if (StepCount % secondSteps == 0) Stats.RollSecond();
            if (Cells.AliveCount == 0)
            {
                Extinct = true;
                Species.Finish(Time);
                Stats.Sample(Cells, Species, Time);
            }
            else
            {
                if (StepCount % speciesCheckSteps == 0) Species.Check(Cells, Time);
                if (StepCount % Stats.IntervalSteps == 0) Stats.Sample(Cells, Species, Time);
            }
        }

        void Kill(Cell c, DeathCause cause)
        {
            c.Alive = false;
            c.Cause = cause;
            Stats.RecordDeath(cause, Time);
        }

        static float SinTo(Cell c, float tx, float ty)
        {
            return (float)Math.Sin(Math.Atan2(ty - c.Y, tx - c.X) - c.Heading);
        }

        void StepCell(Cell c, float dt)
        {
            float[] T = c.G.Trait;
            float sense = T[TraitId.Sense];
            float[] input = brain.Inputs;

            // ---- sense
            int fi = -1; float fd = float.MaxValue;
            int k = foodHash.Query(c.X, c.Y, sense, qbuf);
            for (int q = 0; q < k; q++)
            {
                int j = qbuf[q];
                if (foodEaten[j]) continue;
                float dx = FoodX[j] - c.X, dy = FoodY[j] - c.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < fd) { fd = d2; fi = j; }
            }

            Cell prey = null, threat = null;
            float pd = float.MaxValue, td = float.MaxValue;
            k = cellHash.Query(c.X, c.Y, sense, qbuf);
            for (int q = 0; q < k; q++)
            {
                Cell o = Cells.Alive[qbuf[q]];
                if (o == c || !o.Alive) continue;
                float dx = o.X - c.X, dy = o.Y - c.Y;
                float d2 = dx * dx + dy * dy;
                if (o.Radius * P.PredationRatio <= c.Radius) { if (d2 < pd) { pd = d2; prey = o; } }
                else if (o.Radius >= P.PredationRatio * c.Radius) { if (d2 < td) { td = d2; threat = o; } }
            }

            int ti = -1; float tdist = float.MaxValue;
            k = toxHash.Query(c.X, c.Y, sense, qbuf);
            for (int q = 0; q < k; q++)
            {
                int j = qbuf[q];
                if (ToxS[j] < P.ToxinSenseStrength) continue;
                float dx = ToxX[j] - c.X, dy = ToxY[j] - c.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < tdist) { tdist = d2; ti = j; }
            }

            float temp = World.TemperatureAt(c.X, c.Y);
            input[BrainLayout.InFoodDir] = fi >= 0 ? SinTo(c, FoodX[fi], FoodY[fi]) : 0f;
            input[BrainLayout.InFoodProx] = fi >= 0 ? 1f - (float)Math.Sqrt(fd) / sense : 0f;
            input[BrainLayout.InPreyDir] = prey != null ? SinTo(c, prey.X, prey.Y) : 0f;
            input[BrainLayout.InPreyProx] = prey != null ? 1f - (float)Math.Sqrt(pd) / sense : 0f;
            input[BrainLayout.InThreatDir] = threat != null ? SinTo(c, threat.X, threat.Y) : 0f;
            input[BrainLayout.InThreatProx] = threat != null ? 1f - (float)Math.Sqrt(td) / sense : 0f;
            input[BrainLayout.InToxinDir] = ti >= 0 ? SinTo(c, ToxX[ti], ToxY[ti]) : 0f;
            input[BrainLayout.InToxinProx] = ti >= 0 ? 1f - (float)Math.Sqrt(tdist) / sense : 0f;
            input[BrainLayout.InTempMismatch] = Clamp(temp - T[TraitId.PreferredTemperature], -1f, 1f);
            input[BrainLayout.InEnergy] = c.Energy / c.Emax;
            input[BrainLayout.InBias] = 1f;

            // ---- think
            brain.Forward(c.G.Weights);
            float[] o4 = brain.Outputs;
            c.OutTurn = o4[BrainLayout.OutTurn];
            c.OutThrust = (o4[BrainLayout.OutThrust] + 1f) * 0.5f;
            c.OutBite = o4[BrainLayout.OutBite];
            c.OutMate = o4[BrainLayout.OutMate];

            // ---- move
            c.Heading += c.OutTurn * P.TurnRate * dt;
            if (c.Heading > 3.14159265f) c.Heading -= 6.2831853f;
            else if (c.Heading < -3.14159265f) c.Heading += 6.2831853f;
            float speed = c.OutThrust * T[TraitId.Speed];
            c.X = Clamp(c.X + (float)Math.Cos(c.Heading) * speed * dt, 0f, World.Width);
            c.Y = Clamp(c.Y + (float)Math.Sin(c.Heading) * speed * dt, 0f, World.Height);

            // ---- pay costs
            float eBase = c.BaseCost + P.MoveCost * speed * speed * (float)Math.Sqrt(T[TraitId.Size]);
            float eAge = c.Age > P.AgeStart ? P.AgeCost * (c.Age - P.AgeStart) : 0f;
            float stress = Math.Abs(temp - T[TraitId.PreferredTemperature]) - T[TraitId.TemperatureTolerance];
            float eTemp = stress > 0f ? P.TempStressCost * stress : 0f;

            float rt = 2.5f + c.Radius;
            float sumTox = 0f;
            k = toxHash.Query(c.X, c.Y, rt, qbuf);
            for (int q = 0; q < k; q++)
            {
                int j = qbuf[q];
                float dx = ToxX[j] - c.X, dy = ToxY[j] - c.Y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d < rt) sumTox += ToxS[j] * (1f - d / rt);
            }
            float exposure = Math.Min(1f, P.ToxinExposure * sumTox);
            float eTox = P.ToxinCost * exposure * (1f - 0.9f * T[TraitId.ToxinResistance]);

            c.Energy -= (eBase + eAge + eTemp + eTox) * dt;
            c.Age += dt;
            float a = dt / P.DrainTimeConstant;
            c.DrainBase += (eBase - c.DrainBase) * a;
            c.DrainTemperature += (eTemp - c.DrainTemperature) * a;
            c.DrainToxin += (eTox - c.DrainToxin) * a;
            c.DrainAge += (eAge - c.DrainAge) * a;

            // ---- eat Food
            k = foodHash.Query(c.X, c.Y, c.Radius + P.FoodRadius, qbuf);
            if (k > 0)
            {
                float gain = P.FoodEnergy * (1f - P.DietFoodPenalty * T[TraitId.Diet]);
                for (int q = 0; q < k; q++)
                {
                    int j = qbuf[q];
                    if (foodEaten[j]) continue;
                    foodEaten[j] = true;
                    eatenCount++;
                    c.Energy = Math.Min(c.Emax, c.Energy + gain);
                }
            }

            // ---- bite
            if (c.OutBite > P.BiteOutputThreshold && T[TraitId.Diet] >= P.PredationMinDiet && prey != null && prey.Alive)
            {
                float dx = prey.X - c.X, dy = prey.Y - c.Y;
                float reach = c.Radius + prey.Radius + P.BiteReach;
                if (dx * dx + dy * dy < reach * reach)
                {
                    if (c.BiteTargetId == prey.Id) c.BiteTimer += dt;
                    else { c.BiteTargetId = prey.Id; c.BiteTimer = dt; }
                    if (c.BiteTimer >= P.BiteTime)
                    {
                        float diet = T[TraitId.Diet];
                        c.Energy = Math.Min(c.Emax, c.Energy + diet * (P.PredatorEnergyShare * prey.Energy + P.PredatorMaxShare * prey.Emax));
                        Kill(prey, DeathCause.Eaten);
                        c.BiteTimer = 0f;
                        c.BiteTargetId = -1;
                    }
                }
                else { c.BiteTimer = 0f; c.BiteTargetId = -1; }
            }
            else { c.BiteTimer = 0f; c.BiteTargetId = -1; }

            // ---- death
            if (c.Energy <= 0f)
            {
                c.Energy = 0f;
                DeathCause cause = DeathCause.Starved;
                float best = c.DrainBase;
                if (c.DrainTemperature > best) { best = c.DrainTemperature; cause = DeathCause.Temperature; }
                if (c.DrainToxin > best) { best = c.DrainToxin; cause = DeathCause.Poisoned; }
                if (c.DrainAge > best) { cause = DeathCause.OldAge; }
                Kill(c, cause);
                return;
            }

            // ---- reproduction
            if (c.Energy >= T[TraitId.ReproductionThreshold] * c.Emax && Cells.AliveCount + newCount < P.MaxCells)
            {
                if (T[TraitId.SexualTendency] >= P.SexThreshold) TryMate(c, sense);
                else Divide(c);
            }
        }

        // -------------------------------------------------------- Reproduction

        void Divide(Cell parent)
        {
            float childEnergy = P.ChildShare * parent.Energy;
            Cell child = Birth(parent, null, childEnergy);
            if (child == null) return;
            parent.Energy *= P.ParentKeep;
        }

        float MaxMateDistance(Cell c)
        {
            float dmax = P.MateDistMaxFactor * P.SpeciesTheta;
            float dmin = P.MateDistMinFactor * P.SpeciesTheta;
            float ch = c.G.Trait[TraitId.Choosiness];
            return dmax + (dmin - dmax) * ch;
        }

        void TryMate(Cell c, float sense)
        {
            float myMax = MaxMateDistance(c);
            Cell best = null;
            float bestD2 = float.MaxValue;
            int k = cellHash.Query(c.X, c.Y, sense, qbuf);
            for (int q = 0; q < k; q++)
            {
                Cell o = Cells.Alive[qbuf[q]];
                if (o == c || !o.Alive || o.MatedStep == StepCount) continue;
                if (o.G.Trait[TraitId.SexualTendency] < P.SexThreshold) continue;
                if (o.Energy < P.MateEnergyFraction * o.G.Trait[TraitId.ReproductionThreshold] * o.Emax) continue;
                float dx = o.X - c.X, dy = o.Y - c.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 >= bestD2) continue;
                float gd = GeneticDistance.Distance(c.G, o.G, P);
                if (gd > myMax || gd > MaxMateDistance(o)) continue;
                best = o;
                bestD2 = d2;
            }
            if (best == null) return;
            float payA = P.MateCost * c.Energy;
            float payB = P.MateCost * best.Energy;
            Cell child = Birth(c, best, P.MateChildShare * (payA + payB));
            if (child == null) return;
            c.Energy -= payA;
            best.Energy -= payB;
            c.MatedStep = StepCount;
            best.MatedStep = StepCount;
        }

        Cell Birth(Cell a, Cell b, float childEnergy)
        {
            Cell child = Cells.Acquire();
            if (child == null) return null;
            child.ResetState();
            child.Id = NextCellId++;
            child.ParentId = a.Id;
            child.Generation = (b == null ? a.Generation : Math.Max(a.Generation, b.Generation)) + 1;
            child.SpeciesId = a.SpeciesId;
            if (b == null) child.G.CopyFrom(a.G);
            else Genome.Crossover(a.G, b.G, child.G, SimRng);
            Genome.Mutate(child.G, SimRng, P, Setup.MutationRate);
            child.Refresh(P);
            UpdateColor(child);
            float ang = SimRng.NextFloat01() * 6.2831853f;
            child.X = Clamp(a.X + (float)Math.Cos(ang) * a.Radius, 0f, World.Width);
            child.Y = Clamp(a.Y + (float)Math.Sin(ang) * a.Radius, 0f, World.Height);
            child.Heading = a.Heading;
            child.Energy = Math.Min(childEnergy, child.Emax);
            newborns[newCount++] = child;
            Stats.RecordBirth(child.Generation);
            return child;
        }

        // ------------------------------------------------------------- Queries

        public Cell FindCell(int id) { return Cells.FindById(id); }

        /// <summary>The nearest living Cell to a point, within max(radius, minReach), or null.</summary>
        public Cell PickCell(float x, float y, float minReach)
        {
            Cell best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < Cells.AliveCount; i++)
            {
                Cell c = Cells.Alive[i];
                float dx = c.X - x, dy = c.Y - y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                float reach = Math.Max(c.Radius, minReach);
                if (d <= reach && d < bestD) { bestD = d; best = c; }
            }
            return best;
        }
    }
}
