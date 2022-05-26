using System;
using Digger.Modules.Core.Sources.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.Operations
{
    public class KernelOperation : IOperation<VoxelKernelModificationJob>
    {
        public ModificationParameters Params;

        public ModificationArea GetAreaToModify(DiggerSystem digger)
        {
            var action = Params.Action;
            if (action != ActionType.Paint && action != ActionType.PaintHoles && Params.Opacity < 0f) {
                Debug.LogWarning("Opacity can only be negative when action type is 'Paint' or 'PaintHoles'");
                return new ModificationArea
                {
                    NeedsModification = false
                };
            }

            return ModificationAreaUtils.GetSphericalAreaToModify(digger, Params.Position, math.max(math.max(Params.Size.x, Params.Size.y), Params.Size.z));
        }

        public VoxelKernelModificationJob Do(VoxelChunk chunk)
        {
            chunk.InitVoxelArrayBeforeOperation();
            var voxels = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob);
            var voxelsOut = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob);

            var heights = new NativeArray<float>(chunk.HeightArray, Allocator.TempJob);
            var holes = new NativeArray<int>(chunk.HolesArray, Allocator.TempJob);
            var chunkPosition = chunk.ChunkPosition;
            var digger = chunk.Digger;

            var job = new VoxelKernelModificationJob
            {
                SizeVox = chunk.SizeVox,
                SizeVox2 = chunk.SizeVox * chunk.SizeVox,
                SizeOfMesh = chunk.SizeOfMesh,
                LowInd = chunk.SizeVox - 3,
                Action = Params.Action,
                HeightmapScale = chunk.HeightmapScale,
                Voxels = voxels,
                VoxelsOut = voxelsOut,
                Intensity = Params.Opacity,
                Center = Params.Position - chunk.AbsoluteWorldPosition,
                Radius = Params.Size.x,
                ChunkAltitude = chunk.WorldPosition.y,
                Heights = heights,
                Holes = holes,
                NewHolesConcurrentCounter = new NativeArray<int>(1, Allocator.TempJob),

                NeighborVoxelsLBB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, -1, -1)),
                NeighborVoxelsLBF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, -1, +1)),
                NeighborVoxelsLB_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, -1, +0)),
                NeighborVoxels_BB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, -1, -1)),
                NeighborVoxels_BF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, -1, +1)),
                NeighborVoxels_B_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, -1, +0)),
                NeighborVoxelsRBB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, -1, -1)),
                NeighborVoxelsRBF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, -1, +1)),
                NeighborVoxelsRB_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, -1, +0)),
                NeighborVoxelsL_B = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +0, -1)),
                NeighborVoxelsL_F = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +0, +1)),
                NeighborVoxelsL__ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +0, +0)),
                NeighborVoxels__B = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, +0, -1)),
                NeighborVoxels__F = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, +0, +1)),
                NeighborVoxelsR_B = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +0, -1)),
                NeighborVoxelsR_F = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +0, +1)),
                NeighborVoxelsR__ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +0, +0)),
                NeighborVoxelsLUB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +1, -1)),
                NeighborVoxelsLUF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +1, +1)),
                NeighborVoxelsLU_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(-1, +1, +0)),
                NeighborVoxels_UB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, +1, -1)),
                NeighborVoxels_UF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, +1, +1)),
                NeighborVoxels_U_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+0, +1, +0)),
                NeighborVoxelsRUB = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +1, -1)),
                NeighborVoxelsRUF = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +1, +1)),
                NeighborVoxelsRU_ = VoxelChunk.LoadVoxels(digger, chunkPosition + new Vector3i(+1, +1, +0)),
            };

            return job;
        }

        public void Complete(VoxelKernelModificationJob job, VoxelChunk chunk)
        {
            // Wait for the job to complete
            job.DisposeNeighbors();
            job.Voxels.Dispose();

            job.VoxelsOut.CopyTo(chunk.VoxelArray);
            job.VoxelsOut.Dispose();
            job.Heights.Dispose();
            
            if (job.NewHolesConcurrentCounter[0] > 0) {
                chunk.Cutter.Cut(job.Holes, chunk.VoxelPosition, chunk.ChunkPosition);
            }

            job.NewHolesConcurrentCounter.Dispose();
            job.Holes.Dispose();
        }
    }
}