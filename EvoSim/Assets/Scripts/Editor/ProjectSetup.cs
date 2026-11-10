using System.IO;
using Evo.UnityLayer;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.Editor
{
    /// <summary>
    /// Creates everything in the project that is not code: the runtime theme, RenderAssets, Main.unity, Player settings for WebGL
    /// and tokens.uss. Idempotent; the build script runs it first.
    /// </summary>
    public static class ProjectSetup
    {
        public const string ThemePath = "Assets/UI/Theme/RuntimeTheme.tss";
        public const string TokensPath = "Assets/UI/Theme/tokens.uss";
        public const string RenderAssetsPath = "Assets/Resources/RenderAssets.asset";
        public const string ShaderPath = "Assets/Shaders/UnlitVertexColorTexture.shader";
        public const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Divergence/Apply project setup")]
        public static void Apply()
        {
            GenerateTokensUss.Write();
            WriteTheme();
            CreateRenderAssets();
            CreateScene();
            ConfigurePlayer();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Divergence project setup applied.");
        }

        static void WriteTheme()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ThemePath));
            string text = "@import url(\"unity-theme://default\");\n";
            if (!File.Exists(ThemePath) || File.ReadAllText(ThemePath) != text)
            {
                File.WriteAllText(ThemePath, text);
                AssetDatabase.ImportAsset(ThemePath, ImportAssetOptions.ForceUpdate);
            }
        }

        static void CreateRenderAssets()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RenderAssetsPath));
            var assets = AssetDatabase.LoadAssetAtPath<RenderAssets>(RenderAssetsPath);
            if (assets == null)
            {
                assets = ScriptableObject.CreateInstance<RenderAssets>();
                AssetDatabase.CreateAsset(assets, RenderAssetsPath);
            }
            assets.UnlitVertexColorTexture = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            assets.FontRegular = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/JetBrainsMono-Regular.ttf");
            assets.FontMedium = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/JetBrainsMono-Medium.ttf");
            assets.FontBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/JetBrainsMono-Bold.ttf");
            assets.RuntimeTheme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            EditorUtility.SetDirty(assets);
            if (assets.UnlitVertexColorTexture == null) Debug.LogError("Shader not found at " + ShaderPath);
            if (assets.RuntimeTheme == null) Debug.LogError("Runtime theme not found at " + ThemePath);
            if (assets.FontRegular == null) Debug.LogError("Fonts not found under Assets/Fonts");
        }

        static void CreateScene()
        {
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            if (File.Exists("Assets/Scenes/SampleScene.unity")) AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Yumei";
            PlayerSettings.productName = "Divergence";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSpeed);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.template = "PROJECT:Divergence";
        }
    }

    /// <summary>Writes Assets/UI/Theme/tokens.uss from DesignTokens (DESIGN 9). A test checks the committed file matches.</summary>
    public static class GenerateTokensUss
    {
        [MenuItem("Divergence/Generate tokens.uss")]
        public static void Write()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ProjectSetup.TokensPath));
            string text = DesignTokens.GenerateUss();
            if (!File.Exists(ProjectSetup.TokensPath) || File.ReadAllText(ProjectSetup.TokensPath) != text)
            {
                File.WriteAllText(ProjectSetup.TokensPath, text);
                AssetDatabase.ImportAsset(ProjectSetup.TokensPath);
            }
        }
    }
}
