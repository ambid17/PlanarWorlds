using Digger.Modules.Core.Sources.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.Operations
{
    public class BasicOperation : IOperation<VoxelModificationJob>
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

        public VoxelModificationJob Do(VoxelChunk chunk)
        {
            var job = new VoxelModificationJob
            {
                SizeVox = chunk.SizeVox,
                SizeVox2 = chunk.SizeVox * chunk.SizeVox,
                HeightmapScale = chunk.HeightmapScale,
                ChunkAltitude = chunk.WorldPosition.y,
                Voxels = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob),
                Heights = new NativeArray<float>(chunk.HeightArray, Allocator.TempJob),
                Holes = new NativeArray<int>(chunk.HolesArray, Allocator.TempJob),
                NewHolesConcurrentCounter = new NativeArray<int>(1, Allocator.TempJob),
                Brush = Params.Brush,
                Action = Params.Action,
                Intensity = Params.Opacity,
                IsTargetIntensity = Params.OpacityIsTarget,
                Center = Params.Position - chunk.AbsoluteWorldPosition,
                Size = Params.Size,
                UpsideDown = Params.StalagmiteUpsideDown,
                TextureIndex = (uint)Params.TextureIndex,
            };
            job.PostConstruct();
            return job;
        }

        public void Complete(VoxelModificationJob job, VoxelChunk chunk)
        {
            job.Voxels.CopyTo(chunk.VoxelArray);
            job.Voxels.Dispose();
            job.Heights.Dispose();

            var cutter = chunk.Cutter;
            if (job.Action != ActionType.Reset) {
                if (job.NewHolesConcurrentCounter[0] > 0) {
                    cutter.Cut(job.Holes, chunk.VoxelPosition, chunk.ChunkPosition);
                }
            } else {
                cutter.UnCut(job.Holes, chunk.VoxelPosition, chunk.ChunkPosition);
            }

            job.NewHolesConcurrentCounter.Dispose();
            job.Holes.Dispose();
        }
    }
}