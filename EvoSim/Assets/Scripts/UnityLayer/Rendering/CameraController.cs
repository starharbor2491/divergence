using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Orthographic camera over the World. The camera viewport is the part of the window left of the side panel and
    /// under the top bar. Pan by dragging, zoom around the pointer, F fits the World (PLAN 12.4, 13).
    /// </summary>
    public sealed class CameraController
    {
        public const float FitMargin = 2f;
        public const float MinSize = 8f;
        public const float MaxFitFactor = 1.2f;
        public const float ZoomStep = 1.1f;

        public readonly Camera Camera;
        float worldW = 160f, worldH = 90f;
        Rect viewport = new Rect(0, 0, 1, 1);

        public CameraController(Camera camera)
        {
            Camera = camera;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DesignTokens.WorldBg;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(80f, 45f, -1f);
        }

        public Rect Viewport { get { return viewport; } }

        /// <summary>Pixels per World unit at the current zoom.</summary>
        public float PixelsPerUnit { get { return Camera.pixelHeight / (2f * Camera.orthographicSize); } }

        public float UnitsPerPixel { get { return 1f / Mathf.Max(0.0001f, PixelsPerUnit); } }

        public void SetWorld(float width, float height)
        {
            worldW = width; worldH = height;
            Fit();
        }

        /// <summary>Sets the viewport in normalised window coordinates (origin at the bottom left).</summary>
        public void SetViewport(Rect normalised)
        {
            if (normalised.width <= 0.01f || normalised.height <= 0.01f) return;
            bool changed = normalised != viewport;
            viewport = normalised;
            Camera.rect = normalised;
            if (changed) { ClampSize(); ClampPosition(); }
        }

        float Aspect { get { return Mathf.Max(0.01f, Camera.pixelWidth / (float)Mathf.Max(1, Camera.pixelHeight)); } }

        public float FitSize
        {
            get
            {
                float halfH = (worldH + 2f * FitMargin) * 0.5f;
                float halfW = (worldW + 2f * FitMargin) * 0.5f;
                return Mathf.Max(halfH, halfW / Aspect);
            }
        }

        public void Fit()
        {
            Camera.orthographicSize = FitSize;
            Camera.transform.position = new Vector3(worldW * 0.5f, worldH * 0.5f, -1f);
        }

        void ClampSize()
        {
            Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize, Mathf.Min(MinSize, FitSize), FitSize * MaxFitFactor);
        }

        void ClampPosition()
        {
            float halfH = Camera.orthographicSize, halfW = halfH * Aspect;
            Vector3 p = Camera.transform.position;
            p.x = halfW * 2f >= worldW + 2f * FitMargin ? worldW * 0.5f : Mathf.Clamp(p.x, halfW - FitMargin, worldW + FitMargin - halfW);
            p.y = halfH * 2f >= worldH + 2f * FitMargin ? worldH * 0.5f : Mathf.Clamp(p.y, halfH - FitMargin, worldH + FitMargin - halfH);
            Camera.transform.position = p;
        }

        public Vector2 ScreenToWorld(Vector2 screen)
        {
            Vector3 w = Camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            return new Vector2(w.x, w.y);
        }

        public bool ViewportContains(Vector2 screen)
        {
            return Camera.pixelRect.Contains(screen);
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

        /// <summary>Keeps the zoom limits valid after the window size changes.</summary>
        public void OnResize()
        {
            ClampSize();
            ClampPosition();
        }
    }
}
