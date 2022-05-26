using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Digger.Modules.Core.Sources
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public float3 Vertex;
        public float3 Normal;
        public float4 Color;
        public float2 UV1;
        public float4 UV2;
        public float4 UV3;
        public float4 UV4;

        public static readonly VertexAttributeDescriptor[] Layout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 4),
        };
    }
}