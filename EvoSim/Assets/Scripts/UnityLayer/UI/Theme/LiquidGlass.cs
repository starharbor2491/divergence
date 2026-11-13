using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum GlassTone
    {
        Panel,
        Pill,
        Modal,
        Well
    }

    public sealed class GlassEntry
    {
        public VisualElement Element;
        public float Radius;
        public GlassTone Tone;
    }

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

            e.generateVisualContent += ctx => PaintEdge(ctx, e, radius, shadow);
        }

        public static void StyleWell(VisualElement e, float radius)
        {
            e.Round(radius);
            e.style.backgroundColor = DesignTokens.GlassInset;
            e.Border(DesignTokens.Border, DesignTokens.BorderWidth);
        }

        static void ApplyFill(VisualElement e, GlassTone tone)
        {
            if (tone == GlassTone.Modal)
            {
                e.style.backgroundColor = DesignTokens.GlassModal;
                return;
            }
            e.style.backgroundColor = GlassBackdrop.IsActive
                ? new Color(1f, 1f, 1f, 0.025f)
                : DesignTokens.Hex("#141820", 0.86f);
        }

        public static void RefreshFills()
        {
            for (int i = 0; i < Surfaces.Count; i++)
                ApplyFill(Surfaces[i].Element, Surfaces[i].Tone);
        }

        static void PaintEdge(MeshGenerationContext ctx, VisualElement e, float radius, bool shadow)
        {
            float w = e.layout.width, h = e.layout.height;
            if (float.IsNaN(w) || float.IsNaN(h) || w < 4f || h < 4f) return;
            float rad = Mathf.Clamp(radius, 0f, Mathf.Min(w, h) * 0.5f);
            Painter2D p = ctx.painter2D;
            p.lineCap = LineCap.Round;
            p.lineJoin = LineJoin.Round;

            if (shadow)
            {
                StrokeRect(p, -1.5f, 2f, w + 3f, h + 3f, rad + 1.5f, 3f, new Color(0f, 0f, 0f, 0.22f));
                StrokeRect(p, -4.5f, 4.5f, w + 9f, h + 9f, rad + 4.5f, 5f, new Color(0f, 0f, 0f, 0.10f));
            }

            StrokeRect(p, 0.5f, 0.5f, w - 1f, h - 1f, Mathf.Max(0f, rad - 0.5f), 1f, DesignTokens.GlassRimSide);

            float rr = Mathf.Max(0f, rad - 0.5f);
            p.lineWidth = 1.15f;
            p.strokeColor = DesignTokens.GlassRimTop;
            p.BeginPath();
            if (rr > 0.5f)
            {
                p.MoveTo(new Vector2(0.5f, Mathf.Min(h - rr - 0.5f, h * 0.45f)));
                p.LineTo(new Vector2(0.5f, 0.5f + rr));
                p.Arc(new Vector2(0.5f + rr, 0.5f + rr), rr, Angle.Degrees(180), Angle.Degrees(270));
                p.LineTo(new Vector2(w - 0.5f - rr, 0.5f));
                p.Arc(new Vector2(w - 0.5f - rr, 0.5f + rr), rr, Angle.Degrees(-90), Angle.Degrees(-30));
            }
            else
            {
                p.MoveTo(new Vector2(0.5f, h * 0.45f));
                p.LineTo(new Vector2(0.5f, 0.5f));
                p.LineTo(new Vector2(w - 0.5f, 0.5f));
            }
            p.Stroke();

            p.lineWidth = 1f;
            p.strokeColor = DesignTokens.GlassRimBottom;
            p.BeginPath();
            if (rr > 0.5f)
            {
                p.MoveTo(new Vector2(w - 0.5f, Mathf.Max(0.5f + rr, h * 0.6f)));
                p.LineTo(new Vector2(w - 0.5f, h - 0.5f - rr));
                p.Arc(new Vector2(w - 0.5f - rr, h - 0.5f - rr), rr, Angle.Degrees(0), Angle.Degrees(90));
                p.LineTo(new Vector2(Mathf.Max(0.5f + rr, w * 0.35f), h - 0.5f));
            }
            else
            {
                p.MoveTo(new Vector2(w - 0.5f, h * 0.6f));
                p.LineTo(new Vector2(w - 0.5f, h - 0.5f));
                p.LineTo(new Vector2(w * 0.35f, h - 0.5f));
            }
            p.Stroke();
        }

        static void StrokeRect(Painter2D p, float x, float y, float w, float h, float r, float lineWidth, Color color)
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
