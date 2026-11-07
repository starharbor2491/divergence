using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Base of the four side panel tabs. Bind attaches a new Run; Refresh redraws once per real second.</summary>
    public abstract class PanelTab : VisualElement
    {
        protected Simulation sim;
        protected readonly AppController app;

        protected PanelTab(AppController app)
        {
            this.app = app;
            style.flexDirection = FlexDirection.Column;
            style.flexShrink = 0;
            this.Pad(DesignTokens.Space4);
        }

        public virtual void Bind(Simulation s) { sim = s; Refresh(); }

        public abstract void Refresh();

        /// <summary>Title in type-label with the explanation tooltip, caption underneath, then the chart (DESIGN 5.1).</summary>
        protected static VisualElement Section(string title, string unit, string tipId, string caption, VisualElement content)
        {
            VisualElement s = Ui.Column();
            s.style.flexShrink = 0;
            s.style.marginTop = DesignTokens.Space5;
            VisualElement head = Ui.Row();
            Label t = Ui.LabelText(title);
            t.pickingMode = PickingMode.Position;
            Tooltips.Attach(t, tipId);
            head.Add(t);
            if (!string.IsNullOrEmpty(unit))
            {
                Label u = Ui.Caption(unit);
                u.style.marginLeft = DesignTokens.Space2;
                head.Add(u);
            }
            s.Add(head);
            Label c = Ui.Caption(caption);
            c.style.marginTop = DesignTokens.Space1;
            c.style.marginBottom = DesignTokens.Space2;
            s.Add(c);
            s.Add(content);
            return s;
        }

        protected static void FixedHeight(VisualElement e, float height)
        {
            e.style.height = height;
            e.style.alignSelf = Align.Stretch;
        }
    }
}
