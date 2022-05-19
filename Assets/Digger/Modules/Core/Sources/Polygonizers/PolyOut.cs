using Unity.Collections;

namespace Digger.Modules.Core.Sources.Polygonizers
{
    public struct PolyOut
    {
        public const int MaxVertexCount = 65536;
        public const int MaxTriangleCount = 65536;
        
        public NativeArray<VertexData> outVertexData;

        public NativeArray<ushort> outTriangles;

        public void Dispose()
        {
            outVertexData.Dispose();
            outTriangles.Dispose();
        }

        public static PolyOut New()
        {
            return new PolyOut
            {
                outVertexData = new NativeArray<VertexData>(MaxVertexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory),
                outTriangles = new NativeArray<ushort>(MaxTriangleCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory)
            };
        }
    }

}