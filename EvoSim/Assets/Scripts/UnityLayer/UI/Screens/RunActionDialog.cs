using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>A reversible safeguard before discarding or replacing the current Run.</summary>
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
            card.style.backgroundColor = DesignTokens.Bg1;
            card.Border(DesignTokens.BorderStrong, DesignTokens.BorderWidth);
            card.Round(DesignTokens.Radius);
            card.Pad(DesignTokens.Space5);
            Add(card);
            VisualElement header = Ui.Row();
            header.Add(Ui.Title(title));
            header.Add(Ui.Spacer());
            var close = new EvoButton(ButtonKind.Icon, null, "close", "confirm-cancel");
            close.Clicked += () => Close(true);
            header.Add(close);
            card.Add(header);
            Label note = Ui.Body("The current Run will be discarded. Save first if you want to keep it.");
            note.style.marginTop = note.style.marginBottom = DesignTokens.Space4;
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
                note.text = "Download requested. Check your browser's downloads before discarding the Run.";
            };
            buttons.Add(download);
            var proceed = new EvoButton(ButtonKind.Primary, action, null, "confirm-discard");
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
