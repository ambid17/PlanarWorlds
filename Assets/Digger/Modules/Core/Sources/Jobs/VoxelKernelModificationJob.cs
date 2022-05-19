using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct VoxelKernelModificationJob : IJobParallelFor
    {
        public int SizeVox;
        public int SizeOfMesh;
        public int SizeVox2;
        public int LowInd;
        public ActionType Action;
        public float3 HeightmapScale;
        public float3 Center;
        public float Radius;
        public float Intensity;

        public float ChunkAltitude;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> Voxels;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [WriteOnly] public NativeArray<Voxel> VoxelsOut;

        // Smooth action only
        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLBB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLBF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLB_;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_BB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_BF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_B_;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRBB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRBF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRB_;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsL_B;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsL_F;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsL__;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels__B;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels__F;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsR_B;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsR_F;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsR__;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLUB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLUF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsLU_;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_UB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_UF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxels_U_;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRUB;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRUF;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> NeighborVoxelsRU_;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> Holes;
        
        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> NewHolesConcurrentCounter;

        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);
            var p = pi * HeightmapScale;
            var terrainHeight = Heights[Utils.XYZToHeightIndex(pi, SizeVox)];
            var terrainHeightValue = p.y + ChunkAltitude - terrainHeight;

            // Always use a spherical brush
            var distance = ComputeSphereDistance(p);

            Voxel voxel;
            switch (Action) {
                case ActionType.Smooth:
                    voxel = ApplySmooth(index, pi, distance, terrainHeightValue);
                    break;
                case ActionType.BETA_Sharpen:
                    voxel = ApplySharpen(index, pi, distance, terrainHeightValue);
                    break;
                default:
                    return; // never happens
            }

            if (voxel.Alteration != Voxel.Unaltered) {
                voxel = Utils.AdjustAlteration(voxel, pi, HeightmapScale.y, p.y + ChunkAltitude, terrainHeightValue, SizeVox, Heights);
            }

            if (voxel.IsAlteredNearBelowSurface || voxel.IsAlteredNearAboveSurface) {
                NativeCollections.Utils.IncrementAt(NewHolesConcurrentCounter, 0);
                NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x, pi.z, SizeVox));
                if (pi.x >= 1) {
                    NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x - 1, pi.z, SizeVox));
                    if (pi.z >= 1) {
                        NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x - 1, pi.z - 1, SizeVox));
                    }
                }

                if (pi.z >= 1) {
                    NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x, pi.z - 1, SizeVox));
                }
            }

            VoxelsOut[index] = voxel;
        }

        public void DisposeNeighbors()
        {
            NeighborVoxelsLBB.Dispose();
            NeighborVoxelsLBF.Dispose();
            NeighborVoxelsLB_.Dispose();
            NeighborVoxels_BB.Dispose();
            NeighborVoxels_BF.Dispose();
            NeighborVoxels_B_.Dispose();
            NeighborVoxelsRBB.Dispose();
            NeighborVoxelsRBF.Dispose();
            NeighborVoxelsRB_.Dispose();
            NeighborVoxelsL_B.Dispose();
            NeighborVoxelsL_F.Dispose();
            NeighborVoxelsL__.Dispose();
            NeighborVoxels__B.Dispose();
            NeighborVoxels__F.Dispose();
            NeighborVoxelsR_B.Dispose();
            NeighborVoxelsR_F.Dispose();
            NeighborVoxelsR__.Dispose();
            NeighborVoxelsLUB.Dispose();
            NeighborVoxelsLUF.Dispose();
            NeighborVoxelsLU_.Dispose();
            NeighborVoxels_UB.Dispose();
            NeighborVoxels_UF.Dispose();
            NeighborVoxels_U_.Dispose();
            NeighborVoxelsRUB.Dispose();
            NeighborVoxelsRUF.Dispose();
            NeighborVoxelsRU_.Dispose();
        }

        private float ComputeSphereDistance(float3 p)
        {
            var vec = p - Center;
            var distance = (float)Math.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
            return Radius - distance;
        }

        private Voxel ApplySmooth(int index, int3 pi, float distance, float terrainHeightValue)
        {
            var voxel = Voxels[index];

            var voxelValue = 0f;
            uint alterationNeighbour = 0;
            for (var x = pi.x - 1; x <= pi.x + 1; ++x) {
                for (var y = pi.y - 1; y <= pi.y + 1; ++y) {
                    for (var z = pi.z - 1; z <= pi.z + 1; ++z) {
                        var vox = GetVoxelAt(x, y, z);
                        voxelValue += vox.Value;
                        if (vox.Alteration > alterationNeighbour)
                            alterationNeighbour = vox.Alteration;
                    }
                }
            }

            const float by27 = 1f / 27f;
            voxelValue *= by27;

            if (math.abs(voxelValue - terrainHeightValue) < 0.1f)
                return voxel;

            if (voxel.IsAlteredFarOrNearSurface)
                alterationNeighbour = voxel.Alteration;

            return ComputeAltered(distance, voxel, voxelValue, alterationNeighbour);
        }

        private Voxel ApplySharpen(int index, int3 pi, float distance, float terrainHeightValue)
        {
            var voxel = Voxels[index];
            if (!voxel.IsAlteredFarOrNearSurface)
                return voxel;

            var voxelValue = 0f;
            uint alterationNeighbour = 0;
            voxelValue += VoxelValue(pi.x - 1, pi.y, pi.z, -1f, ref alterationNeighbour);
            voxelValue += VoxelValue(pi.x + 1, pi.y, pi.z, -1f, ref alterationNeighbour);
            voxelValue += VoxelValue(pi.x, pi.y - 1, pi.z, -1f, ref alterationNeighbour);
            voxelValue += VoxelValue(pi.x, pi.y + 1, pi.z, -1f, ref alterationNeighbour);
            voxelValue += VoxelValue(pi.x, pi.y, pi.z - 1, -1f, ref alterationNeighbour);
            voxelValue += VoxelValue(pi.x, pi.y, pi.z + 1, -1f, ref alterationNeighbour);
            voxelValue += voxel.Value * 7f;

            if (math.abs(voxelValue - terrainHeightValue) < 0.1f)
                return voxel;

            if (voxel.IsAlteredFarOrNearSurface)
                alterationNeighbour = voxel.Alteration;

            if (alterationNeighbour <= Voxel.OnSurface || voxelValue <= 0f && voxel.Value >= 0f || voxelValue >= 0f && voxel.Value <= 0f || Math.Abs(voxelValue) < 0.001f ||
                Math.Abs(voxelValue) > Math.Max(Math.Abs(voxel.Value) * 2, 4f))
                return voxel;

            return ComputeAltered(distance, voxel, voxelValue, alterationNeighbour);
        }

        private float VoxelValue(int x, int y, int z, float weight, ref uint alterationNeighbour)
        {
            var vox = GetVoxelAt(x, y, z);
            if (vox.Alteration > alterationNeighbour)
                alterationNeighbour = vox.Alteration;
            return weight * vox.Value;
        }

        private Voxel ComputeAltered(float distance, Voxel voxel, float voxelValue, uint alterationNeighbour)
        {
            if (distance >= 0) {
                voxel.Value = Mathf.Lerp(voxel.Value, voxelValue, Intensity);
                voxel.Alteration = alterationNeighbour;
            }

            return voxel;
        }

        private Voxel GetVoxelAt(int x, int y, int z)
        {
            // [x * SizeVox * SizeVox + y * SizeVox + z]
            if (x == -1) {
                if (y == -1) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsLBB, LowInd * SizeVox2 + LowInd * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsLBF, LowInd * SizeVox2 + LowInd * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsLB_, LowInd * SizeVox2 + LowInd * SizeVox + z);
                    }
                } else if (y > SizeOfMesh) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsLUB, LowInd * SizeVox2 + (y - SizeOfMesh) * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsLUF, LowInd * SizeVox2 + (y - SizeOfMesh) * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsLU_, LowInd * SizeVox2 + (y - SizeOfMesh) * SizeVox + z);
                    }
                } else {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsL_B, LowInd * SizeVox2 + y * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsL_F, LowInd * SizeVox2 + y * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsL__, LowInd * SizeVox2 + y * SizeVox + z);
                    }
                }
            } else if (x > SizeOfMesh) {
                if (y == -1) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsRBB, (x - SizeOfMesh) * SizeVox2 + LowInd * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsRBF, (x - SizeOfMesh) * SizeVox2 + LowInd * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsRB_, (x - SizeOfMesh) * SizeVox2 + LowInd * SizeVox + z);
                    }
                } else if (y > SizeOfMesh) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsRUB, (x - SizeOfMesh) * SizeVox2 + (y - SizeOfMesh) * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsRUF, (x - SizeOfMesh) * SizeVox2 + (y - SizeOfMesh) * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsRU_, (x - SizeOfMesh) * SizeVox2 + (y - SizeOfMesh) * SizeVox + z);
                    }
                } else {
                    if (z == -1) {
                        return GetSafe(NeighborVoxelsR_B, (x - SizeOfMesh) * SizeVox2 + y * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxelsR_F, (x - SizeOfMesh) * SizeVox2 + y * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxelsR__, (x - SizeOfMesh) * SizeVox2 + y * SizeVox + z);
                    }
                }
            } else {
                if (y == -1) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxels_BB, x * SizeVox2 + LowInd * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxels_BF, x * SizeVox2 + LowInd * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxels_B_, x * SizeVox2 + LowInd * SizeVox + z);
                    }
                } else if (y > SizeOfMesh) {
                    if (z == -1) {
                        return GetSafe(NeighborVoxels_UB, x * SizeVox2 + (y - SizeOfMesh) * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxels_UF, x * SizeVox2 + (y - SizeOfMesh) * SizeVox + (z - SizeOfMesh));
                    } else {
                        return GetSafe(NeighborVoxels_U_, x * SizeVox2 + (y - SizeOfMesh) * SizeVox + z);
                    }
                } else {
                    if (z == -1) {
                        return GetSafe(NeighborVoxels__B, x * SizeVox2 + y * SizeVox + LowInd);
                    } else if (z > SizeOfMesh) {
                        return GetSafe(NeighborVoxels__F, x * SizeVox2 + y * SizeVox + (z - SizeOfMesh));
                    } else {
                        return Voxels[x * SizeVox2 + y * SizeVox + z];
                    }
                }
            }
        }

        private Voxel GetSafe(NativeArray<Voxel> array, int index)
        {
            if (array.Length > 1) {
                return array[index];
            }

            return new Voxel();
        }

        private Voxel GetVoxelAtDebug(int x, int y, int z)
        {
            x = Mathf.Max(0, Mathf.Min(x, LowInd));
            y = Mathf.Max(0, Mathf.Min(y, LowInd));
            z = Mathf.Max(0, Mathf.Min(z, LowInd));
            return Voxels[x * SizeVox2 + y * SizeVox + z];
        }
    }
}