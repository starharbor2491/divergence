using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Every icon of DESIGN.md, drawn on a 24 by 24 grid with Painter2D. There are no image files.</summary>
    public static class Icons
    {
        public static readonly string[] Names =
        {
            "play", "pause", "speed-1", "speed-2", "speed-3", "speed-4", "event", "temperature", "save", "load", "csv",
            "screenshot", "help", "panel", "home", "close", "chevron-down", "check", "random", "restart",
            "tab-traits", "tab-species", "tab-tree", "tab-stats",
            "heat-wave", "ice-age", "famine", "food-bloom", "toxin-spill",
            "status-good", "status-warning", "status-critical", "status-serious", "glyph-branch",
            "fit", "notes"
        };

        /// <summary>Draws an icon with its top left corner at the origin, scaled so that 24 units equal size.</summary>
        public static void Draw(Painter2D p, string name, Color color, float size, bool on = false)
        {
            var d = new Pen(p, color, size / 24f);
            switch (name)
            {
                case "play": d.FillPoly(true, 8, 5, 19, 12, 8, 19); break;
                case "pause": d.FillRound(7, 5, 3.5f, 14, 1.2f); d.FillRound(13.5f, 5, 3.5f, 14, 1.2f); break;
                case "speed-1": d.Line(false, 9, 6, 15, 12, 9, 18); break;
                case "speed-2": d.Line(false, 6, 6, 12, 12, 6, 18); d.Line(false, 12, 6, 18, 12, 12, 18); break;
                case "speed-3": d.Line(false, 4, 6, 9, 12, 4, 18); d.Line(false, 9, 6, 14, 12, 9, 18); d.Line(false, 14, 6, 19, 12, 14, 18); break;
                case "speed-4":
                    d.Line(false, 3, 6, 7, 12, 3, 18); d.Line(false, 8, 6, 12, 12, 8, 18);
                    d.Line(false, 13, 6, 17, 12, 13, 18); d.Line(false, 18, 6, 22, 12, 18, 18);
                    break;
                case "event": d.Line(false, 6, 4, 6, 20); d.FillPoly(true, 6, 5, 18, 8.5f, 6, 12); break;
                case "temperature": d.Round(10, 3, 4, 13, 2); d.FillCircle(12, 17.5f, 3.5f); break;
                case "save": d.Line(false, 12, 4, 12, 14); d.Line(false, 8, 10, 12, 14, 16, 10); d.Line(false, 5, 15, 5, 20, 19, 20, 19, 15); break;
                case "load": d.Line(false, 12, 14, 12, 4); d.Line(false, 8, 8, 12, 4, 16, 8); d.Line(false, 5, 15, 5, 20, 19, 20, 19, 15); break;
                case "csv":
                    d.Round(4, 5, 16, 14, 2); d.Line(false, 4, 10, 20, 10); d.Line(false, 4, 14.5f, 20, 14.5f); d.Line(false, 10, 5, 10, 19);
                    break;
                case "screenshot":
                    d.Line(false, 4, 9, 4, 4, 9, 4); d.Line(false, 15, 4, 20, 4, 20, 9);
                    d.Line(false, 20, 15, 20, 20, 15, 20); d.Line(false, 9, 20, 4, 20, 4, 15); d.Circle(12, 12, 3.2f);
                    break;
                case "fit":
                    d.Line(false, 4, 9, 4, 4, 9, 4); d.Line(false, 15, 4, 20, 4, 20, 9);
                    d.Line(false, 20, 15, 20, 20, 15, 20); d.Line(false, 9, 20, 4, 20, 4, 15);
                    d.FillCircle(12, 12, 1.8f);
                    break;
                case "notes":
                    d.Circle(12, 12, 8.5f);
                    d.FillCircle(12, 8, 1.2f);
                    d.Line(false, 12, 11.5f, 12, 16.5f);
                    break;
                case "help": d.Circle(12, 12, 9); break;
                case "panel":
                    d.Round(3, 5, 18, 14, 2.5f); d.Line(false, 14.5f, 5, 14.5f, 19);
                    if (on) d.FillRound(14.5f, 5, 6.5f, 14, 1.5f);
                    break;
                case "home": d.Line(false, 4, 12, 12, 4, 20, 12); d.Line(false, 6, 11, 6, 20, 18, 20, 18, 11); break;
                case "close": d.Line(false, 6.5f, 6.5f, 17.5f, 17.5f); d.Line(false, 17.5f, 6.5f, 6.5f, 17.5f); break;
                case "chevron-down": d.Line(false, 7, 10, 12, 15, 17, 10); break;
                case "check": d.Line(false, 5, 12.5f, 10, 17.5f, 19, 7); break;
                case "random":
                    d.Round(4, 4, 16, 16, 3.5f); d.FillCircle(9, 9, 1.4f); d.FillCircle(15, 15, 1.4f); d.FillCircle(12, 12, 1.4f);
                    break;
                case "restart": d.Arc(12, 12, 7, -60, 240); d.Line(false, 17, 3, 17.5f, 8, 12.5f, 8); break;
                case "tab-traits": d.Line(false, 4, 8, 20, 8); d.Line(false, 4, 16, 20, 16); d.FillCircle(9, 8, 2.5f); d.FillCircle(15, 16, 2.5f); break;
                case "tab-species": d.Circle(9, 12, 5); d.Circle(15, 12, 5); break;
                case "tab-tree": d.Branch(); break;
                case "tab-stats": d.FillRound(5, 12, 3, 7, 1); d.FillRound(10.5f, 6, 3, 13, 1); d.FillRound(16, 9, 3, 10, 1); break;
                case "heat-wave":
                    d.Circle(12, 12, 4);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        d.Line(false, 12 + 7 * Mathf.Cos(a), 12 + 7 * Mathf.Sin(a), 12 + 10 * Mathf.Cos(a), 12 + 10 * Mathf.Sin(a));
                    }
                    break;
                case "ice-age": d.Line(false, 12, 3, 12, 21); d.Line(false, 4.2f, 7.5f, 19.8f, 16.5f); d.Line(false, 19.8f, 7.5f, 4.2f, 16.5f); break;
                case "famine": d.FillCircle(12, 12, 3.5f); d.Line(false, 5, 19, 19, 5); break;
                case "food-bloom": d.FillCircle(8, 14, 2.6f); d.FillCircle(15, 15, 2.6f); d.FillCircle(12, 8, 2.6f); break;
                case "toxin-spill": d.Drop(); break;
                case "status-good": d.Circle(12, 12, 9); d.Line(false, 8, 12.5f, 11, 15.5f, 16, 9); break;
                case "status-warning": d.Line(true, 12, 4, 21, 19, 3, 19); d.Line(false, 12, 10, 12, 14); d.FillCircle(12, 16.5f, 1); break;
                case "status-critical": d.Circle(12, 12, 9); d.Line(false, 9, 9, 15, 15); d.Line(false, 15, 9, 9, 15); break;
                case "status-serious": d.Circle(12, 12, 9); d.Line(false, 12, 7, 12, 13); d.FillCircle(12, 16.5f, 1); break;
                case "glyph-branch": d.Branch(); break;
            }
        }

        struct Pen
        {
            readonly Painter2D p;
            readonly Color color;
            readonly float k;

            public Pen(Painter2D p, Color color, float k)
            {
                this.p = p; this.color = color; this.k = k;
                p.lineWidth = 1.9f * k;
                p.lineCap = LineCap.Round;
                p.lineJoin = LineJoin.Round;
                p.strokeColor = color;
                p.fillColor = color;
            }

            Vector2 V(float x, float y) { return new Vector2(x * k, y * k); }

            public void Line(bool closed, params float[] xy)
            {
                p.BeginPath();
                p.MoveTo(V(xy[0], xy[1]));
                for (int i = 2; i < xy.Length; i += 2) p.LineTo(V(xy[i], xy[i + 1]));
                if (closed) p.ClosePath();
                p.Stroke();
            }

            public void FillPoly(bool close, params float[] xy)
            {
                p.BeginPath();
                p.MoveTo(V(xy[0], xy[1]));
                for (int i = 2; i < xy.Length; i += 2) p.LineTo(V(xy[i], xy[i + 1]));
                p.ClosePath();
                p.Fill();
                p.Stroke();
            }

            public void Circle(float cx, float cy, float r)
            {
                p.BeginPath();
                p.Arc(V(cx, cy), r * k, Angle.Degrees(0), Angle.Degrees(360));
                p.Stroke();
            }

            public void FillCircle(float cx, float cy, float r)
            {
                p.BeginPath();
                p.Arc(V(cx, cy), r * k, Angle.Degrees(0), Angle.Degrees(360));
                p.Fill();
            }

            void RoundPath(float x, float y, float w, float h, float r)
            {
                r = Mathf.Min(r, Mathf.Min(w, h) * 0.5f);
                p.BeginPath();
                if (r <= 0.01f)
                {
                    p.MoveTo(V(x, y)); p.LineTo(V(x + w, y)); p.LineTo(V(x + w, y + h)); p.LineTo(V(x, y + h)); p.ClosePath();
                    return;
                }
                p.MoveTo(V(x + r, y));
                p.LineTo(V(x + w - r, y));
                p.Arc(V(x + w - r, y + r), r * k, Angle.Degrees(-90), Angle.Degrees(0));
                p.LineTo(V(x + w, y + h - r));
                p.Arc(V(x + w - r, y + h - r), r * k, Angle.Degrees(0), Angle.Degrees(90));
                p.LineTo(V(x + r, y + h));
                p.Arc(V(x + r, y + h - r), r * k, Angle.Degrees(90), Angle.Degrees(180));
                p.LineTo(V(x, y + r));
                p.Arc(V(x + r, y + r), r * k, Angle.Degrees(180), Angle.Degrees(270));
                p.ClosePath();
            }

            public void Round(float x, float y, float w, float h, float r) { RoundPath(x, y, w, h, r); p.Stroke(); }
            public void FillRound(float x, float y, float w, float h, float r) { RoundPath(x, y, w, h, r); p.Fill(); }

            public void Arc(float cx, float cy, float r, float a0, float a1)
            {
                p.BeginPath();
                p.Arc(V(cx, cy), r * k, Angle.Degrees(a0), Angle.Degrees(a1));
                p.Stroke();
            }

            public void Drop()
            {
                p.BeginPath();
                p.MoveTo(V(12, 3));
                p.BezierCurveTo(V(12, 3), V(5, 11), V(5, 15));
                p.Arc(V(12, 15), 7 * k, Angle.Degrees(180), Angle.Degrees(0), ArcDirection.CounterClockwise);
                p.BezierCurveTo(V(19, 11), V(12, 3), V(12, 3));
                p.ClosePath();
                p.Stroke();
            }

            public void Branch()
            {
                p.lineWidth = 2.2f * k;
                p.BeginPath();
                p.MoveTo(V(4, 12)); p.LineTo(V(10, 12));
                p.BezierCurveTo(V(14, 12), V(15, 6), V(20, 6));
                p.Stroke();
                p.BeginPath();
                p.MoveTo(V(10, 12));
                p.BezierCurveTo(V(14, 12), V(15, 18), V(20, 18));
                p.Stroke();
                FillCircle(4, 12, 2); FillCircle(20, 6, 2); FillCircle(20, 18, 2);
            }
        }
    }

    /// <summary>A VisualElement that draws one named icon in its current Tint. Change Tint or State and it repaints.</summary>
    public class IconElement : VisualElement
    {
        string icon;
        Color tint = DesignTokens.Text2;
        bool on;
        float size;
        Label questionMark;

        public IconElement(string icon, float size = 18f)
        {
            this.icon = icon;
            this.size = size;
            pickingMode = PickingMode.Ignore;
            style.width = size;
            style.height = size;
            style.flexShrink = 0;
            generateVisualContent += OnGenerate;
            if (icon == "help")
            {
                questionMark = new Label("?");
                questionMark.pickingMode = PickingMode.Ignore;
                Ui.Type(questionMark, FontRole.Bold, DesignTokens.TypeLabelSize);
                questionMark.style.position = Position.Absolute;
                questionMark.style.left = 0; questionMark.style.top = 0;
                questionMark.style.width = size; questionMark.style.height = size;
                questionMark.style.unityTextAlign = TextAnchor.MiddleCenter;
                questionMark.style.paddingTop = 0; questionMark.style.paddingBottom = 0;
                questionMark.style.marginTop = 0; questionMark.style.marginBottom = 0;
                Add(questionMark);
            }
        }

        public string Icon
        {
            get { return icon; }
            set { if (icon != value) { icon = value; MarkDirtyRepaint(); } }
        }

        public Color Tint
        {
            get { return tint; }
            set
            {
                if (tint == value) return;
                tint = value;
                if (questionMark != null) questionMark.style.color = value;
                MarkDirtyRepaint();
            }
        }

        public bool On
        {
            get { return on; }
            set { if (on != value) { on = value; MarkDirtyRepaint(); } }
        }

        void OnGenerate(MeshGenerationContext ctx)
        {
            Icons.Draw(ctx.painter2D, icon, tint, size, on);
        }
    }
}
