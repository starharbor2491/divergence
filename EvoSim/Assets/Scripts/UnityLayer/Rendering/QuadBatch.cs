using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Evo.UnityLayer
{
    /// <summary>
    /// One dynamic mesh of textured, vertex-coloured quads drawn with a single material.
    /// The index buffer is filled once; each frame only the vertices that are used are uploaded.
    /// </summary>
    public sealed class QuadBatch
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
        }

        readonly Mesh mesh;
        readonly Vertex[] vertices;
        readonly int capacity;
        int count;
        public readonly GameObject GameObject;

        public int Count { get { return count; } }
        public int Capacity { get { return capacity; } }

        public QuadBatch(string name, Transform parent, Material material, int capacityQuads, int sortingOrder)
        {
            capacity = capacityQuads;
            vertices = new Vertex[capacity * 4];
            mesh = new Mesh { name = name };
            mesh.MarkDynamic();
            mesh.SetVertexBufferParams(capacity * 4,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
            var indices = new ushort[capacity * 6];
            for (int q = 0; q < capacity; q++)
            {
                int v = q * 4, i = q * 6;
                indices[i] = (ushort)v; indices[i + 1] = (ushort)(v + 1); indices[i + 2] = (ushort)(v + 2);
                indices[i + 3] = (ushort)v; indices[i + 4] = (ushort)(v + 2); indices[i + 5] = (ushort)(v + 3);
            }
            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(indices, 0, 0, indices.Length, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            mesh.subMeshCount = 1;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(100000f, 100000f, 10f));

            GameObject = new GameObject(name);
            GameObject.transform.SetParent(parent, false);
            GameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = GameObject.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.sortingOrder = sortingOrder;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            r.allowOcclusionWhenDynamic = false;
        }

        public void Begin() { count = 0; }

        /// <summary>An axis-aligned quad centred on (cx, cy).</summary>
        public void Quad(float cx, float cy, float halfW, float halfH, Color32 color)
        {
            if (count >= capacity) return;
            int v = count++ * 4;
            vertices[v] = new Vertex { Position = new Vector3(cx - halfW, cy - halfH, 0f), Color = color, Uv = new Vector2(0f, 0f) };
            vertices[v + 1] = new Vertex { Position = new Vector3(cx - halfW, cy + halfH, 0f), Color = color, Uv = new Vector2(0f, 1f) };
            vertices[v + 2] = new Vertex { Position = new Vector3(cx + halfW, cy + halfH, 0f), Color = color, Uv = new Vector2(1f, 1f) };
            vertices[v + 3] = new Vertex { Position = new Vector3(cx + halfW, cy - halfH, 0f), Color = color, Uv = new Vector2(1f, 0f) };
        }

        /// <summary>A line segment drawn as a rotated rectangle of the given thickness.</summary>
        public void Segment(float x0, float y0, float x1, float y1, float thickness, Color32 color)
        {
            if (count >= capacity) return;
            float dx = x1 - x0, dy = y1 - y0;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 1e-6f) return;
            float nx = -dy / len * thickness * 0.5f, ny = dx / len * thickness * 0.5f;
            int v = count++ * 4;
            vertices[v] = new Vertex { Position = new Vector3(x0 - nx, y0 - ny, 0f), Color = color, Uv = new Vector2(0.5f, 0.5f) };
            vertices[v + 1] = new Vertex { Position = new Vector3(x0 + nx, y0 + ny, 0f), Color = color, Uv = new Vector2(0.5f, 0.5f) };
            vertices[v + 2] = new Vertex { Position = new Vector3(x1 + nx, y1 + ny, 0f), Color = color, Uv = new Vector2(0.5f, 0.5f) };
            vertices[v + 3] = new Vertex { Position = new Vector3(x1 - nx, y1 - ny, 0f), Color = color, Uv = new Vector2(0.5f, 0.5f) };
        }

        public void End()
        {
            if (count > 0)
                mesh.SetVertexBufferData(vertices, 0, 0, count * 4, 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, count * 6, MeshTopology.Triangles),
                MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
            GameObject.SetActive(count > 0);
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
            Object.Destroy(mesh);
        }
    }
}
