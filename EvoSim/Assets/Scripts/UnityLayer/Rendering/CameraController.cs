using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Full-bleed orthographic camera over the World. The camera always renders across 100% of the window so the living
    /// World blurs and refracts underneath floating Liquid Glass panels, while Fit() centres the World cleanly inside
    /// the unobstructed stage region.
    /// </summary>
    public sealed class CameraController
    {
        public const float FitMargin = 3.5f;
        public const float MinSize = 8f;
        public const float MaxFitFactor = 1.35f;
        public const float ZoomStep = 1.1f;

        public readonly Camera Camera;
        float worldW = 160f, worldH = 90f;
        Rect stage = new Rect(0, 0, 1, 1);

        public CameraController(Camera camera)
        {
            Camera = camera;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DesignTokens.Bg0;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 10f;
            camera.rect = new Rect(0, 0, 1, 1);
            camera.transform.position = new Vector3(80f, 45f, -1f);
        }

        public Rect Viewport { get { return stage; } }

        /// <summary>Pixels per World unit at the current zoom.</summary>
        public float PixelsPerUnit { get { return Mathf.Max(1, Camera.pixelHeight) / (2f * Camera.orthographicSize); } }

        public float UnitsPerPixel { get { return 1f / Mathf.Max(0.0001f, PixelsPerUnit); } }

        public void SetWorld(float width, float height)
        {
            worldW = width; worldH = height;
            Fit();
        }

        /// <summary>Records the unobstructed stage rect in normalised window coordinates while keeping full-bleed camera output.</summary>
        public void SetViewport(Rect normalised)
        {
            if (normalised.width <= 0.01f || normalised.height <= 0.01f) return;
            bool changed = normalised != stage;
            stage = normalised;
            Camera.rect = new Rect(0, 0, 1, 1);
            if (changed) { ClampSize(); ClampPosition(); }
        }

        float FullAspect { get { return Mathf.Max(0.01f, Camera.pixelWidth / (float)Mathf.Max(1, Camera.pixelHeight)); } }

        public float FitSize
        {
            get
            {
                float sw = Mathf.Clamp(stage.width, 0.25f, 1f);
                float sh = Mathf.Clamp(stage.height, 0.25f, 1f);
                float halfH = (worldH + 2f * FitMargin) * 0.5f / sh;
                float halfW = (worldW + 2f * FitMargin) * 0.5f / Mathf.Max(0.05f, FullAspect * sw);
                return Mathf.Max(halfH, halfW);
            }
        }

        public void Fit()
        {
            float size = FitSize;
            Camera.orthographicSize = size;
            float fullW = 2f * size * FullAspect;
            float fullH = 2f * size;
            // Shift camera so the World centre lands at the centre of the unobstructed stage
            float offsetX = (stage.center.x - 0.5f) * fullW;
            float offsetY = (stage.center.y - 0.5f) * fullH;
            Camera.transform.position = new Vector3(worldW * 0.5f - offsetX, worldH * 0.5f - offsetY, -1f);
        }

        /// <summary>Centres directly on the World for the full-screen Setup ambient backdrop.</summary>
        public void FitCentered(float zoomFactor = 0.86f)
        {
            float halfH = worldH * 0.5f;
            float halfW = worldW * 0.5f / FullAspect;
            Camera.orthographicSize = Mathf.Min(halfH, halfW) * zoomFactor;
            Camera.transform.position = new Vector3(worldW * 0.5f, worldH * 0.5f, -1f);
        }

        void ClampSize()
        {
            Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize, Mathf.Min(MinSize, FitSize), FitSize * MaxFitFactor);
        }

        void ClampPosition()
        {
            float halfH = Camera.orthographicSize, halfW = halfH * FullAspect;
            float padX = halfW * 0.85f + FitMargin;
            float padY = halfH * 0.85f + FitMargin;
            Vector3 p = Camera.transform.position;
            p.x = Mathf.Clamp(p.x, -padX + halfW, worldW + padX - halfW);
            p.y = Mathf.Clamp(p.y, -padY + halfH, worldH + padY - halfH);
            Camera.transform.position = p;
        }

        public Vector2 ScreenToWorld(Vector2 screen)
        {
            Vector3 w = Camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            return new Vector2(w.x, w.y);
        }

        public bool ViewportContains(Vector2 screen)
        {
            return screen.x >= 0 && screen.y >= 0 && screen.x <= Screen.width && screen.y <= Screen.height;
        }

        public void PanByPixels(Vector2 deltaPixels)
        {
            float u = UnitsPerPixel;
            Camera.transform.position -= new Vector3(deltaPixels.x * u, deltaPixels.y * u, 0f);
            ClampPosition();
        }

        /// <summary>Zooms by notches (positive zooms in) keeping the World point under the pointer fixed.</summary>
        public void Zoom(float notches, Vector2 screenPointer)
        {
            if (Mathf.Abs(notches) < 1e-4f) return;
            Vector2 before = ScreenToWorld(screenPointer);
            Camera.orthographicSize /= Mathf.Pow(ZoomStep, notches);
            ClampSize();
            Vector2 after = ScreenToWorld(screenPointer);
            Camera.transform.position += new Vector3(before.x - after.x, before.y - after.y, 0f);
            ClampPosition();
        }

        public void OnResize()
        {
            ClampSize();
            ClampPosition();
        }
    }
}
