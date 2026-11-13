using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Builds the whole program from code when the scene loads: camera, World view, Liquid Glass backdrop, UI document, input and AppController.
    /// Main.unity is nearly empty (PLAN 14.5).
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (AppController.Instance != null) return;
            Application.runInBackground = true;
            Application.targetFrameRate = 60;

            RenderAssets assets = RenderAssets.Load();
            LoadFonts(assets);

            var host = new GameObject("Divergence");
            Object.DontDestroyOnLoad(host);

            var camGo = new GameObject("World camera");
            camGo.transform.SetParent(host.transform, false);
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            var controller = new CameraController(camera);

            var world = new WorldView(host.transform, assets);
            AppController app = AppController.Create(host, world, controller);

            FileBridge.Create();
            EnsureEventSystem();

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(host.transform, false);
            uiGo.SetActive(false);
            var doc = uiGo.AddComponent<UIDocument>();
            doc.panelSettings = MakePanelSettings(assets);
            uiGo.SetActive(true);

            GlassBackdrop.Create(host, camera, assets, doc);

            var ui = new UiRoot(app, doc.rootVisualElement);
            app.Attach(ui, new InputRouter(app));
            DevHarness.TryStart(app);
        }

        static void LoadFonts(RenderAssets assets)
        {
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Ui.FontRegular = assets != null && assets.FontRegular != null ? assets.FontRegular : fallback;
            Ui.FontMedium = assets != null && assets.FontMedium != null ? assets.FontMedium : Ui.FontRegular;
            Ui.FontBold = assets != null && assets.FontBold != null ? assets.FontBold : Ui.FontRegular;

            Ui.UiRegular = assets != null && assets.UiFontRegular != null ? assets.UiFontRegular : Ui.FontRegular;
            Ui.UiMedium = assets != null && assets.UiFontMedium != null ? assets.UiFontMedium : Ui.UiRegular;
            Ui.UiSemiBold = assets != null && assets.UiFontSemiBold != null ? assets.UiFontSemiBold : Ui.UiMedium;
            Ui.UiBold = assets != null && assets.UiFontBold != null ? assets.UiFontBold : Ui.UiSemiBold;
        }

        static PanelSettings MakePanelSettings(RenderAssets assets)
        {
            var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(DesignTokens.ReferenceWidth, DesignTokens.ReferenceHeight);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.clearColor = false;
            if (assets != null && assets.RuntimeTheme != null) ps.themeStyleSheet = assets.RuntimeTheme;
            return ps;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.transform.SetParent(Object.FindFirstObjectByType<AppController>().transform, false);
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
