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
            this.Pad(DesignTokens.Space2, DesignTokens.Space4, DesignTokens.Space4, DesignTokens.Space4);
        }

        public virtual void Bind(Simulation s) { sim = s; Refresh(); }

        public abstract void Refresh();

        /// <summary>Section header with explanation tooltip, plain-English caption, and chart/content block.</summary>
        protected static VisualElement Section(string title, string unit, string tipId, string caption, VisualElement content)
        {
            VisualElement s = Ui.Column();
            s.style.flexShrink = 0;
            s.style.marginTop = DesignTokens.Space4;
            VisualElement head = Ui.Row();
            Label t = Ui.Subhead(title, DesignTokens.Text1);
            t.pickingMode = PickingMode.Position;
            Tooltips.Attach(t, tipId);
            head.Add(t);
            if (!string.IsNullOrEmpty(unit))
            {
                Label u = Ui.Caption(unit, DesignTokens.AccentText);
                u.style.marginLeft = DesignTokens.Space2;
                head.Add(u);
            }
            s.Add(head);
            Label c = Ui.Caption(caption, DesignTokens.Text2);
            c.style.marginTop = 3;
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
