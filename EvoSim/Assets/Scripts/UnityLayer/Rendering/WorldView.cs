using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Draws the living tide-pool World: deep water substrate & caustics, ambient/full Temperature currents, active Event
    /// zones, drifting Toxin spores & haze, bioluminescent Food phytoplankton, multi-layer organic Cells (aura, flagellum,
    /// translucent membrane, diet-coded organelle, eyes), birth ripples, Sense sonar ring, and Event placement preview.
    /// Rebuilt each frame from Simulation state; zero simulation state lives here.
    /// </summary>
    public sealed class WorldView
    {
        const float AmbientTintOpacity = 0.12f;
        const float FullTintOpacity = 0.38f;
        const float TintRefreshSeconds = 0.4f;
        const float CellFadeAlpha = 0.24f;
        const int TintW = 160, TintH = 90;
        const int MaxRipples = 64;

        struct Ripple
        {
            public float X, Y, Age, Duration, MaxRadius;
            public Color Color;
        }

        readonly GameObject root;
        readonly Material whiteMaterial, circleMaterial, glowMaterial, cellMaterial, tintMaterial;
        readonly Texture2D whiteTexture, circleTexture, glowTexture, cellTexture, tintTexture;
        readonly QuadBatch background, tint, ambientGlow, toxin, food, cellGlow, cells, overlay;
        readonly Color32[] tintPixels = new Color32[TintW * TintH];
        readonly Ripple[] ripples = new Ripple[MaxRipples];
        int rippleHead;

        Simulation sim;
        bool tintDirty = true;
        bool hadTemperatureEvent;
        float sinceTint;
        float animTime;
        int lastMaxCellId = -1;
        double lastSimTime = -1;

        public bool TintOn;
        public EventKind PreviewKind;
        public const int NoCell = -1;
        public int SelectedCellId = NoCell;
        public int HighlightSpeciesId;
        public bool PreviewActive;
        public Vector2 PreviewCentre;
        public float PreviewRadius;

        public WorldView(Transform parent, RenderAssets assets)
        {
            root = new GameObject("World");
            root.transform.SetParent(parent, false);

            Shader shader = assets != null ? assets.UnlitVertexColorTexture : null;
            if (shader == null) shader = Shader.Find("Evo/UnlitVertexColorTexture");

            whiteTexture = MakeTexture(4, 4, FilterMode.Point);
            var white = new Color32[16];
            for (int i = 0; i < 16; i++) white[i] = new Color32(255, 255, 255, 255);
            whiteTexture.SetPixels32(white);
            whiteTexture.Apply(false, true);

            circleTexture = MakeCircleTexture(64);
            glowTexture = MakeGlowTexture(64);
            cellTexture = MakeCellMembraneTexture(128);
            tintTexture = MakeTexture(TintW, TintH, FilterMode.Bilinear);

            whiteMaterial = MakeMaterial(shader, whiteTexture);
            circleMaterial = MakeMaterial(shader, circleTexture);
            glowMaterial = MakeMaterial(shader, glowTexture);
            cellMaterial = MakeMaterial(shader, cellTexture);
            tintMaterial = MakeMaterial(shader, tintTexture);

            background = new QuadBatch("Background", root.transform, whiteMaterial, 8, 0);
            tint = new QuadBatch("Temperature tint", root.transform, tintMaterial, 1, 1);
            ambientGlow = new QuadBatch("Ambient & Events", root.transform, glowMaterial, 3200, 2);
            toxin = new QuadBatch("Toxin", root.transform, circleMaterial, 1600, 3);
            food = new QuadBatch("Food", root.transform, circleMaterial, 1600, 4);
            cellGlow = new QuadBatch("Cell membranes", root.transform, cellMaterial, 800, 5);
            cells = new QuadBatch("Cell details", root.transform, circleMaterial, 700 * 8, 6);
            overlay = new QuadBatch("Overlay", root.transform, circleMaterial, 1200, 7);
        }

        static Material MakeMaterial(Shader shader, Texture2D texture)
        {
            var m = new Material(shader);
            m.mainTexture = texture;
            return m;
        }

        static Texture2D MakeTexture(int w, int h, FilterMode filter)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            t.filterMode = filter;
            t.wrapMode = TextureWrapMode.Clamp;
            return t;
        }

        static Texture2D MakeCircleTexture(int size)
        {
            Texture2D t = MakeTexture(size, size, FilterMode.Bilinear);
            var px = new Color32[size * size];
            float c = (size - 1) * 0.5f, radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - d) * 255f);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        static Texture2D MakeGlowTexture(int size)
        {
            Texture2D t = MakeTexture(size, size, FilterMode.Bilinear);
            var px = new Color32[size * size];
            float c = (size - 1) * 0.5f, invR = 1f / (size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = Mathf.Clamp01(Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) * invR);
                    float falloff = (1f - r) * (1f - r) * (1f - 0.35f * r);
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(falloff) * 255f);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        /// <summary>Organic phase-contrast cell sprite: translucent jelly cytoplasm + bright lipid membrane rim + subtle 3D highlight.</summary>
        static Texture2D MakeCellMembraneTexture(int size)
        {
            Texture2D t = MakeTexture(size, size, FilterMode.Bilinear);
            var px = new Color32[size * size];
            float c = (size - 1) * 0.5f, invR = 1f / (size * 0.5f - 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - c) * invR;
                    float ny = (y - c) * invR;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    if (r >= 1.02f)
                    {
                        px[y * size + x] = new Color32(255, 255, 255, 0);
                        continue;
                    }
                    float edgeAa = Mathf.Clamp01((1.01f - r) * (size * 0.42f));
                    // Translucent inner cytoplasm rising to a crisp luminous outer membrane between r = 0.72 and 0.96
                    float rim = Mathf.SmoothStep(0.62f, 0.88f, r) * (1f - 0.18f * Mathf.SmoothStep(0.92f, 1.0f, r));
                    float alpha = Mathf.Lerp(0.56f, 0.98f, rim) * edgeAa;

                    // Soft top-left directional cellular sheen for spherical jelly volume
                    float hx = nx + 0.26f, hy = ny - 0.26f;
                    float spec = Mathf.Exp(-(hx * hx + hy * hy) * 4.5f) * 0.14f;
                    float shade = Mathf.Clamp01(0.88f + rim * 0.14f + spec);
                    byte v = (byte)Mathf.RoundToInt(shade * 255f);
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f);
                    px[y * size + x] = new Color32(v, v, v, a);
                }
            }
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        public void SetVisible(bool visible) { root.SetActive(visible); }

        public void SetSimulation(Simulation s)
        {
            if (sim == s) return;
            sim = s;
            SelectedCellId = NoCell;
            HighlightSpeciesId = 0;
            PreviewActive = false;
            tintDirty = true;
            sinceTint = 99f;
            lastMaxCellId = s != null ? s.NextCellId : -1;
            lastSimTime = s != null ? s.Time : -1;
            for (int i = 0; i < MaxRipples; i++) ripples[i].Duration = 0f;
        }

        public void MarkTintDirty() { tintDirty = true; }

        static Color32 C32(Color c, float alpha = 1f)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Clamp01(c.r) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(c.g) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(c.b) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
        }

        public void Render(CameraController cam, float realDt)
        {
            if (sim == null) { root.SetActive(false); return; }
            root.SetActive(true);
            animTime += Mathf.Min(realDt, 0.1f);
            float upp = cam.UnitsPerPixel;
            float w = sim.World.Width, h = sim.World.Height;

            DetectBirthRipples(realDt);

            background.Begin();
            // Deep outer water + rich tide-pool interior
            background.Quad(w * 0.5f, h * 0.5f, w * 2.5f, h * 2.5f, C32(DesignTokens.Bg0));
            background.Quad(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, C32(DesignTokens.WorldBg));
            background.End();

            RenderTint(realDt, w, h);
            RenderAmbientAndEvents(upp, w, h);
            RenderToxin(upp);
            RenderFood(upp);
            RenderCells(upp);
            RenderOverlay(upp, w, h);
        }

        void DetectBirthRipples(float realDt)
        {
            for (int i = 0; i < MaxRipples; i++)
                if (ripples[i].Duration > 0f) ripples[i].Age += realDt;

            double dtSim = sim.Time - lastSimTime;
            if (lastMaxCellId >= 0 && sim.NextCellId > lastMaxCellId && dtSim > 0 && dtSim < 1.5)
            {
                CellPool pool = sim.Cells;
                int spawned = 0;
                for (int i = pool.AliveCount - 1; i >= 0 && spawned < 6; i--)
                {
                    Cell c = pool.Alive[i];
                    if (c.Id < lastMaxCellId) break;
                    ripples[rippleHead] = new Ripple
                    {
                        X = c.X,
                        Y = c.Y,
                        Age = 0f,
                        Duration = 0.75f,
                        MaxRadius = c.Radius * 3.6f + 0.8f,
                        Color = new Color(c.ColorR, c.ColorG, c.ColorB)
                    };
                    rippleHead = (rippleHead + 1) % MaxRipples;
                    spawned++;
                }
            }
            lastMaxCellId = sim.NextCellId;
            lastSimTime = sim.Time;
        }

        void RenderTint(float realDt, float w, float h)
        {
            tint.Begin();
            sinceTint += realDt;
            bool hasEvent = sim.Events.HasTemperatureEvent();
            if (hadTemperatureEvent && !hasEvent) tintDirty = true;
            hadTemperatureEvent = hasEvent;
            if ((tintDirty || hasEvent) && sinceTint >= TintRefreshSeconds)
            {
                RebuildTint();
                tintDirty = false;
                sinceTint = 0f;
            }
            float opacity = TintOn ? FullTintOpacity : AmbientTintOpacity;
            tint.Quad(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, C32(Color.white, opacity));
            tint.End();
        }

        void RebuildTint()
        {
            float sx = sim.World.Width / TintW, sy = sim.World.Height / TintH;
            for (int j = 0; j < TintH; j++)
                for (int i = 0; i < TintW; i++)
                    tintPixels[j * TintW + i] = TemperatureColors.At(sim.World.TemperatureAt((i + 0.5f) * sx, (j + 0.5f) * sy));
            tintTexture.SetPixels32(tintPixels);
            tintTexture.Apply(false, false);
        }

        void RenderAmbientAndEvents(float upp, float w, float h)
        {
            ambientGlow.Begin();

            // Gentle drifting underwater caustic light pools so the aquatic medium feels alive
            for (int i = 0; i < 7; i++)
            {
                float phase = animTime * 0.14f + i * 1.93f;
                float cx = w * (0.14f + 0.72f * ((i * 0.37f + 0.06f * Mathf.Sin(phase)) % 1f));
                float cy = h * (0.18f + 0.64f * (((i * 0.61f + 0.23f) % 1f) + 0.05f * Mathf.Cos(phase * 0.8f)));
                float rad = 24f + (i % 3) * 9f;
                ambientGlow.Quad(cx, cy, rad, rad * 0.78f, C32(DesignTokens.WorldCaustic, 0.038f));
            }

            // Active environmental Event zones (Heat wave, Ice age, Famine, Food bloom)
            for (int i = 0; i < sim.Events.ActiveCount; i++)
            {
                ActiveEvent ev = sim.Events.Active[i];
                if (ev == null) continue;
                float env = Mathf.Clamp01(ev.Envelope(sim.P));
                Color ec = EventZoneColor(ev.Spec.Kind);
                if (ev.Spec.WholeWorld)
                {
                    ambientGlow.Quad(w * 0.5f, h * 0.5f, w * 0.6f, h * 0.6f, C32(ec, 0.10f * env));
                }
                else
                {
                    float pulse = 1f + 0.03f * Mathf.Sin(animTime * 2.4f + i);
                    float r = ev.Spec.Radius * pulse;
                    ambientGlow.Quad(ev.Spec.X, ev.Spec.Y, r, r, C32(ec, 0.18f * env));
                }
            }

            // Soft bioluminescent phytoplankton glow under Food (subsampled when dense to keep vertex budget tight)
            int foodStride = Mathf.Max(1, sim.FoodCount / 900);
            float fGlowR = Mathf.Max(sim.P.FoodRadius * 4.2f, 4.0f * upp);
            Color32 fg = C32(DesignTokens.WorldFoodGlow, 0.18f);
            for (int i = 0; i < sim.FoodCount; i += foodStride)
                ambientGlow.Quad(sim.FoodX[i], sim.FoodY[i], fGlowR, fGlowR, fg);

            // Soft toxic spore haze
            int toxStride = Mathf.Max(1, sim.ToxCount / 700);
            float tGlowR = Mathf.Max(0.95f, 4.5f * upp);
            for (int i = 0; i < sim.ToxCount; i += toxStride)
            {
                float s = Mathf.Clamp01(sim.ToxS[i]);
                ambientGlow.Quad(sim.ToxX[i], sim.ToxY[i], tGlowR, tGlowR, C32(DesignTokens.WorldToxinHaze, 0.14f * s));
            }

            // Cell energy bioluminescence aura
            CellPool pool = sim.Cells;
            for (int i = 0; i < pool.AliveCount; i++)
            {
                Cell c = pool.Alive[i];
                float fade = HighlightSpeciesId == 0 || c.SpeciesId == HighlightSpeciesId ? 1f : 0.18f;
                float fullness = c.Emax > 0f ? Mathf.Clamp01(c.Energy / c.Emax) : 0.5f;
                float haloR = c.Radius * (1.85f + 0.45f * fullness);
                ambientGlow.Quad(c.X, c.Y, haloR, haloR,
                    C32(new Color(c.ColorR, c.ColorG, c.ColorB), (0.10f + 0.14f * fullness) * fade));
            }

            ambientGlow.End();
        }

        static Color EventZoneColor(EventKind kind)
        {
            switch (kind)
            {
                case EventKind.HeatWave: return DesignTokens.TempHot;
                case EventKind.IceAge: return DesignTokens.TempCold;
                case EventKind.Famine: return DesignTokens.StatusWarning;
                case EventKind.FoodBloom: return DesignTokens.WorldFood;
                default: return DesignTokens.WorldToxin;
            }
        }

        void RenderToxin(float upp)
        {
            toxin.Begin();
            float r = Mathf.Max(0.28f, 1.8f * upp);
            for (int i = 0; i < sim.ToxCount; i++)
            {
                float strength = Mathf.Clamp01(sim.ToxS[i]);
                toxin.Quad(sim.ToxX[i], sim.ToxY[i], r, r, C32(DesignTokens.WorldToxin, 0.28f + 0.62f * strength));
            }
            toxin.End();
        }

        void RenderFood(float upp)
        {
            food.Begin();
            float r = Mathf.Max(sim.P.FoodRadius * 1.35f, 1.85f * upp);
            Color32 outer = C32(DesignTokens.WorldFood, 0.92f);
            for (int i = 0; i < sim.FoodCount; i++)
                food.Quad(sim.FoodX[i], sim.FoodY[i], r, r, outer);
            food.End();
        }

        void RenderCells(float upp)
        {
            cellGlow.Begin();
            cells.Begin();
            CellPool pool = sim.Cells;
            Color32 pupilColor = C32(DesignTokens.WorldEye);

            for (int i = 0; i < pool.AliveCount; i++)
            {
                Cell c = pool.Alive[i];
                float alpha = HighlightSpeciesId == 0 || c.SpeciesId == HighlightSpeciesId ? 1f : CellFadeAlpha;
                float r = c.Radius;
                float cos = Mathf.Cos(c.Heading), sin = Mathf.Sin(c.Heading);
                Color bodyCol = new Color(c.ColorR, c.ColorG, c.ColorB);

                // 1. Undulating swimming flagellum tail behind the Cell
                float thrust = Mathf.Clamp01(c.OutThrust);
                float speedTrait = c.G.Trait[TraitId.Speed];
                float wagPhase = animTime * (4.5f + speedTrait * 2.6f * (0.35f + thrust)) + c.Id * 1.91f;
                float wag1 = Mathf.Sin(wagPhase) * r * (0.16f + 0.22f * thrust);
                float wag2 = Mathf.Sin(wagPhase - 1.35f) * r * (0.28f + 0.34f * thrust);
                float bx0 = c.X - cos * (r * 0.82f), by0 = c.Y - sin * (r * 0.82f);
                float tx1 = c.X - cos * (r * 1.45f) - sin * wag1;
                float ty1 = c.Y - sin * (r * 1.45f) + cos * wag1;
                float tx2 = c.X - cos * (r * 2.05f) - sin * wag2;
                float ty2 = c.Y - sin * (r * 2.05f) + cos * wag2;
                float tailThick = Mathf.Max(r * 0.18f, 1.1f * upp);
                cells.Segment(bx0, by0, tx1, ty1, tailThick, C32(bodyCol, 0.48f * alpha));
                cells.Segment(tx1, ty1, tx2, ty2, tailThick * 0.65f, C32(bodyCol, 0.26f * alpha));

                // 2. Predator forward mandible prongs when Diet >= 0.45 so hunters are visually unmistakable
                float diet = c.G.Trait[TraitId.Diet];
                if (diet >= 0.45f)
                {
                    float pScale = Mathf.Clamp01((diet - 0.45f) / 0.45f);
                    float jawLen = r * (0.32f + 0.22f * pScale);
                    float jawThick = Mathf.Max(r * 0.14f, 1.0f * upp);
                    Color32 jawCol = C32(Color.Lerp(bodyCol, DesignTokens.WorldPredatorCore, 0.65f), 0.85f * alpha);
                    float jxL = c.X + Mathf.Cos(c.Heading + 0.42f) * (r * 0.88f);
                    float jyL = c.Y + Mathf.Sin(c.Heading + 0.42f) * (r * 0.88f);
                    float jxR = c.X + Mathf.Cos(c.Heading - 0.42f) * (r * 0.88f);
                    float jyR = c.Y + Mathf.Sin(c.Heading - 0.42f) * (r * 0.88f);
                    cells.Segment(jxL, jyL, jxL + Mathf.Cos(c.Heading + 0.15f) * jawLen, jyL + Mathf.Sin(c.Heading + 0.15f) * jawLen, jawThick, jawCol);
                    cells.Segment(jxR, jyR, jxR + Mathf.Cos(c.Heading - 0.15f) * jawLen, jyR + Mathf.Sin(c.Heading - 0.15f) * jawLen, jawThick, jawCol);
                }

                // 3. Organic translucent membrane + cytoplasm body
                cellGlow.Quad(c.X, c.Y, r, r, C32(bodyCol, alpha));

                // 4. Inner organelle / nucleus (emerald tint for grazers, warm coral-amber for predators)
                Color coreTint = Color.Lerp(DesignTokens.WorldGrazerCore, DesignTokens.WorldPredatorCore, Mathf.Clamp01(diet * 1.25f));
                Color nucleusCol = Color.Lerp(bodyCol, coreTint, 0.55f);
                float nr = r * 0.36f;
                float nx = c.X - cos * (r * 0.12f), ny = c.Y - sin * (r * 0.12f);
                cells.Quad(nx, ny, nr, nr, C32(nucleusCol, 0.72f * alpha));

                // 5. Crisp directional eyes (sclera + pupil)
                float er = 0.22f * r, pr = 0.135f * r, ed = 0.54f * r;
                float exL = c.X + Mathf.Cos(c.Heading + 0.58f) * ed, eyL = c.Y + Mathf.Sin(c.Heading + 0.58f) * ed;
                float exR = c.X + Mathf.Cos(c.Heading - 0.58f) * ed, eyR = c.Y + Mathf.Sin(c.Heading - 0.58f) * ed;
                Color32 sclera = C32(Color.white, 0.92f * alpha);
                Color32 pupil = new Color32(pupilColor.r, pupilColor.g, pupilColor.b, (byte)Mathf.RoundToInt(alpha * 255f));
                cells.Quad(exL, eyL, er, er, sclera);
                cells.Quad(exR, eyR, er, er, sclera);
                float pShift = 0.055f * r;
                cells.Quad(exL + cos * pShift, eyL + sin * pShift, pr, pr, pupil);
                cells.Quad(exR + cos * pShift, eyR + sin * pShift, pr, pr, pupil);
            }

            cellGlow.End();
            cells.End();
        }

        void RenderOverlay(float upp, float w, float h)
        {
            overlay.Begin();

            // Luminous petri-dish / tide-pool containment frame
            float outerPad = 4f * upp;
            Color32 outerGlow = C32(DesignTokens.WorldWall, 0.22f);
            Color32 innerWall = C32(DesignTokens.WorldWall, 0.68f);
            float t = Mathf.Max(1.35f * upp, 0.06f);
            overlay.Segment(-outerPad, -outerPad, w + outerPad, -outerPad, t * 2.2f, outerGlow);
            overlay.Segment(w + outerPad, -outerPad, w + outerPad, h + outerPad, t * 2.2f, outerGlow);
            overlay.Segment(w + outerPad, h + outerPad, -outerPad, h + outerPad, t * 2.2f, outerGlow);
            overlay.Segment(-outerPad, h + outerPad, -outerPad, -outerPad, t * 2.2f, outerGlow);

            overlay.Segment(0, 0, w, 0, t, innerWall);
            overlay.Segment(w, 0, w, h, t, innerWall);
            overlay.Segment(w, h, 0, h, t, innerWall);
            overlay.Segment(0, h, 0, 0, t, innerWall);

            // Mitosis / birth ripples
            for (int i = 0; i < MaxRipples; i++)
            {
                if (ripples[i].Duration <= 0f || ripples[i].Age >= ripples[i].Duration) continue;
                float u = ripples[i].Age / ripples[i].Duration;
                float rad = Mathf.Lerp(0.4f, ripples[i].MaxRadius, 1f - (1f - u) * (1f - u));
                float alpha = (1f - u) * 0.45f;
                Ring(ripples[i].X, ripples[i].Y, rad, Mathf.Max(1.2f * upp, 0.05f), 20, C32(ripples[i].Color, alpha));
            }

            // Active localized Event boundary rings
            for (int i = 0; i < sim.Events.ActiveCount; i++)
            {
                ActiveEvent ev = sim.Events.Active[i];
                if (ev == null || ev.Spec.WholeWorld) continue;
                float env = Mathf.Clamp01(ev.Envelope(sim.P));
                Color ec = EventZoneColor(ev.Spec.Kind);
                DashedRing(ev.Spec.X, ev.Spec.Y, ev.Spec.Radius, 1.5f * upp, 36, animTime * 0.35f, C32(ec, 0.48f * env));
            }

            // Selected Cell: Sense Range sonar circle + crisp selection ring
            if (SelectedCellId != NoCell)
            {
                Cell c = sim.FindCell(SelectedCellId);
                if (c != null && c.Alive)
                {
                    float sense = c.G.Trait[TraitId.Sense];
                    DashedRing(c.X, c.Y, sense, Mathf.Max(1.1f * upp, 0.04f), 40, animTime * 0.2f, C32(DesignTokens.WorldSense, 0.34f));
                    float pulse = 1f + 0.08f * Mathf.Sin(animTime * 4f);
                    Ring(c.X, c.Y, c.Radius + (2.5f + pulse) * upp, 1.8f * upp, 32, C32(DesignTokens.WorldSelection, 0.95f));
                }
            }

            // Armed Event placement preview
            if (PreviewActive)
            {
                float circumferencePx = 2f * Mathf.PI * PreviewRadius / Mathf.Max(upp, 1e-5f);
                int dashes = Mathf.Clamp(Mathf.RoundToInt(circumferencePx / 10f), 16, 120);
                Color pcCol = EventZoneColor(PreviewKind);
                DashedRing(PreviewCentre.x, PreviewCentre.y, PreviewRadius, 2.2f * upp, dashes, animTime * 0.6f, C32(pcCol, 0.92f));
            }

            overlay.End();
        }

        void Ring(float cx, float cy, float radius, float thickness, int segments, Color32 color)
        {
            float step = 2f * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * step, a1 = a0 + step * 1.05f;
                overlay.Segment(cx + Mathf.Cos(a0) * radius, cy + Mathf.Sin(a0) * radius,
                    cx + Mathf.Cos(a1) * radius, cy + Mathf.Sin(a1) * radius, thickness, color);
            }
        }

        void DashedRing(float cx, float cy, float radius, float thickness, int dashes, float phase, Color32 color)
        {
            float step = 2f * Mathf.PI / dashes;
            for (int i = 0; i < dashes; i++)
            {
                float a0 = phase + i * step, a1 = a0 + step * 0.55f;
                overlay.Segment(cx + Mathf.Cos(a0) * radius, cy + Mathf.Sin(a0) * radius,
                    cx + Mathf.Cos(a1) * radius, cy + Mathf.Sin(a1) * radius, thickness, color);
            }
        }
    }
}
