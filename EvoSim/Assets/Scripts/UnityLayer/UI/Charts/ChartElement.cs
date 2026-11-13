using System;
using System.Collections.Generic;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Shared Liquid Glass chart anatomy: recessed rounded surface, gridlines, tabular mono axis labels, Event chips,
    /// crosshair and hover read-out. Redraws once per sample or real second, never every frame.
    /// </summary>
    public abstract class ChartElement : VisualElement
    {
        public const float MarginLeft = 56f, MarginRight = 16f, MarginTop = 28f, MarginBottom = 36f;

        protected Rect plot;
        protected float xMin, xMax = 1f, yMin, yMax = 1f;
        protected readonly List<float> xTicks = new List<float>();
        protected readonly List<float> yTicks = new List<float>();
        protected Func<float, string> xFormat = v => v.ToString("0");
        protected Func<float, string> yFormat = v => v.ToString("0");
        protected int hoverIndex = -1;
        protected bool showEventMarkers = true;
        protected Func<double, float> eventTimeToX;

        readonly List<Label> labels = new List<Label>();
        int labelsUsed;
        readonly TooltipCard card = new TooltipCard(true);
        readonly List<VisualElement> chips = new List<VisualElement>();
        readonly List<EventRecord> markers = new List<EventRecord>();
        readonly List<float> markerX = new List<float>();
        Vector2 pointer;
        Vector2 panelPointer;
        bool pointerInside, readingCard;
        static readonly HashSet<ChartElement> charts = new HashSet<ChartElement>();

        protected ChartElement()
        {
            style.flexShrink = 0;
            style.backgroundColor = DesignTokens.ChartSurface;
            this.Round(DesignTokens.RadiusCard);
            this.Border(DesignTokens.Border, DesignTokens.BorderWidth);
            generateVisualContent += OnGenerate;
            card.Show(false);
            RegisterCallback<AttachToPanelEvent>(e => charts.Add(this));
            RegisterCallback<DetachFromPanelEvent>(e => { charts.Remove(this); InvalidateHover(true); card.RemoveFromHierarchy(); });
            RegisterCallback<GeometryChangedEvent>(e => Redraw());
            RegisterCallback<PointerMoveEvent>(OnMove);
            RegisterCallback<PointerLeaveEvent>(e =>
            {
                pointerInside = false;
                if (!card.worldBound.Contains(e.position)) SetHover(-1);
            });
            card.RegisterCallback<PointerEnterEvent>(e => readingCard = true);
            card.RegisterCallback<PointerLeaveEvent>(e => { readingCard = false; if (!worldBound.Contains(e.position)) SetHover(-1); });
            RegisterCallback<PointerDownEvent>(e => { if (IsValidIndex(hoverIndex) && HoverClicked != null) HoverClicked(hoverIndex); });
        }

        // ------------------------------------------------------------ to implement
        protected abstract void ComputeAxes();
        protected abstract void DrawData(Painter2D p);
        protected virtual int SnapIndex(Vector2 local) { return -1; }
        protected virtual bool UseCrosshair { get { return true; } }
        protected virtual void AddExtraLabels() { }
        public event Action<int> HoverClicked;
        protected virtual float IndexX(int index) { return 0f; }
        protected virtual void FillHover(int index, List<ReadoutRow> rows, out string title) { title = ""; }
        protected virtual void DrawHover(Painter2D p, int index) { }
        protected virtual bool IsValidIndex(int index) { return index >= 0; }

        public static void HideReadouts()
        {
            foreach (ChartElement chart in charts) chart.InvalidateHover(true);
        }

        protected void InvalidateHover(bool forgetPointer = false)
        {
            if (forgetPointer) { pointerInside = false; readingCard = false; }
            hoverIndex = -1;
            card.Show(false);
        }

        // ------------------------------------------------------------ coordinates
        protected float X(float v) { return plot.xMin + (v - xMin) / Mathf.Max(1e-6f, xMax - xMin) * plot.width; }
        protected float Y(float v) { return plot.yMax - (v - yMin) / Mathf.Max(1e-6f, yMax - yMin) * plot.height; }

        public void Redraw()
        {
            float w = layout.width, h = layout.height;
            if (float.IsNaN(w) || w < 10f || h < 10f) return;
            plot = new Rect(MarginLeft, MarginTop, Mathf.Max(10f, w - MarginLeft - MarginRight), Mathf.Max(10f, h - MarginTop - MarginBottom));
            xTicks.Clear(); yTicks.Clear();
            ComputeAxes();
            LayoutLabels();
            LayoutMarkers();
            if (pointerInside || readingCard)
            {
                pointer = this.WorldToLocal(panelPointer);
                SetHover(plot.Contains(pointer) ? SnapIndex(pointer) : -1, true);
            }
            MarkDirtyRepaint();
        }

        // ------------------------------------------------------------ axis labels
        protected Label GetLabel(float width = 60f)
        {
            if (labelsUsed == labels.Count)
            {
                Label l = Ui.MonoCaption("", DesignTokens.Text3);
                l.style.position = Position.Absolute;
                l.style.width = 60;
                labels.Add(l);
                Add(l);
            }
            Label label = labels[labelsUsed++];
            label.style.width = width;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.color = DesignTokens.Text3;
            label.Show(true);
            return label;
        }

        void LayoutLabels()
        {
            labelsUsed = 0;
            foreach (float t in yTicks)
            {
                Label l = GetLabel();
                l.text = yFormat(t);
                l.style.unityTextAlign = TextAnchor.MiddleRight;
                l.style.left = MarginLeft - 8 - 60;
                l.style.top = Y(t) - DesignTokens.TypeCaptionLine * 0.5f;
            }
            foreach (float t in xTicks)
            {
                Label l = GetLabel();
                l.text = xFormat(t);
                l.style.unityTextAlign = TextAnchor.UpperCenter;
                l.style.left = X(t) - 30;
                l.style.top = plot.yMax + 8;
            }
            AddExtraLabels();
            for (int i = labelsUsed; i < labels.Count; i++) labels[i].Show(false);
        }

        // ------------------------------------------------------------ Event markers
        public void SetEvents(IList<EventRecord> events)
        {
            markers.Clear();
            for (int i = 0; i < events.Count; i++) markers.Add(events[i]);
        }

        void LayoutMarkers()
        {
            markerX.Clear();
            int used = 0;
            if (showEventMarkers && eventTimeToX != null)
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    float xv = eventTimeToX(markers[i].Time);
                    if (float.IsNaN(xv) || xv < xMin - 1e-4f || xv > xMax + 1e-4f) { continue; }
                    float px = X(xv);
                    markerX.Add(px);
                }
                var order = new List<int>();
                for (int i = 0; i < markers.Count; i++)
                {
                    float xv = eventTimeToX(markers[i].Time);
                    if (!float.IsNaN(xv) && xv >= xMin && xv <= xMax) order.Add(i);
                }
                order.Sort((a, b) => X(eventTimeToX(markers[a].Time)).CompareTo(X(eventTimeToX(markers[b].Time))));
                for (int i = 0; i < order.Count;)
                {
                    var group = new List<EventRecord>();
                    float firstX = X(eventTimeToX(markers[order[i]].Time)), lastX = firstX;
                    do
                    {
                        lastX = X(eventTimeToX(markers[order[i]].Time));
                        group.Add(markers[order[i++]]);
                    }
                    while (i < order.Count && X(eventTimeToX(markers[order[i]].Time)) - lastX < DesignTokens.EventChipSize + DesignTokens.EventChipGap);
                    VisualElement chip = GetChip(used++);
                    chip.style.left = Mathf.Clamp((firstX + lastX) * 0.5f - DesignTokens.EventChipSize * 0.5f, 2, layout.width - DesignTokens.EventChipSize - 2);
                    chip.style.top = DesignTokens.Space1;
                    chip.userData = group;
                    ((Label)chip[0]).text = group.Count == 1 ? EventInfo.Letters[(int)group[0].Kind] : (group.Count <= 9 ? group.Count.ToString() : "+");
                    chip.Show(true);
                }
            }
            for (int i = used; i < chips.Count; i++) chips[i].Show(false);
        }

        VisualElement GetChip(int index)
        {
            while (chips.Count <= index)
            {
                var chip = new VisualElement();
                chip.style.position = Position.Absolute;
                chip.Size(DesignTokens.EventChipSize, DesignTokens.EventChipSize);
                chip.Round(DesignTokens.EventChipSize * 0.5f);
                chip.style.backgroundColor = DesignTokens.Bg3;
                chip.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
                chip.style.alignItems = Align.Center;
                chip.style.justifyContent = Justify.Center;
                Label letter = Ui.CaptionBold("", DesignTokens.AccentText);
                letter.style.unityTextAlign = TextAnchor.MiddleCenter;
                chip.Add(letter);
                VisualElement captured = chip;
                Tooltips.Attach(chip, () => EventGroupTip((List<EventRecord>)captured.userData), true);
                Tooltips.AttachedIds.Add("event-marker");
                chips.Add(chip);
                Add(chip);
            }
            return chips[index];
        }

        static TipContent EventGroupTip(List<EventRecord> group)
        {
            if (group.Count == 1) return EventTip(group[0]);
            var text = new System.Text.StringBuilder();
            foreach (EventRecord record in group)
            {
                TipContent tip = EventTip(record);
                if (text.Length > 0) text.Append('\n');
                text.Append(tip.Name).Append(": ").Append(tip.Body);
            }
            return new TipContent { Name = group.Count + " Events", Body = text.ToString() };
        }

        static TipContent EventTip(EventRecord r)
        {
            string detail = "At " + Ui.Mmss(r.Time) + ". " + (r.WholeWorld ? "Whole World." : "Radius " + Ui.F(r.Radius, 0) + ".");
            if (EventInfo.HasIntensity(r.Kind)) detail += " Intensity " + Ui.F(r.Intensity, 2) + ".";
            if (EventInfo.HasDuration(r.Kind)) detail += " Duration " + Ui.F(r.Duration, 0) + " s.";
            return new TipContent { Name = EventInfo.Names[(int)r.Kind], Body = detail, Key = null };
        }

        // ------------------------------------------------------------ hover
        void OnMove(PointerMoveEvent e)
        {
            panelPointer = e.position;
            pointerInside = true;
            pointer = this.WorldToLocal(e.position);
            if (pointer.x < plot.xMin - 4 || pointer.x > plot.xMax + 4 || pointer.y < plot.yMin - 4 || pointer.y > plot.yMax + 4)
            {
                SetHover(-1);
                return;
            }
            SetHover(SnapIndex(pointer));
        }

        void SetHover(int index, bool force = false)
        {
            if (!IsValidIndex(index)) index = -1;
            if (!force && index == hoverIndex) return;
            hoverIndex = index;
            if (index < 0)
            {
                card.Show(false);
            }
            else
            {
                var rows = new List<ReadoutRow>();
                string title;
                FillHover(index, rows, out title);
                VisualElement overlay = Ui.Overlay ?? this;
                if (card.parent != overlay) overlay.Add(card);
                card.BringToFront();
                card.SetRows(title, rows);
                card.Show(true);
                card.PlaceNear(overlay.WorldToLocal(this.LocalToWorld(new Vector2(UseCrosshair ? IndexX(index) : pointer.x, pointer.y))));
            }
            MarkDirtyRepaint();
        }

        // ------------------------------------------------------------ drawing
        protected static void Segment(Painter2D p, Vector2 a, Vector2 b, Color color, float width)
        {
            p.lineWidth = width;
            p.strokeColor = color;
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.Stroke();
        }

        void OnGenerate(MeshGenerationContext ctx)
        {
            Painter2D p = ctx.painter2D;
            if (plot.width < 10f) return;
            p.lineCap = LineCap.Round;
            p.lineJoin = LineJoin.Round;
            foreach (float t in yTicks) Segment(p, new Vector2(plot.xMin, Y(t)), new Vector2(plot.xMax, Y(t)), DesignTokens.ChartGrid, 1f);
            Segment(p, new Vector2(plot.xMin, plot.yMax), new Vector2(plot.xMax, plot.yMax), DesignTokens.ChartAxis, 1f);
            Segment(p, new Vector2(plot.xMin, plot.yMin), new Vector2(plot.xMin, plot.yMax), DesignTokens.ChartAxis, 1f);

            DrawData(p);

            for (int i = 0; i < markerX.Count; i++)
            {
                Color mc = DesignTokens.WithAlpha(DesignTokens.AccentText, 0.42f);
                for (float y = plot.yMin; y < plot.yMax; y += 8f)
                    Segment(p, new Vector2(markerX[i], y), new Vector2(markerX[i], Mathf.Min(y + 4f, plot.yMax)), mc, 1.1f);
            }

            if (IsValidIndex(hoverIndex) && UseCrosshair)
            {
                float hx = IndexX(hoverIndex);
                Segment(p, new Vector2(hx, plot.yMin), new Vector2(hx, plot.yMax), DesignTokens.WithAlpha(DesignTokens.Text1, 0.45f), 1.1f);
                DrawHover(p, hoverIndex);
            }
        }

        protected static void HoverDot(Painter2D p, Vector2 c, Color color)
        {
            p.fillColor = DesignTokens.WithAlpha(color, 0.28f);
            p.BeginPath(); p.Arc(c, 8f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
            p.fillColor = DesignTokens.Bg0;
            p.BeginPath(); p.Arc(c, 5.5f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
            p.fillColor = color;
            p.BeginPath(); p.Arc(c, 3.8f, Angle.Degrees(0), Angle.Degrees(360)); p.Fill();
        }

        // ------------------------------------------------------------ helpers
        public static void NiceTicks(float min, float max, int target, List<float> output)
        {
            output.Clear();
            float range = Mathf.Max(1e-6f, max - min);
            float raw = range / Mathf.Max(1, target);
            float mag = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(raw)));
            float norm = raw / mag;
            float step = (norm < 1.5f ? 1f : norm < 3.5f ? 2f : norm < 7.5f ? 5f : 10f) * mag;
            float start = Mathf.Ceil(min / step - 1e-4f) * step;
            for (float v = start; v <= max + step * 1e-4f && output.Count < 12; v += step) output.Add(Mathf.Abs(v) < step * 1e-4f ? 0f : v);
        }

        protected static int Nearest(IList<float> xs, int count, float x)
        {
            if (count == 0) return -1;
            int best = 0;
            for (int i = 1; i < count; i++)
                if (Mathf.Abs(xs[i] - x) <= Mathf.Abs(xs[best] - x)) best = i;
            return best;
        }

        public static string TimeTick(float seconds)
        {
            return Ui.Mmss(seconds);
        }
    }
}
