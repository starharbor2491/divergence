using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Evo.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>Short, opt-in visual regression tour. Synthetic history avoids long CPU simulation runs.</summary>
    public static class UxSmokeTour
    {
        static readonly BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;

        public static IEnumerator Run(AppController app, string directory)
        {
            Application.targetFrameRate = 30;
            var stack = new Stack<IEnumerator>();
            stack.Push(Tour(app, directory));
            while (stack.Count > 0)
            {
                bool next;
                object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception error)
                {
                    Debug.LogError("UX-SMOKE-FAILED " + error);
                    Application.Quit(1);
                    yield break;
                }
                if (!next) { stack.Pop(); continue; }
                if (current is IEnumerator child && !(current is CustomYieldInstruction)) { stack.Push(child); continue; }
                yield return current;
            }
            Debug.Log("UX-SMOKE-PASS: rendering and bounded readouts, readable Tree, confirmation and minimum size");
            Application.Quit();
        }

        static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        static void Click(VisualElement element, Vector2 point, int count)
        {
            using (PointerDownEvent down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, mousePosition = point, button = 0, clickCount = count }))
                element.SendEvent(down);
            using (PointerUpEvent up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, mousePosition = point, button = 0, clickCount = count }))
                element.SendEvent(up);
        }

        static IEnumerator CheckSlider(UiRoot ui)
        {
            EvoSlider slider = Ui.Overlay.parent.Q<SetupScreen>().Q<EvoSlider>();
            VisualElement area = (VisualElement)typeof(EvoSlider).GetField("area", Hidden).GetValue(slider);
            VisualElement thumb = (VisualElement)typeof(EvoSlider).GetField("thumb", Hidden).GetValue(slider);
            slider.Value = 0.5f;
            yield return Frames(2);
            Vector2 trackPoint = area.LocalToWorld(new Vector2(area.layout.width * 0.8f, area.layout.height * 0.5f));
            Click(area, trackPoint, 1);
            yield return Frames(2);
            float trackValue = slider.Value;
            Click(area, trackPoint, 2);
            Require(Mathf.Approximately(slider.Value, trackValue), "Double track click reset the slider");
            slider.Value = 0.55f;
            yield return Frames(2);
            Click(area, thumb.worldBound.center, 1);
            yield return Frames(2);
            Click(area, thumb.worldBound.center, 2);
            Require(Mathf.Approximately(slider.Value, 1f), "Double thumb click did not reset the slider");
        }
        static IEnumerator Shot(string directory, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), shot.EncodeToPNG());
            UnityEngine.Object.Destroy(shot);
        }

        static void Fixture(Simulation sim)
        {
            sim.StepCount = (long)Math.Ceiling(59 / (double)sim.P.Dt);
            for (int i = 2; i <= 30; i++)
                sim.Species.Records.Add(new SpeciesRecord { Id = i, ParentId = i / 2, Name = "Species " + i,
                    BirthTime = i, PeakPopulation = 200 - i, LastPopulation = 5, ColorR = 0.6f + i * 0.01f, ColorG = 0.8f, ColorB = 0.9f });
            foreach (SpeciesRecord record in sim.Species.Records) record.CurrentPopulation = 0;
            for (int i = 0; i < sim.Cells.AliveCount; i++)
            {
                sim.Cells.Alive[i].SpeciesId = i % 30 + 1;
                sim.Species.Get(i % 30 + 1).CurrentPopulation++;
            }
            sim.Stats.Samples.Clear();
            for (int i = 0; i < 60; i++)
            {
                var sample = new Sample { Time = i, Count = sim.Cells.AliveCount, MeanGeneration = i < 30 ? i * 0.4f : 9f + (i - 30) * 0.2f };
                for (int t = 0; t < Traits.ChartCount; t++)
                {
                    sample.TraitMean[t] = Traits.FounderMean[t] + 0.03f * Traits.Range(t) * Mathf.Sin(i * 0.1f);
                    sample.TraitSd[t] = Traits.Range(t) * 0.02f;
                    sample.Bins[t * TraitBins.Count + 2] = sample.Count / 2;
                    sample.Bins[t * TraitBins.Count + 3] = sample.Count - sample.Count / 2;
                }
                sample.SpeciesIds = new int[30];
                sample.SpeciesPops = new int[30];
                for (int s = 0; s < 30; s++) { sample.SpeciesIds[s] = s + 1; sample.SpeciesPops[s] = sim.Species.Get(s + 1).CurrentPopulation; }
                sim.Stats.Samples.Add(sample);
            }
            sim.Events.Log.Add(new EventRecord { Time = 20, Kind = EventKind.HeatWave, Radius = 20, Duration = 90, Intensity = 0.3f });
            sim.Events.Log.Add(new EventRecord { Time = 20, Kind = EventKind.Famine, Radius = 15, Duration = 90 });
            for (int i = 0; i < 100; i++)
                sim.Events.Log.Add(new EventRecord { Time = 20, Kind = EventKind.FoodBloom, Radius = 15, Duration = 90 });
        }

        static IEnumerator CheckGroupedEvents(ChartElement chart, string directory)
        {
            var chips = (List<VisualElement>)typeof(ChartElement).GetField("chips", Hidden).GetValue(chart);
            VisualElement chip = chips[0];
            using (PointerMoveEvent move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = chip.worldBound.center }))
            using (PointerEnterEvent enter = PointerEnterEvent.GetPooled(move))
            { enter.target = chip; chip.SendEvent(enter); }
            yield return new WaitForSecondsRealtime(0.5f);
            var card = (TooltipCard)typeof(Tooltips).GetField("readout", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Require(card.IsShown(), "Grouped Event card did not open");
            var scroll = card.Q<ScrollView>();
            Require(scroll.verticalScroller.highValue > 0, "Grouped Events cannot scroll");
            using (PointerMoveEvent move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = card.worldBound.center }))
            using (PointerLeaveEvent leave = PointerLeaveEvent.GetPooled(move))
            { leave.target = chip; chip.SendEvent(leave); }
            using (PointerMoveEvent move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = card.worldBound.center }))
            using (PointerEnterEvent enter = PointerEnterEvent.GetPooled(move))
            { enter.target = card; card.SendEvent(enter); }
            yield return new WaitForSecondsRealtime(0.5f);
            Require(card.IsShown(), "Grouped Event card closed while reading");
            using (WheelEvent wheel = WheelEvent.GetPooled(new Event { type = EventType.ScrollWheel, mousePosition = card.worldBound.center, delta = new Vector2(0, 1000) }))
                scroll.SendEvent(wheel);
            yield return Frames(3);
            Require(scroll.scrollOffset.y > 0, "Wheel did not scroll the grouped Events");
            Rect bounds = card.worldBound, window = Ui.Overlay.worldBound;
            Require(bounds.yMin >= window.yMin && bounds.yMax <= window.yMax + 1, "Grouped Event card leaves the window");
            yield return Shot(directory, "10-grouped-events-scrolled");
            Tooltips.Hide();
        }

        static IEnumerator Tour(AppController app, string directory)
        {
            Directory.CreateDirectory(directory);
            yield return Frames(6);
            yield return Shot(directory, "01-setup");
            yield return CheckSlider(app.Ui);
            app.StartRun(app.SetupDraft);
            app.TogglePause();
            Fixture(app.Sim);
            app.Ui.OnRunStarted();
            yield return Frames(8);
            yield return Shot(directory, "02-traits");
            app.Ui.SelectTab(1);
            yield return Frames(4);
            SpeciesTab species = app.Ui.WorldRegion.parent.Q<SpeciesTab>();
            Require(species.Query<LegendRow>().ToList().Exists(row => row.Tag == 2 && !row.Q<Label>().text.Contains("(in Other)")),
                "Individually plotted Species was labelled Other before first refresh");
            SpeciesChart chart = app.Ui.WorldRegion.parent.Q<SpeciesChart>();
            Require(chart != null, "Species chart missing");
            yield return CheckGroupedEvents(chart, directory);
            Vector2 pointer = chart.LocalToWorld(new Vector2(chart.layout.width * 0.5f, chart.layout.height * 0.6f));
            using (PointerMoveEvent move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = pointer }))
                chart.SendEvent(move);
            yield return Frames(4);
            TooltipCard readout = (TooltipCard)typeof(ChartElement).GetField("card", Hidden).GetValue(chart);
            Require(readout.parent == Ui.Overlay, "Readout is not in the window overlay");
            Require(readout.IsShown(), "Species hover readout did not open");
            Rect bounds = readout.worldBound, window = Ui.Overlay.worldBound;
            Require(bounds.width > 0 && bounds.height > 0 && bounds.xMin >= window.xMin && bounds.xMax <= window.xMax + 1 &&
                bounds.yMin >= window.yMin && bounds.yMax <= window.yMax + 1, "Readout leaves the window: " + bounds);
            yield return Shot(directory, "03-species-hover");
            Rect plot = (Rect)typeof(ChartElement).GetField("plot", Hidden).GetValue(chart);
            pointer = chart.LocalToWorld(new Vector2(plot.xMax - 1, plot.center.y));
            using (PointerMoveEvent move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = pointer }))
                chart.SendEvent(move);
            app.Sim.P.MaxSamples = 20;
            app.Sim.Stats.Sample(app.Sim.Cells, app.Sim.Species, app.Sim.Time);
            chart.Refresh();
            yield return Frames(3);
            int hovered = (int)typeof(ChartElement).GetField("hoverIndex", Hidden).GetValue(chart);
            Require(hovered >= 0 && hovered < app.Sim.Stats.Samples.Count && readout.IsShown(),
                "Stationary hover did not recover across decimation");
            app.Ui.SelectTab(2);
            yield return Frames(4);
            TreeChart tree = app.Ui.WorldRegion.parent.Q<TreeChart>();
            Require(tree.layout.height >= 30 * DesignTokens.TreeRowHeight, "Tree rows were squeezed");
            yield return Shot(directory, "04-tree");
            app.Ui.SelectTab(3);
            yield return Frames(4);
            yield return Shot(directory, "05-stats");
            app.GoToSetup();
            yield return Frames(3);
            Require(app.Sim != null, "Menu discarded the Run without confirmation");
            yield return Shot(directory, "06-discard-confirmation");
            app.Ui.HandleEscape();
            app.Ui.OpenEventDialog(EventKind.ToxinSpill);
            yield return Frames(3);
            EventDialog dialog = (EventDialog)typeof(UiRoot).GetField("eventDialog", Hidden).GetValue(app.Ui);
            EvoCheckbox whole = dialog.Q<EvoCheckbox>();
            whole.Value = true;
            ((Action<bool>)typeof(EvoCheckbox).GetField("Changed", Hidden).GetValue(whole))(true);
            Require(dialog.Q<EvoSlider>().Enabled, "Whole World Toxin quantity is disabled");
            yield return Shot(directory, "07-whole-world-toxin");
            app.Ui.CloseAll();
            app.Ui.OpenHelp();
            HelpScreen help = (HelpScreen)typeof(UiRoot).GetField("help", Hidden).GetValue(app.Ui);
            typeof(HelpScreen).GetMethod("Select", Hidden).Invoke(help, new object[] { 5 });
            yield return Frames(4);
            yield return Shot(directory, "08-reproduction-help");
            Screen.SetResolution(1200, 675, false);
            yield return Frames(12);
            Require(app.Ui.WindowTooSmall, "Minimum-window warning did not activate");
            yield return Shot(directory, "09-minimum-size");
        }
    }
}
