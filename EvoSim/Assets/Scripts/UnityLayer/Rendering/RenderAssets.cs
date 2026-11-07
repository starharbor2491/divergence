using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Holds the references the build must keep. The shader is referenced here instead of being found by name,
    /// because Shader.Find can be stripped from a build. The asset lives in Assets/Resources.
    /// </summary>
    public sealed class RenderAssets : ScriptableObject
    {
        public const string ResourceName = "RenderAssets";

        public Shader UnlitVertexColorTexture;
        public Font FontRegular, FontMedium, FontBold;
        public ThemeStyleSheet RuntimeTheme;

        public static RenderAssets Load()
        {
            return Resources.Load<RenderAssets>(ResourceName);
        }
    }
}
