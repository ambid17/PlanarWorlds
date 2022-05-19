using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct VoxelFillSurfaceJob : IJobParallelFor
    {
        public float ChunkAltitude;
        public int SizeVox;
        public int SizeVox2;
        public float3 HeightmapScale;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> Holes;

        [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> Voxels;

        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);
            var p = pi * HeightmapScale;
            var voxel = Voxels[index];
            if (voxel.Alteration == Voxel.Unaltered && Utils.IsOnHole(pi, SizeVox, Holes) && Utils.IsOnSurface(pi,HeightmapScale.y, p.y + ChunkAltitude, SizeVox, Heights)) {
                voxel.Alteration = Voxel.OnSurface;
                Voxels[index] = voxel;
            }
        }
    }
}