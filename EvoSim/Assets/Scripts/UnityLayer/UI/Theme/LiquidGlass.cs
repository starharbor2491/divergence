using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum GlassTone
    {
        /// <summary>Primary floating HUD surface directly over the live World (real camera frost + refraction).</summary>
        Panel,
        /// <summary>Compact floating capsule pill over the live World (stronger bevel refraction).</summary>
        Pill,
        /// <summary>Modal dialog, dropdown menu or tooltip that may overlap other UI (high-legibility frost).</summary>
        Modal,
        /// <summary>Recessed card or well nested inside an already-frosted glass panel.</summary>
        Well
    }

    public sealed class GlassEntry
    {
        public VisualElement Element;
        public float Radius;
        public GlassTone Tone;
    }

    /// <summary>
    /// Apple-style Liquid Glass material for UI Toolkit: registers floating surfaces with GlassBackdrop for real-time
    /// World blur & curved-edge refraction, and paints soft elevation shadow, upper liquid sheen, and directional
    /// specular meniscus rims with zero extra child VisualElements.
    /// </summary>
    public static class LiquidGlass
    {
        public static readonly List<GlassEntry> Surfaces = new List<GlassEntry>();

        public static void Attach(VisualElement e, float radius, GlassTone tone = GlassTone.Panel, bool shadow = true)
        {
            e.Round(radius);
            if (tone == GlassTone.Well)
            {
                StyleWell(e, radius);
                return;
            }

            ApplyFill(e, tone);

            var entry = new GlassEntry { Element = e, Radius = radius, Tone = tone };
            e.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (!Surfaces.Contains(entry)) Surfaces.Add(entry);
                ApplyFill(e, tone);
            });
            e.RegisterCallback<DetachFromPanelEvent>(_ => Surfaces.Remove(entry));
            if (e.panel != null && !Surfaces.Contains(entry)) Surfaces.Add(entry);

            e.generateVisualContent += ctx => PaintSpecularChrome(ctx, e, radius, tone, shadow);
        }

        public static void StyleWell(VisualElement e, float radius)
        {
            e.Round(radius);
            e.style.backgroundColor = DesignTokens.GlassInset;
            e.Border(DesignTokens.Border, DesignTokens.BorderWidth);
        }

        static void ApplyFill(VisualElement e, GlassTone tone)
        {
            bool hasLiveBackdrop = GlassBackdrop.IsActive;
            switch (tone)
            {
                case GlassTone.Modal:
                    e.style.backgroundColor = DesignTokens.GlassModal;
                    break;
                case GlassTone.Pill:
                    e.style.backgroundColor = hasLiveBackdrop
                        ? new Color(0.75f, 0.92f, 1f, 0.03f)
                        : DesignTokens.Hex("#0D1E2E", 0.82f);
                    break;
                default:
                    e.style.backgroundColor = hasLiveBackdrop
                        ? new Color(0.75f, 0.92f, 1f, 0.035f)
                        : DesignTokens.Hex("#0C1B2A", 0.84f);
                    break;
            }
        }

        public static void RefreshFills()
        {
            for (int i = 0; i < Surfaces.Count; i++)
                ApplyFill(Surfaces[i].Element, Surfaces[i].Tone);
        }

        static void PaintSpecularChrome(MeshGenerationContext ctx, VisualElement e, float radius, GlassTone tone, bool shadow)
        {
            // Use full border box [0..layout.width, 0..layout.height]
            float w = e.layout.width, h = e.layout.height;
            if (float.IsNaN(w) || float.IsNaN(h) || w < 4f || h < 4f) return;
            float rad = Mathf.Clamp(radius, 0f, Mathf.Min(w, h) * 0.5f);
            Painter2D p = ctx.painter2D;
            p.lineCap = LineCap.Round;
            p.lineJoin = LineJoin.Round;

            // 1. Soft multi-step offset drop shadow outside the glass lens
            if (shadow)
            {
                StrokeRoundedRect(p, -1.5f, 1.5f, w + 3f, h + 3f, rad + 1.5f, 3f, new Color(0.01f, 0.03f, 0.06f, 0.20f));
                StrokeRoundedRect(p, -4.0f, 3.5f, w + 8f, h + 8f, rad + 4f, 4.5f, new Color(0.01f, 0.03f, 0.06f, 0.11f));
                StrokeRoundedRect(p, -7.5f, 6.0f, w + 15f, h + 15f, rad + 7.5f, 6f, new Color(0.01f, 0.03f, 0.06f, 0.05f));
            }

            // 2. Subtle upper liquid sheen band (gives curved convex droplet feel)
            float sheenH = Mathf.Clamp(h * 0.32f, 10f, 52f);
            if (sheenH < h - 4f && w > rad * 2f + 2f)
            {
                float inset = 1.5f;
                float sr = Mathf.Max(2f, rad - inset);
                p.fillColor = tone == GlassTone.Modal
                    ? new Color(1f, 1f, 1f, 0.045f)
                    : DesignTokens.GlassSheen;
                p.BeginPath();
                p.MoveTo(new Vector2(inset + sr, inset));
                p.LineTo(new Vector2(w - inset - sr, inset));
                p.Arc(new Vector2(w - inset - sr, inset + sr), sr, Angle.Degrees(-90), Angle.Degrees(0));
                p.LineTo(new Vector2(w - inset, sheenH));
                p.LineTo(new Vector2(inset, sheenH));
                p.LineTo(new Vector2(inset, inset + sr));
                p.Arc(new Vector2(inset + sr, inset + sr), sr, Angle.Degrees(180), Angle.Degrees(270));
                p.ClosePath();
                p.Fill();
            }

            // 3. Base perimeter refractive hairline
            StrokeRoundedRect(p, 0.5f, 0.5f, w - 1f, h - 1f, Mathf.Max(0f, rad - 0.5f), 1.15f, DesignTokens.GlassRimSide);

            // 4. Directional top-left Apple Liquid Glass specular highlight
            float rr = Mathf.Max(0f, rad - 0.5f);
            p.lineWidth = 1.35f;
            p.strokeColor = DesignTokens.GlassRimTop;
            p.BeginPath();
            if (rr > 0.5f)
            {
                p.MoveTo(new Vector2(0.5f, Mathf.Min(h - rr - 0.5f, h * 0.55f)));
                p.LineTo(new Vector2(0.5f, 0.5f + rr));
                p.Arc(new Vector2(0.5f + rr, 0.5f + rr), rr, Angle.Degrees(180), Angle.Degrees(270));
                p.LineTo(new Vector2(w - 0.5f - rr, 0.5f));
                p.Arc(new Vector2(w - 0.5f - rr, 0.5f + rr), rr, Angle.Degrees(-90), Angle.Degrees(-25));
            }
            else
            {
                p.MoveTo(new Vector2(0.5f, h * 0.55f));
                p.LineTo(new Vector2(0.5f, 0.5f));
                p.LineTo(new Vector2(w - 0.5f, 0.5f));
            }
            p.Stroke();

            // 5. Bottom-right bioluminescent water bounce highlight
            p.lineWidth = 1.15f;
            p.strokeColor = DesignTokens.GlassRimBottom;
            p.BeginPath();
            if (rr > 0.5f)
            {
                p.MoveTo(new Vector2(w - 0.5f, Mathf.Max(0.5f + rr, h * 0.52f)));
                p.LineTo(new Vector2(w - 0.5f, h - 0.5f - rr));
                p.Arc(new Vector2(w - 0.5f - rr, h - 0.5f - rr), rr, Angle.Degrees(0), Angle.Degrees(90));
                p.LineTo(new Vector2(Mathf.Max(0.5f + rr, w * 0.25f), h - 0.5f));
            }
            else
            {
                p.MoveTo(new Vector2(w - 0.5f, h * 0.52f));
                p.LineTo(new Vector2(w - 0.5f, h - 0.5f));
                p.LineTo(new Vector2(w * 0.25f, h - 0.5f));
            }
            p.Stroke();
        }

        static void StrokeRoundedRect(Painter2D p, float x, float y, float w, float h, float r, float lineWidth, Color color)
        {
            if (w <= 1f || h <= 1f) return;
            r = Mathf.Clamp(r, 0f, Mathf.Min(w, h) * 0.5f);
            p.lineWidth = lineWidth;
            p.strokeColor = color;
            p.BeginPath();
            if (r <= 0.5f)
            {
                p.MoveTo(new Vector2(x, y));
                p.LineTo(new Vector2(x + w, y));
                p.LineTo(new Vector2(x + w, y + h));
                p.LineTo(new Vector2(x, y + h));
                p.ClosePath();
            }
            else
            {
                p.MoveTo(new Vector2(x + r, y));
                p.LineTo(new Vector2(x + w - r, y));
                p.Arc(new Vector2(x + w - r, y + r), r, Angle.Degrees(-90), Angle.Degrees(0));
                p.LineTo(new Vector2(x + w, y + h - r));
                p.Arc(new Vector2(x + w - r, y + h - r), r, Angle.Degrees(0), Angle.Degrees(90));
                p.LineTo(new Vector2(x + r, y + h));
                p.Arc(new Vector2(x + r, y + h - r), r, Angle.Degrees(90), Angle.Degrees(180));
                p.LineTo(new Vector2(x, y + r));
                p.Arc(new Vector2(x + r, y + r), r, Angle.Degrees(180), Angle.Degrees(270));
                p.ClosePath();
            }
            p.Stroke();
        }
    }
}
