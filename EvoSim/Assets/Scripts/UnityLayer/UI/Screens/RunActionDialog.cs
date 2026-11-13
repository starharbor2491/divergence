using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public sealed class RunActionDialog : VisualElement
    {
        Action cancel;
        public bool IsOpen { get; private set; }

        public RunActionDialog()
        {
            this.Fill();
            style.backgroundColor = DesignTokens.Scrim;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            this.Show(false);
        }

        public void Open(string title, string action, Action discard, Action save, Action cancelled)
        {
            Close(false);
            cancel = cancelled;
            IsOpen = true;
            var card = new VisualElement();
            card.style.width = DesignTokens.DialogWidth;
            LiquidGlass.Attach(card, DesignTokens.RadiusPanel, GlassTone.Modal);
            card.Pad(DesignTokens.Space5);
            Add(card);

            VisualElement header = Ui.Row();
            header.Add(Ui.Heading(title, DesignTokens.Text1));
            header.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "confirm-cancel");
            close.Clicked += () => Close(true);
            header.Add(close);
            card.Add(header);

            Label note = Ui.Body("Your current run will end unless you save a snapshot first.", DesignTokens.Text2);
            note.style.marginTop = DesignTokens.Space3;
            note.style.marginBottom = DesignTokens.Space4;
            card.Add(note);

            VisualElement buttons = Ui.Row();
            buttons.style.justifyContent = Justify.FlexEnd;
            var back = new EvoButton(ButtonKind.Secondary, "Cancel", null, "confirm-cancel");
            back.Clicked += () => Close(true);
            buttons.Add(back);
            var download = new EvoButton(ButtonKind.Secondary, "Save", "save", "confirm-save");
            download.style.marginLeft = DesignTokens.Space2;
            download.Clicked += () =>
            {
                save();
                note.text = "Snapshot download requested. Check your browser downloads before discarding.";
            };
            buttons.Add(download);
            var proceed = new EvoButton(ButtonKind.Danger, action, null, "confirm-discard");
            proceed.style.marginLeft = DesignTokens.Space2;
            proceed.Clicked += () => { Close(false); discard(); };
            buttons.Add(proceed);
            card.Add(buttons);
            this.Show(true);
        }

        public void Close(bool notify = true)
        {
            Action callback = cancel;
            cancel = null;
            IsOpen = false;
            Clear();
            this.Show(false);
            Tooltips.Hide();
            if (notify && callback != null) callback();
        }
    }
}
