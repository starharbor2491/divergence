using UnityEngine;
using UnityEngine.InputSystem;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Reads the mouse and keyboard through the Input System (PLAN 12.4). Drag pans, wheel zooms, a click selects a Cell
    /// or places an armed Event. Keys: Space, 1 to 4, F, T, Tab, Escape. UI Toolkit gets its own events through the EventSystem.
    /// </summary>
    public sealed class InputRouter
    {
        public const float ClickSlopPixels = 4f;

        readonly AppController app;
        bool dragging, moved;
        Vector2 dragStart, dragLast;
        Texture2D crosshair;
        bool crosshairOn;

        public InputRouter(AppController app) { this.app = app; }

        public void Tick()
        {
            UiRoot ui = app.Ui;
            if (ui == null || ui.WindowTooSmall) return;
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (kb != null) HandleKeys(kb, ui);
            if (mouse == null || !app.HasRun || app.Mode == AppMode.Setup)
            {
                SetCrosshair(false);
                return;
            }
            HandleMouse(mouse, ui);
        }

        void HandleKeys(Keyboard kb, UiRoot ui)
        {
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (ui.HandleEscape()) return;
                if (app.Armed != null) app.CancelPlacement();
                else if (app.HasRun) app.ClearSelection();
                return;
            }
            if (!app.HasRun || app.Mode == AppMode.Setup || ui.ModalOpen) return;
            if (kb.spaceKey.wasPressedThisFrame) app.TogglePause();
            if (kb.digit1Key.wasPressedThisFrame) app.SetSpeed(0);
            if (kb.digit2Key.wasPressedThisFrame) app.SetSpeed(1);
            if (kb.digit3Key.wasPressedThisFrame) app.SetSpeed(2);
            if (kb.digit4Key.wasPressedThisFrame) app.SetSpeed(3);
            if (kb.fKey.wasPressedThisFrame) app.FitView();
            if (kb.tKey.wasPressedThisFrame) app.ToggleTint();
            if (kb.tabKey.wasPressedThisFrame) app.TogglePanel();
        }

        void HandleMouse(Mouse mouse, UiRoot ui)
        {
            Vector2 pos = mouse.position.ReadValue();
            bool blocked = ui.ModalOpen || ui.IsOverUi(pos) || !app.Cam.ViewportContains(pos);

            // Event placement preview follows the pointer.
            EventSpecView(pos, blocked);

            if (ui.ModalOpen) { dragging = false; return; }

            float scroll = mouse.scroll.ReadValue().y;
            if (!blocked && Mathf.Abs(scroll) > 0.001f) app.Cam.Zoom(ScrollToNotches(scroll), pos);

            if (mouse.leftButton.wasPressedThisFrame && !blocked)
            {
                dragging = true; moved = false;
                dragStart = dragLast = pos;
            }
            if (dragging && mouse.leftButton.isPressed)
            {
                if (!moved && (pos - dragStart).magnitude >= ClickSlopPixels) moved = true;
                if (moved)
                {
                    app.Cam.PanByPixels(pos - dragLast);
                    dragLast = pos;
                }
            }
            if (dragging && mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                if (!moved) Click(pos);
            }
        }

        static float ScrollToNotches(float y)
        {
            float a = Mathf.Abs(y);
            if (a >= 50f) return Mathf.Sign(y) * Mathf.Max(1f, Mathf.Round(a / 100f));
            return Mathf.Clamp(y, -3f, 3f);
        }

        void Click(Vector2 screen)
        {
            Vector2 world = app.Cam.ScreenToWorld(screen);
            if (app.Armed != null) app.PlaceArmedAt(world);
            else app.SelectAt(world);
        }

        void EventSpecView(Vector2 pos, bool blocked)
        {
            WorldView view = app.WorldView;
            bool armed = app.Armed != null && !app.Armed.WholeWorld;
            if (armed && !blocked)
            {
                view.PreviewActive = true;
                view.PreviewCentre = app.Cam.ScreenToWorld(pos);
                view.PreviewRadius = app.Armed.Radius;
                SetCrosshair(true);
            }
            else
            {
                view.PreviewActive = false;
                SetCrosshair(armed);
            }
        }

        void SetCrosshair(bool on)
        {
            if (on == crosshairOn) return;
            crosshairOn = on;
            if (on)
            {
                if (crosshair == null) crosshair = MakeCrosshair();
                Cursor.SetCursor(crosshair, new Vector2(crosshair.width * 0.5f, crosshair.height * 0.5f), CursorMode.Auto);
            }
            else Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        static Texture2D MakeCrosshair()
        {
            const int n = 25;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    bool line = (x == n / 2 || y == n / 2) && (Mathf.Abs(x - n / 2) > 2 || Mathf.Abs(y - n / 2) > 2);
                    px[y * n + x] = line ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            t.SetPixels32(px);
            t.Apply();
            return t;
        }
    }
}
