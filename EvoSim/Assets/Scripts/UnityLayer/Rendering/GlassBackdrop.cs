using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Captures the live World at 1/3 resolution into a bilinear RenderTexture and draws rounded, frosted,
    /// edge-refracted Liquid Glass lenses underneath every visible floating UI surface before UIDocument renders.
    /// </summary>
    public sealed class GlassBackdrop : MonoBehaviour
    {
        const int GlassLayer = 5; // Built-in "UI" layer, excluded from the capture camera
        const int MaxLenses = 48;

        [StructLayout(LayoutKind.Sequential)]
        struct GlassVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public Vector4 Meta;
        }

        public static bool IsActive { get; private set; }

        Camera mainCam;
        Camera captureCam;
        UIDocument doc;
        RenderTexture backdropRt;
        Material glassMat;
        Mesh mesh;
        MeshRenderer meshRenderer;
        readonly GlassVertex[] vertices = new GlassVertex[MaxLenses * 4];
        int rtW, rtH;

        public static GlassBackdrop Create(GameObject host, Camera mainCamera, RenderAssets assets, UIDocument uiDoc)
        {
            Shader shader = assets != null ? assets.LiquidGlassBackdrop : null;
            if (shader == null) shader = Shader.Find("Evo/LiquidGlassBackdrop");
            if (shader == null || mainCamera == null || uiDoc == null) return null;

            var gb = host.AddComponent<GlassBackdrop>();
            gb.Init(mainCamera, shader, uiDoc);
            return gb;
        }

        void Init(Camera mainCamera, Shader shader, UIDocument uiDoc)
        {
            mainCam = mainCamera;
            doc = uiDoc;

            // Main camera sees both Default (World) and GlassLayer; capture camera sees only Default (World)
            mainCam.cullingMask = (1 << 0) | (1 << GlassLayer);
            mainCam.depth = 0;

            var capGo = new GameObject("World backdrop capture");
            capGo.SetActive(false);
            capGo.transform.SetParent(transform, false);
            captureCam = capGo.AddComponent<Camera>();
            captureCam.orthographic = true;
            captureCam.clearFlags = CameraClearFlags.SolidColor;
            captureCam.backgroundColor = DesignTokens.Bg0;
            captureCam.cullingMask = 1 << 0;
            captureCam.nearClipPlane = -10f;
            captureCam.farClipPlane = 10f;
            captureCam.depth = -10;
            captureCam.allowMSAA = false;
            captureCam.allowHDR = false;

            glassMat = new Material(shader);
            EnsureRenderTexture();
            capGo.SetActive(true);

            mesh = new Mesh { name = "LiquidGlassLenses" };
            mesh.MarkDynamic();
            mesh.SetVertexBufferParams(MaxLenses * 4,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4));
            var indices = new ushort[MaxLenses * 6];
            for (int q = 0; q < MaxLenses; q++)
            {
                int v = q * 4, i = q * 6;
                indices[i] = (ushort)v; indices[i + 1] = (ushort)(v + 1); indices[i + 2] = (ushort)(v + 2);
                indices[i + 3] = (ushort)v; indices[i + 4] = (ushort)(v + 2); indices[i + 5] = (ushort)(v + 3);
            }
            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(indices, 0, 0, indices.Length, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            mesh.subMeshCount = 1;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(100000f, 100000f, 10f));

            var lensGo = new GameObject("Liquid glass backdrop");
            lensGo.layer = GlassLayer;
            lensGo.transform.SetParent(transform, false);
            lensGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshRenderer = lensGo.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = glassMat;
            meshRenderer.sortingOrder = 100;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            IsActive = true;
            LiquidGlass.RefreshFills();
        }

        void EnsureRenderTexture()
        {
            int w = Mathf.Clamp(Screen.width / 3, 128, 1280);
            int h = Mathf.Clamp(Screen.height / 3, 72, 720);
            if (backdropRt != null && rtW == w && rtH == h) return;
            if (backdropRt != null)
            {
                if (captureCam != null) captureCam.targetTexture = null;
                backdropRt.Release();
                Destroy(backdropRt);
            }
            rtW = w; rtH = h;
            var desc = new RenderTextureDescriptor(w, h, RenderTextureFormat.Default, 24)
            {
                msaaSamples = 1,
                useMipMap = false,
                autoGenerateMips = false,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear
            };
            backdropRt = new RenderTexture(desc)
            {
                name = "LiquidGlassBackdropRT",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            backdropRt.Create();
            if (captureCam != null) captureCam.targetTexture = backdropRt;
            if (glassMat != null) glassMat.SetTexture("_BackdropTex", backdropRt);
        }

        void LateUpdate()
        {
            if (mainCam == null || captureCam == null || doc == null || doc.rootVisualElement == null) return;
            EnsureRenderTexture();

            // Sync capture camera with main camera
            captureCam.transform.position = mainCam.transform.position;
            captureCam.orthographicSize = mainCam.orthographicSize;
            captureCam.backgroundColor = mainCam.backgroundColor;

            VisualElement root = doc.rootVisualElement;
            Rect panelRect = root.layout;
            if (float.IsNaN(panelRect.width) || panelRect.width < 10f || panelRect.height < 10f)
            {
                meshRenderer.enabled = false;
                return;
            }

            float scaleX = Screen.width / panelRect.width;
            float scaleY = Screen.height / panelRect.height;
            float scaleAvg = (scaleX + scaleY) * 0.5f;

            int count = 0;
            for (int i = 0; i < LiquidGlass.Surfaces.Count && count < MaxLenses; i++)
            {
                GlassEntry entry = LiquidGlass.Surfaces[i];
                if (entry.Tone == GlassTone.Modal || entry.Tone == GlassTone.Well) continue;
                VisualElement el = entry.Element;
                if (el == null || el.panel == null || !IsHierarchyVisible(el)) continue;

                Rect wb = el.worldBound;
                if (float.IsNaN(wb.width) || wb.width < 4f || wb.height < 4f) continue;

                // Convert UI Toolkit panel coords (top-left origin) to Unity screen coords (bottom-left origin)
                float sxMin = wb.xMin * scaleX;
                float sxMax = wb.xMax * scaleX;
                float syMax = Screen.height - wb.yMin * scaleY;
                float syMin = Screen.height - wb.yMax * scaleY;

                Vector3 bl = mainCam.ScreenToWorldPoint(new Vector3(sxMin, syMin, 1f));
                Vector3 tl = mainCam.ScreenToWorldPoint(new Vector3(sxMin, syMax, 1f));
                Vector3 tr = mainCam.ScreenToWorldPoint(new Vector3(sxMax, syMax, 1f));
                Vector3 br = mainCam.ScreenToWorldPoint(new Vector3(sxMax, syMin, 1f));
                bl.z = tl.z = tr.z = br.z = 0f;

                float widthPx = wb.width * scaleAvg;
                float heightPx = wb.height * scaleAvg;
                float radiusPx = entry.Radius * scaleAvg;
                float refract = entry.Tone == GlassTone.Pill ? 1.15f : 0.9f;

                Color tint = DesignTokens.Hex("#102437");
                float tintAlpha = entry.Tone == GlassTone.Pill ? 0.44f : 0.48f;
                var c32 = new Color32(
                    (byte)Mathf.RoundToInt(tint.r * 255f),
                    (byte)Mathf.RoundToInt(tint.g * 255f),
                    (byte)Mathf.RoundToInt(tint.b * 255f),
                    (byte)Mathf.RoundToInt(tintAlpha * 255f));
                var meta = new Vector4(widthPx, heightPx, radiusPx, refract);

                int v = count++ * 4;
                vertices[v] = new GlassVertex { Position = bl, Color = c32, Uv = new Vector2(0f, 0f), Meta = meta };
                vertices[v + 1] = new GlassVertex { Position = tl, Color = c32, Uv = new Vector2(0f, 1f), Meta = meta };
                vertices[v + 2] = new GlassVertex { Position = tr, Color = c32, Uv = new Vector2(1f, 1f), Meta = meta };
                vertices[v + 3] = new GlassVertex { Position = br, Color = c32, Uv = new Vector2(1f, 0f), Meta = meta };
            }

            if (count > 0)
            {
                mesh.SetVertexBufferData(vertices, 0, 0, count * 4, 0,
                    MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
                mesh.SetSubMesh(0, new SubMeshDescriptor(0, count * 6, MeshTopology.Triangles),
                    MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
            }
            meshRenderer.enabled = count > 0;
        }

        static bool IsHierarchyVisible(VisualElement e)
        {
            for (VisualElement cur = e; cur != null; cur = cur.parent)
            {
                if (cur.resolvedStyle.display == DisplayStyle.None || cur.style.display == DisplayStyle.None) return false;
                if (cur.resolvedStyle.visibility == Visibility.Hidden || cur.style.visibility == Visibility.Hidden) return false;
                if (cur.resolvedStyle.opacity <= 0.01f) return false;
            }
            return true;
        }

        void OnDestroy()
        {
            IsActive = false;
            if (captureCam != null) captureCam.targetTexture = null;
            if (backdropRt != null) { backdropRt.Release(); Destroy(backdropRt); }
            if (glassMat != null) Destroy(glassMat);
            if (mesh != null) Destroy(mesh);
        }
    }
}
