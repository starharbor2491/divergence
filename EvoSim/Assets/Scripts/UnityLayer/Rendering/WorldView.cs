using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Draws the World: background, Temperature tint, Toxin, Food, Cells, selection ring, Event preview (PLAN 13).
    /// Everything is rebuilt from the Simulation state each frame into a few dynamic meshes; nothing here is simulation state.
    /// </summary>
    public sealed class WorldView
    {
        const float TintOpacity = 0.30f;
        const float TintRefreshSeconds = 0.5f;
        const float CellFadeAlpha = 0.35f;
        const int TintW = 160, TintH = 90;

        readonly GameObject root;
        readonly Material whiteMaterial, circleMaterial, tintMaterial;
        readonly Texture2D whiteTexture, circleTexture, tintTexture;
        readonly QuadBatch background, tint, toxin, food, cells, overlay;
        readonly Color32[] tintPixels = new Color32[TintW * TintH];

        Simulation sim;
        bool tintDirty = true;
        bool hadTemperatureEvent;
        float sinceTint;

        public bool TintOn;
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
            tintTexture = MakeTexture(TintW, TintH, FilterMode.Bilinear);

            whiteMaterial = MakeMaterial(shader, whiteTexture);
            circleMaterial = MakeMaterial(shader, circleTexture);
            tintMaterial = MakeMaterial(shader, tintTexture);

            background = new QuadBatch("Background", root.transform, whiteMaterial, 1, 0);
            tint = new QuadBatch("Temperature tint", root.transform, tintMaterial, 1, 1);
            toxin = new QuadBatch("Toxin", root.transform, circleMaterial, 1500, 2);
            food = new QuadBatch("Food", root.transform, circleMaterial, 1500, 3);
            cells = new QuadBatch("Cells", root.transform, circleMaterial, 700 * 3, 4);
            overlay = new QuadBatch("Overlay", root.transform, circleMaterial, 400, 5);
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

        /// <summary>White disc with a soft one-texel edge, used for Cells, eyes, Food and Toxin. Its centre texel is solid white, so line segments sample it too.</summary>
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

        public void SetVisible(bool visible) { root.SetActive(visible); }

        public void SetSimulation(Simulation s)
        {
            sim = s;
            SelectedCellId = NoCell;
            HighlightSpeciesId = 0;
            PreviewActive = false;
            tintDirty = true;
            sinceTint = 99f;
        }

        public void MarkTintDirty() { tintDirty = true; }

        static Color32 C32(Color c, float alpha = 1f)
        {
            return new Color32((byte)Mathf.RoundToInt(c.r * 255f), (byte)Mathf.RoundToInt(c.g * 255f), (byte)Mathf.RoundToInt(c.b * 255f), (byte)Mathf.RoundToInt(alpha * 255f));
        }

        public void Render(CameraController cam, float realDt)
        {
            if (sim == null) { root.SetActive(false); return; }
            root.SetActive(true);
            float upp = cam.UnitsPerPixel;
            float w = sim.World.Width, h = sim.World.Height;

            background.Begin();
            background.Quad(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, C32(DesignTokens.WorldBg));
            background.End();

            RenderTint(realDt, w, h);
            RenderToxin(upp);
            RenderFood(upp);
            RenderCells();
            RenderOverlay(cam, upp, w, h);
        }

        void RenderTint(float realDt, float w, float h)
        {
            tint.Begin();
            if (TintOn)
            {
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
                tint.Quad(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, C32(Color.white, TintOpacity));
            }
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

        void RenderToxin(float upp)
        {
            toxin.Begin();
            float r = Mathf.Max(0.25f, 1.5f * upp);
            for (int i = 0; i < sim.ToxCount; i++)
            {
                float strength = Mathf.Clamp01(sim.ToxS[i]);
                toxin.Quad(sim.ToxX[i], sim.ToxY[i], r, r, C32(DesignTokens.WorldToxin, 0.2f + 0.6f * strength));
            }
            toxin.End();
        }

        void RenderFood(float upp)
        {
            food.Begin();
            float r = Mathf.Max(sim.P.FoodRadius, 1.5f * upp);
            Color32 c = C32(DesignTokens.WorldFood);
            for (int i = 0; i < sim.FoodCount; i++) food.Quad(sim.FoodX[i], sim.FoodY[i], r, r, c);
            food.End();
        }

        void RenderCells()
        {
            cells.Begin();
            CellPool pool = sim.Cells;
            Color32 eye = C32(DesignTokens.WorldEye);
            for (int i = 0; i < pool.AliveCount; i++)
            {
                Cell c = pool.Alive[i];
                float alpha = HighlightSpeciesId == 0 || c.SpeciesId == HighlightSpeciesId ? 1f : CellFadeAlpha;
                float r = c.Radius;
                cells.Quad(c.X, c.Y, r, r, C32(new Color(c.ColorR, c.ColorG, c.ColorB), alpha));
                Color32 e = new Color32(eye.r, eye.g, eye.b, (byte)Mathf.RoundToInt(alpha * 255f));
                float er = 0.2f * r, ed = 0.55f * r;
                cells.Quad(c.X + Mathf.Cos(c.Heading + 0.6f) * ed, c.Y + Mathf.Sin(c.Heading + 0.6f) * ed, er, er, e);
                cells.Quad(c.X + Mathf.Cos(c.Heading - 0.6f) * ed, c.Y + Mathf.Sin(c.Heading - 0.6f) * ed, er, er, e);
            }
            cells.End();
        }

        void RenderOverlay(CameraController cam, float upp, float w, float h)
        {
            overlay.Begin();
            Color32 wall = C32(DesignTokens.WorldWall);
            float t = Mathf.Max(upp, 0.05f);
            overlay.Segment(0, 0, w, 0, t, wall);
            overlay.Segment(w, 0, w, h, t, wall);
            overlay.Segment(w, h, 0, h, t, wall);
            overlay.Segment(0, h, 0, 0, t, wall);

            if (SelectedCellId != NoCell)
            {
                Cell c = sim.FindCell(SelectedCellId);
                if (c != null && c.Alive) Ring(c.X, c.Y, c.Radius + 2f * upp, 1.5f * upp, 32, C32(DesignTokens.WorldSelection));
            }

            if (PreviewActive)
            {
                float circumferencePx = 2f * Mathf.PI * PreviewRadius / Mathf.Max(upp, 1e-5f);
                int dashes = Mathf.Clamp(Mathf.RoundToInt(circumferencePx / 8f), 12, 150);
                Color32 pc = C32(DesignTokens.WorldPreview);
                float step = 2f * Mathf.PI / dashes;
                for (int i = 0; i < dashes; i++)
                {
                    float a0 = i * step, a1 = a0 + step * 0.5f;
                    overlay.Segment(PreviewCentre.x + Mathf.Cos(a0) * PreviewRadius, PreviewCentre.y + Mathf.Sin(a0) * PreviewRadius,
                        PreviewCentre.x + Mathf.Cos(a1) * PreviewRadius, PreviewCentre.y + Mathf.Sin(a1) * PreviewRadius, 2f * upp, pc);
                }
            }
            overlay.End();
        }

        void Ring(float cx, float cy, float radius, float thickness, int segments, Color32 color)
        {
            float step = 2f * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * step, a1 = a0 + step * 1.05f;
                overlay.Segment(cx + Mathf.Cos(a0) * radius, cy + Mathf.Sin(a0) * radius, cx + Mathf.Cos(a1) * radius, cy + Mathf.Sin(a1) * radius, thickness, color);
            }
        }
    }
}
