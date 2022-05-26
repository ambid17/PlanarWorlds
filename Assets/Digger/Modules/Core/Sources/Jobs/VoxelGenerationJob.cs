using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct VoxelGenerationJob : IJobParallelFor
    {
        public float ChunkAltitude;
        public int SizeVox;
        public int SizeVox2;
        public float3 HeightmapScale;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [WriteOnly] public NativeArray<Voxel> Voxels;

        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);
            var p = pi * HeightmapScale;
            var height = Heights[Utils.XYZToHeightIndex(pi, SizeVox)];
            var voxel = new Voxel(p.y + ChunkAltitude - height);
            Voxels[index] = voxel;
        }
    }
}