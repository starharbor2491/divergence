using System;
using System.Collections;
using System.IO;
using Evo.Core;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Development aid, only active when the player is started with "-evoShots <folder>": plays a scripted tour of the screens,
    /// fast-forwarding the Run and saving screenshots, so the interface can be checked without a person at the keyboard.
    /// </summary>
    public static class DevHarness
    {
        public static void TryStart(AppController app)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-evoShots") { app.StartCoroutine(Tour(app, args[i + 1])); return; }
        }

        static IEnumerator Shot(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), t.EncodeToPNG());
            UnityEngine.Object.Destroy(t);
        }

        static void FastForward(AppController app, double seconds)
        {
            int steps = (int)(seconds / app.Sim.P.Dt);
            for (int i = 0; i < steps && !app.Sim.Extinct; i++) app.Sim.Step();
        }

        static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

        static IEnumerator Tour(AppController app, string dir)
        {
            Directory.CreateDirectory(dir);
            yield return Frames(10);
            yield return Shot(dir, "01-setup");
            app.StartRun(app.SetupDraft);
            app.TogglePause();
            yield return Frames(10);
            FastForward(app, 20);
            yield return Frames(5);
            yield return Shot(dir, "02-run-20s");
            FastForward(app, 280);
            app.ToggleTint();
            app.SelectAt(new Vector2(app.Sim.Cells.Alive[0].X, app.Sim.Cells.Alive[0].Y));
            yield return Frames(70);
            yield return Shot(dir, "03-run-5min-traits");
            app.Ui.SelectTab(1);
            yield return Frames(70);
            yield return Shot(dir, "04-species");
            app.Ui.SelectTab(2);
            yield return Frames(70);
            yield return Shot(dir, "05-tree");
            app.Ui.SelectTab(3);
            yield return Frames(70);
            yield return Shot(dir, "06-stats");
            app.Ui.SelectTab(0);
            app.Ui.OpenEventDialog(EventKind.HeatWave);
            yield return Frames(5);
            yield return Shot(dir, "07-event-dialog");
            app.Ui.CloseAll();
            app.Ui.OpenHelp();
            yield return Frames(5);
            yield return Shot(dir, "08-help");
            app.Ui.CloseAll();
            app.Sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, X = 40, Y = 40, Radius = 25, Intensity = 0.3f, Duration = 90f });
            FastForward(app, 60);
            yield return Frames(70);
            yield return Shot(dir, "09-after-heatwave");
            Debug.Log("EVO-SHOTS-DONE");
            Application.Quit();
        }
    }
}
