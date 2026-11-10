using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Evo.Core;
using Evo.UnityLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Evo.Tests
{
    public class UxRegressionTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

        static FieldInfo Field(object target, string name)
        {
            for (Type t = target.GetType(); t != null; t = t.BaseType)
            {
                FieldInfo field = t.GetField(name, Hidden);
                if (field != null) return field;
            }
            throw new MissingFieldException(name);
        }

        static T Get<T>(object target, string name) { return (T)Field(target, name).GetValue(target); }
        static void Set(object target, string name, object value) { Field(target, name).SetValue(target, value); }

        static object Call(object target, string name, params object[] args)
        {
            for (Type t = target.GetType(); t != null; t = t.BaseType)
            {
                MethodInfo method = t.GetMethod(name, Hidden);
                if (method != null) return method.Invoke(target, args);
            }
            throw new MissingMethodException(name);
        }

        static List<ReadoutRow> Readout(ChartElement chart, int index)
        {
            var rows = new List<ReadoutRow>();
            Call(chart, "FillHover", index, rows, "");
            return rows;
        }

        [Test]
        public void TerminalExtinctionFinalizesSpeciesBeforeTheLastSampleAndCsv()
        {
            Simulation sim = TestHelpers.Quiet(3);
            foreach (Cell cell in sim.Cells.Alive.Take(sim.Cells.AliveCount)) cell.Energy = 0;
            sim.Step();
            Assert.IsTrue(sim.Extinct);
            Assert.AreEqual(0, sim.Species.LivingCount);
            Assert.AreEqual(0, sim.Species.Get(1).CurrentPopulation);
            Assert.AreEqual(sim.Time, sim.Species.Get(1).ExtinctTime);
            Sample final = sim.Stats.Samples.Last();
            Assert.AreEqual(0, final.Count);
            Assert.AreEqual(0, final.LivingSpecies);
            Assert.AreEqual(1, final.ExtinctSpecies);
            Assert.IsTrue(sim.Species.Get(1).LastPopulation > 0);
            string csv = CsvExport.Species(sim);
            StringAssert.DoesNotContain(",,", csv.Split('\n')[1]);
            Assert.AreEqual(sim.StateHash(), Simulation.FromSnapshot(sim.ToSnapshot()).StateHash());
        }

        [Test]
        public void LastPopulationIsNotReplacedByPeakAndSurvivesSaving()
        {
            Simulation sim = TestHelpers.Quiet(3);
            SpeciesRecord species = sim.Species.Get(1);
            species.PeakPopulation = 100;
            sim.Species.Check(sim.Cells, 2);
            sim.Species.Finish(3);
            Assert.AreEqual(3, species.LastPopulation);
            Assert.AreEqual(3, Simulation.FromSnapshot(sim.ToSnapshot()).Species.Get(1).LastPopulation);
        }

        [Test]
        public void LegacyTerminalSaveIsRepairedWithoutModifyingItsSnapshot()
        {
            Simulation sim = TestHelpers.Quiet(3);
            SimSnapshot snapshot = sim.ToSnapshot();
            snapshot.Extinct = true;
            snapshot.Cells = new CellSnap[0];
            snapshot.Species[0].LastPopulation = 0;
            Simulation loaded = Simulation.FromSnapshot(snapshot);
            Assert.AreEqual(0, loaded.Species.LivingCount);
            Assert.AreEqual(0, loaded.Stats.Samples.Last().Count);
            Assert.AreEqual(0, loaded.Stats.Samples.Last().LivingSpecies);
            Assert.AreEqual(3, snapshot.Species[0].CurrentPopulation, "Loading must not mutate the supplied snapshot.");
        }

        [Test]
        public void MeanGenerationCanFallAndTheAxisUsesActualExtrema()
        {
            var axis = new SampleAxis();
            axis.Build(new List<Sample> {
                new Sample { Time = 0, MeanGeneration = 12 },
                new Sample { Time = 1, MeanGeneration = 9 },
                new Sample { Time = 2, MeanGeneration = 11 }
            }, true);
            CollectionAssert.AreEqual(new[] {12f, 9f, 11f}, axis.Xs.Take(3));
            Assert.AreEqual(9, axis.Min);
            Assert.AreEqual(12, axis.Max);
            MethodInfo nearest = typeof(ChartElement).GetMethod("Nearest", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.AreEqual(1, nearest.Invoke(null, new object[] {axis.Xs, 3, 9f}));
        }

        [Test]
        public void HoverIsInvalidatedOnDecimationTraitAxisAndRunChanges()
        {
            Simulation sim = TestHelpers.Quiet(2, p => p.MaxSamples = 20);
            for (int i = 0; i < 17; i++) sim.Stats.Sample(sim.Cells, sim.Species, i + 1);
            var chart = new TraitLineChart();
            chart.Bind(sim);
            int old = sim.Stats.Samples.Count - 1;
            Set(chart, "hoverIndex", old);
            for (int i = 0; i < 3; i++) sim.Stats.Sample(sim.Cells, sim.Species, 20 + i);
            Assert.Less(sim.Stats.Samples.Count, old);
            chart.Refresh();
            Assert.AreEqual(-1, Get<int>(chart, "hoverIndex"));
            Assert.IsFalse((bool)Call(chart, "IsValidIndex", old));
            Set(chart, "hoverIndex", 1);
            chart.Trait = 1;
            Assert.AreEqual(-1, Get<int>(chart, "hoverIndex"));
            Set(chart, "hoverIndex", 1);
            chart.ByGeneration = true;
            Assert.AreEqual(-1, Get<int>(chart, "hoverIndex"));
            Set(chart, "hoverIndex", 1);
            chart.Bind(TestHelpers.Quiet(1));
            Assert.AreEqual(-1, Get<int>(chart, "hoverIndex"));
        }

        [Test]
        public void EmptyPopulationHasNoTraitMeanReadout()
        {
            Simulation sim = TestHelpers.Quiet(1);
            sim.Cells.Alive[0].Energy = 0;
            sim.Step();
            var chart = new TraitLineChart();
            chart.Bind(sim);
            List<ReadoutRow> rows = Readout(chart, sim.Stats.Samples.Count - 1);
            Assert.AreEqual("No living Cells", rows[0].Name);
            Assert.IsFalse(rows.Any(row => row.Name == "mean"));
            Call(chart, "ComputeAxes");
            Assert.GreaterOrEqual(Get<float>(chart, "yMin"), Traits.Min[0]);
        }

        [Test]
        public void TreeGrowsReadableRowsAndIncludesEvents()
        {
            Simulation sim = TestHelpers.Quiet(1);
            for (int i = 2; i <= 100; i++)
                sim.Species.Records.Add(new SpeciesRecord { Id = i, ParentId = i - 1, Name = "Species " + i });
            sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, Radius = 20, Duration = 20 });
            var chart = new TreeChart();
            chart.Bind(sim);
            Assert.GreaterOrEqual(chart.style.height.value.value, 100 * DesignTokens.TreeRowHeight + ChartElement.MarginTop + ChartElement.MarginBottom);
            Call(chart, "ComputeAxes");
            Assert.AreEqual(DesignTokens.TreeRowHeight, Get<float>(chart, "rowHeight"));
            Assert.IsNotNull(Get<Func<double, float>>(chart, "eventTimeToX"));
        }

        [Test]
        public void SimultaneousEventChipsRetainEveryIntervention()
        {
            Simulation sim = TestHelpers.Quiet(1);
            sim.PlaceEvent(new EventSpec { Kind = EventKind.HeatWave, Radius = 20, Duration = 20 });
            sim.PlaceEvent(new EventSpec { Kind = EventKind.Famine, Radius = 20, Duration = 20 });
            var chart = new TraitLineChart();
            chart.Bind(sim);
            Set(chart, "plot", new Rect(56, 28, 400, 150));
            Call(chart, "ComputeAxes");
            Call(chart, "LayoutMarkers");
            List<VisualElement> chips = Get<List<VisualElement>>(chart, "chips");
            Assert.AreEqual(1, chips.Count);
            var events = (List<EventRecord>)chips[0].userData;
            Assert.AreEqual(2, events.Count);
            CollectionAssert.AreEquivalent(new[] {EventKind.HeatWave, EventKind.Famine}, events.Select(e => e.Kind));
        }

        [Test]
        public void SpeciesChartIdentifiesTheOtherAggregate()
        {
            Simulation sim = TestHelpers.Quiet(1);
            for (int i = 2; i <= 22; i++)
                sim.Species.Records.Add(new SpeciesRecord { Id = i, PeakPopulation = 100 - i, Name = "Species " + i });
            var chart = new SpeciesChart();
            chart.Bind(sim);
            Assert.IsTrue(chart.HasOther);
            Assert.IsTrue(chart.IsAggregated(1));
            Assert.IsFalse(chart.IsAggregated(2), "Legend classification must work before the first chart layout.");
            Assert.AreEqual(1, chart.OtherPopulation);
        }

        [TestCase(0.8f, 0.8f)]
        [TestCase(2f, 2f)]
        [TestCase(0f, 1f)]
        [TestCase(-1f, 1f)]
        public void PixelRatioPreservesFractionalZoom(float input, float expected)
        {
            Assert.AreEqual(expected, FileBridge.ValidPixelRatio(input));
            Assert.AreEqual(1, FileBridge.ValidPixelRatio(float.NaN));
            Assert.AreEqual(1, FileBridge.ValidPixelRatio(float.PositiveInfinity));
        }

        [Test]
        public void FileCallbacksRejectStaleRequestIdsAndDistinguishEmptyFiles()
        {
            FileBridge bridge = FileBridge.Create();
            try
            {
                int received = 0;
                FilePickResult result = default(FilePickResult);
                Set(bridge, "pending", new Action<FilePickResult>(value => { received++; result = value; }));
                Set(bridge, "pendingId", 2);
                bridge.OnFilePicked("{\"id\":1,\"status\":\"success\",\"text\":\"old\"}");
                Assert.AreEqual(0, received);
                bridge.OnFilePicked("{\"id\":2,\"status\":\"success\",\"text\":\"\"}");
                Assert.AreEqual(1, received);
                Assert.AreEqual(FilePickStatus.Success, result.Status);
                Assert.AreEqual("", result.Text);
                bridge.OnFilePicked("{\"id\":2,\"status\":\"success\",\"text\":\"duplicate\"}");
                Assert.AreEqual(1, received);
            }
            finally { Object.DestroyImmediate(bridge.gameObject); }
        }

        [Test]
        public void OrdinaryTooltipsDoNotInterceptPointerEvents()
        {
            var card = new TooltipCard();
            card.SetTip(new TipContent { Name = "Name", Body = "Body" });
            Assert.AreEqual(PickingMode.Ignore, card.pickingMode);
            Assert.IsTrue(card.Query<VisualElement>().ToList().All(element => element.pickingMode == PickingMode.Ignore));
            Assert.AreEqual(PickingMode.Position, new TooltipCard(true).pickingMode);
        }

        [Test]
        public void DoubleTrackClickDoesNotResetTheSlider()
        {
            var slider = new EvoSlider("Food abundance", "food-abundance", 0, 2, 1, 0, v => v.ToString());
            slider.Value = 0.5f;
            using (PointerDownEvent down = PointerDownEvent.GetPooled(new Event {type = EventType.MouseDown, mousePosition = new Vector2(100, 10), button = 0, clickCount = 2}))
                Call(slider, "OnDown", down);
            Assert.AreEqual(0.5f, slider.Value, "A second track click must not reset to the default, even before layout.");
        }

        sealed class AppFixture : IDisposable
        {
            public readonly GameObject Host = new GameObject("UX test");
            public readonly AppController App;
            public readonly VisualElement Root = new VisualElement();
            readonly WorldView world;
            public AppFixture()
            {
                var camera = new CameraController(Host.AddComponent<Camera>());
                world = new WorldView(Host.transform, RenderAssets.Load());
                App = AppController.Create(Host, world, camera);
                App.Attach(new UiRoot(App, Root), new InputRouter(App));
                Call(App, "Begin", TestHelpers.Quiet(2), AppMode.Running);
            }
            public EvoButton ConfirmButton { get { return Root.Query<EvoButton>().ToList().First(b => b.TipId == "confirm-discard"); } }
            public void Confirm() { Get<Action>(ConfirmButton, "Clicked")(); }
            public void Dispose()
            {
                EvoDropdown.CloseOpen();
                Root.Clear();
                foreach (MeshFilter filter in Host.GetComponentsInChildren<MeshFilter>())
                    if (filter.sharedMesh != null) Object.DestroyImmediate(filter.sharedMesh);
                foreach (FieldInfo field in world.GetType().GetFields(Hidden))
                    if (field.FieldType == typeof(Material) || field.FieldType == typeof(Texture2D))
                        Object.DestroyImmediate((Object)field.GetValue(world));
                Object.DestroyImmediate(Host);
                Ui.Overlay = null;
            }
        }

        [Test]
        public void MenuAndRestartRequireExplicitConfirmationAndCancelPreservesTheRun()
        {
            using (var f = new AppFixture())
            {
                Simulation original = f.App.Sim;
                f.App.GoToSetup();
                Assert.AreSame(original, f.App.Sim);
                Assert.IsTrue(f.Root.Q<RunActionDialog>().IsOpen);
                Assert.AreEqual(AppMode.Paused, f.App.Mode);
                f.App.Ui.HandleEscape();
                Assert.AreSame(original, f.App.Sim);
                Assert.AreEqual(AppMode.Running, f.App.Mode);
                f.App.Restart();
                Assert.AreSame(original, f.App.Sim);
                f.App.Ui.HandleEscape();
                f.App.GoToSetup();
                f.Confirm();
                Assert.IsNull(f.App.Sim);
                Assert.AreEqual(AppMode.Setup, f.App.Mode);
            }
        }

        [Test]
        public void ValidLoadWaitsForReplacementConfirmationAndBadLoadPreservesTheRun()
        {
            using (var f = new AppFixture())
            {
                Simulation original = f.App.Sim;
                Call(f.App, "OnLoadText", "");
                Assert.AreSame(original, f.App.Sim);
                Call(f.App, "OnLoadText", "{");
                Assert.AreSame(original, f.App.Sim);
                SimSnapshot invalid = TestHelpers.Quiet(1).ToSnapshot();
                invalid.Stats.IntervalSteps = 0;
                Call(f.App, "OnLoadText", JsonUtility.ToJson(invalid));
                Assert.AreSame(original, f.App.Sim);
                Call(f.App, "OnLoadText", RunFiles.ToJson(TestHelpers.Quiet(1)));
                Assert.AreSame(original, f.App.Sim);
                f.App.Ui.HandleEscape();
                Assert.AreSame(original, f.App.Sim);
                Call(f.App, "OnLoadText", RunFiles.ToJson(TestHelpers.Quiet(1)));
                f.Confirm();
                Assert.AreNotSame(original, f.App.Sim);
                Assert.AreEqual(1, f.App.Sim.Cells.AliveCount);
            }
        }

        [Test]
        public void OutsideWorldPlacementStaysArmedAndWholeWorldReplacesTheOldArm()
        {
            using (var f = new AppFixture())
            {
                var local = new EventSpec {Kind = EventKind.HeatWave, Radius = 5, Duration = 10};
                f.App.BeginPlacement(local);
                f.App.PlaceArmedAt(new Vector2(-1, 10));
                Assert.AreSame(local, f.App.Armed);
                Assert.AreEqual(0, f.App.Sim.Events.Log.Count);
                f.App.ApplyWholeWorld(new EventSpec {Kind = EventKind.FoodBloom, Radius = 20, Duration = 10});
                Assert.IsNull(f.App.Armed);
                Assert.AreEqual(1, f.App.Sim.Events.Log.Count);
                Assert.AreEqual(EventKind.FoodBloom, f.App.Sim.Events.Log[0].Kind);
            }
        }

        [Test]
        public void EventDialogReenablesWhenCapacityRecoversAndKeepsToxinDoseEditable()
        {
            using (var f = new AppFixture())
            {
                while (f.App.Sim.CanPlaceEvent(EventKind.HeatWave))
                    f.App.Sim.PlaceEvent(new EventSpec {Kind = EventKind.HeatWave, Radius = 5, Duration = 10});
                f.App.Ui.OpenEventDialog(EventKind.HeatWave);
                EventDialog dialog = f.Root.Q<EventDialog>();
                EvoButton place = dialog.Query<EvoButton>().ToList().First(b => b.TipId == "event-place");
                Assert.IsTrue(place.Disabled);
                f.App.Sim.Events.Update(10000, f.App.Sim.P);
                dialog.Tick();
                Assert.IsFalse(place.Disabled);
                f.App.Ui.OpenEventDialog(EventKind.ToxinSpill);
                EvoCheckbox whole = dialog.Q<EvoCheckbox>();
                whole.Value = true;
                Get<Action<bool>>(whole, "Changed")(true);
                Assert.IsTrue(dialog.Q<EvoSlider>().Enabled);
                f.App.Ui.OpenEventDialog(EventKind.HeatWave);
                whole = dialog.Q<EvoCheckbox>();
                whole.Value = true;
                Get<Action<bool>>(whole, "Changed")(true);
                Assert.IsFalse(dialog.Q<EvoSlider>().Enabled);
            }
        }

        [Test]
        public void LayerPriorityAndPanelCameraContextStayStable()
        {
            using (var f = new AppFixture())
            {
                VisualElement warning = Get<VisualElement>(f.App.Ui, "tooSmall");
                HelpScreen help = f.Root.Q<HelpScreen>();
                f.App.Ui.OpenHelp();
                Assert.Greater(f.Root.IndexOf(warning), f.Root.IndexOf(help));
                Assert.Greater(f.Root.IndexOf(Ui.Overlay), f.Root.IndexOf(warning));
                f.App.Ui.CloseAll();
                f.App.Cam.Camera.orthographicSize = 20;
                f.App.Cam.Camera.transform.position = new Vector3(50, 40, -1);
                Set(f.App, "fitFrames", 0);
                f.App.TogglePanel();
                Assert.AreEqual(20, f.App.Cam.Camera.orthographicSize);
                Assert.AreEqual(new Vector3(50, 40, -1), f.App.Cam.Camera.transform.position);
                Assert.AreEqual(0, Get<int>(f.App, "fitFrames"), "A toggle must not schedule a later fit.");
            }
        }

        [Test]
        public void DropdownClosesWithItsPanelAndAnEmptySpeciesHighlightClears()
        {
            using (var f = new AppFixture())
            {
                EvoDropdown dropdown = f.Root.Q<TraitsTab>().Q<EvoDropdown>();
                Call(dropdown, "Open");
                Assert.IsTrue(EvoDropdown.IsOpen);
                Assert.IsTrue(f.App.Ui.ModalOpen);
                f.App.TogglePanel();
                Assert.IsFalse(EvoDropdown.IsOpen);
                f.App.ToggleHighlight(1);
                Assert.AreEqual(1, f.App.WorldView.HighlightSpeciesId);
                for (int i = 0; i < f.App.Sim.Cells.AliveCount; i++) f.App.Sim.Cells.Alive[i].SpeciesId = 2;
                Call(f.App, "ValidateHighlight");
                Assert.AreEqual(0, f.App.WorldView.HighlightSpeciesId);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TerminalExtinctionClosesPopupCatchers(bool events)
        {
            using (var f = new AppFixture())
            {
                EvoDropdown dropdown = f.Root.Q<TraitsTab>().Q<EvoDropdown>();
                if (events) EventsMenu.Open(dropdown, kind => { });
                else Call(dropdown, "Open");
                Assert.IsTrue(EvoDropdown.IsOpen || EventsMenu.IsOpen);
                foreach (Cell cell in f.App.Sim.Cells.Alive.Take(f.App.Sim.Cells.AliveCount)) cell.Energy = 0;
                Call(f.App, "Update");
                Assert.AreEqual(AppMode.Extinct, f.App.Mode);
                Assert.IsFalse(EvoDropdown.IsOpen);
                Assert.IsFalse(EventsMenu.IsOpen);
                Assert.IsTrue(f.Root.Q<ExtinctionOverlay>().style.display.value != DisplayStyle.None);
            }
        }

        [Test]
        public void GroupedEventDetailsUseTheInteractiveScrollCard()
        {
            using (var f = new AppFixture())
            {
                for (int i = 0; i < 100; i++)
                    f.App.Sim.PlaceEvent(new EventSpec { Kind = EventKind.ToxinSpill });
                var chart = new TraitLineChart();
                chart.Bind(f.App.Sim);
                Set(chart, "plot", new Rect(56, 28, 400, 150));
                Call(chart, "ComputeAxes");
                Call(chart, "LayoutMarkers");
                VisualElement chip = Get<List<VisualElement>>(chart, "chips")[0];
                var card = (TooltipCard)typeof(Tooltips).GetField("readout", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.AreEqual(PickingMode.Position, card.Q<ScrollView>().pickingMode);
                MethodInfo groupTip = typeof(ChartElement).GetMethod("EventGroupTip", BindingFlags.Static | BindingFlags.NonPublic);
                TipContent tip = (TipContent)groupTip.Invoke(null, new[] { chip.userData });
                Assert.AreEqual(100, tip.Body.Split('\n').Length);
                card.SetTip(tip);
                Assert.IsTrue(card.Query<Label>().ToList().Any(l => l.text.Contains(tip.Body)));
                Tooltips.Hide();
            }
        }
    }
}
