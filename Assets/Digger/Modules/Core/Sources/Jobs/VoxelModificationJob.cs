using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct VoxelModificationJob : IJobParallelFor, IDisposable
    {
        public int SizeVox;
        public int SizeVox2;
        public BrushType Brush;
        public ActionType Action;
        public float3 HeightmapScale;
        public float3 Center;
        public float3 Size;
        public bool UpsideDown;
        public float Intensity;
        public bool IsTargetIntensity;
        public float ChunkAltitude;
        public uint TextureIndex;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        public NativeArray<Voxel> Voxels;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> Holes;
        
        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> NewHolesConcurrentCounter;

        private double coneAngle;
        private float upsideDownSign;

        public void PostConstruct()
        {
            if (Size.y > 0.1f)
                coneAngle = Math.Atan((double)Size.x / Size.y);
            upsideDownSign = UpsideDown ? -1f : 1f;
        }

        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);
            var p = pi * HeightmapScale;
            var terrainHeight = Heights[Utils.XYZToHeightIndex(pi, SizeVox)];
            var terrainHeightValue = p.y + ChunkAltitude - terrainHeight;

            float distance;
            switch (Brush) {
                case BrushType.Sphere:
                    distance = ComputeSphereDistances(p);
                    break;
                case BrushType.HalfSphere:
                    distance = ComputeHalfSphereDistances(p);
                    break;
                case BrushType.RoundedCube:
                    distance = ComputeCubeDistances(p);
                    break;
                case BrushType.Stalagmite:
                    distance = ComputeConeDistances(p);
                    break;
                default:
                    return; // never happens
            }

            Voxel voxel;
            switch (Action) {
                case ActionType.Add:
                case ActionType.Dig:
                    var intensityWeight = math.max(1f, math.abs(terrainHeightValue) * 0.75f);
                    voxel = ApplyDigAdd(index, Action == ActionType.Dig, distance, intensityWeight);
                    break;
                case ActionType.Paint:
                    voxel = ApplyPaint(index, distance);
                    break;
                case ActionType.PaintHoles:
                    voxel = ApplyPaintHoles(index, pi, p, distance);
                    break;
                case ActionType.Reset:
                    voxel = ApplyResetBrush(index, pi, p, distance);
                    break;
                default:
                    return; // never happens
            }


            if (voxel.Alteration != Voxel.Unaltered) {
                voxel = Utils.AdjustAlteration(voxel, pi, HeightmapScale.y, p.y + ChunkAltitude, terrainHeightValue, SizeVox, Heights);
            }

            if (Action != ActionType.Reset && (voxel.IsAlteredNearBelowSurface || voxel.IsAlteredNearAboveSurface)) {
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

            Voxels[index] = voxel;
        }


        private float ComputeSphereDistances(float3 p)
        {
            var radius = Size.x;
            var radiusHeightRatio = radius / math.max(Size.y, 0.01f);
            var vec = p - Center;
            var distance = math.sqrt(vec.x * vec.x + vec.y * vec.y * radiusHeightRatio * radiusHeightRatio + vec.z * vec.z);
            return radius - distance;
        }

        private float ComputeHalfSphereDistances(float3 p)
        {
            var vec = p - Center;
            var distance = math.sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
            return math.min(Size.x - distance, vec.y);
        }

        private float ComputeCubeDistances(float3 p)
        {
            var vec = p - Center;
            return math.min(math.min(Size.x - math.abs(vec.x), Size.y - math.abs(vec.y)), Size.z - math.abs(vec.z));
        }

        private float ComputeConeDistances(float3 p)
        {
            var coneVertex = Center + new float3(0, upsideDownSign * Size.y * 0.95f, 0);
            var vec = p - coneVertex;
            var distance = math.sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
            var flatDistance = math.sqrt(vec.x * vec.x + vec.z * vec.z);
            var pointAngle = Math.Asin((double)flatDistance / distance);
            var d = -distance * Math.Sin(math.abs(pointAngle - coneAngle)) * Math.Sign(pointAngle - coneAngle);
            return math.min(math.min((float)d, Size.y + upsideDownSign * vec.y), -upsideDownSign * vec.y);
        }

        private Voxel ApplyDigAdd(int index, bool dig, float distance, float intensityWeight)
        {
            var voxel = Voxels[index];
            var currentValF = voxel.Value;

            if (dig) {
                voxel.Value = math.max(currentValF, currentValF + Intensity * intensityWeight * distance);
            } else {
                voxel.Value = math.min(currentValF, currentValF - Intensity * intensityWeight * distance);
            }

            if (distance >= 0) {
                voxel.Alteration = Voxel.FarAboveSurface;
                voxel.AddTexture(TextureIndex, 1f);
            }

            return voxel;
        }

        private Voxel ApplyPaint(int index, float distance)
        {
            var voxel = Voxels[index];

            if (distance >= 0) {
                if (IsTargetIntensity) {
                    if (TextureIndex < 28) {
                        voxel.SetTexture(TextureIndex, Intensity);
                    } else if (TextureIndex == 28) {
                        voxel.NormalizedWetnessWeight = Intensity;
                    } else if (TextureIndex == 29) {
                        voxel.NormalizedPuddlesWeight = Intensity;
                    } else if (TextureIndex == 30) {
                        voxel.NormalizedStreamsWeight = Intensity;
                    } else if (TextureIndex == 31) {
                        voxel.NormalizedLavaWeight = Intensity;
                    }
                } else {
                    if (TextureIndex < 28) {
                        voxel.AddTexture(TextureIndex, Intensity);
                    } else if (TextureIndex == 28) {
                        voxel.NormalizedWetnessWeight += Intensity;
                    } else if (TextureIndex == 29) {
                        voxel.NormalizedPuddlesWeight += Intensity;
                    } else if (TextureIndex == 30) {
                        voxel.NormalizedStreamsWeight += Intensity;
                    } else if (TextureIndex == 31) {
                        voxel.NormalizedLavaWeight += Intensity;
                    }
                }
            }

            return voxel;
        }

        private Voxel ApplyPaintHoles(int index, int3 pi, float3 p, float distance)
        {
            var voxel = Voxels[index];
            if (distance >= 0 && Intensity > 0 && voxel.Alteration != Voxel.Unaltered) {
                voxel.Alteration = Voxel.Hole;
            } else if (distance >= 0 && Intensity < 0 && voxel.Alteration == Voxel.Hole) {
                var onSurface = Utils.IsOnSurface(pi, HeightmapScale.y, p.y + ChunkAltitude, SizeVox, Heights);
                voxel.Alteration = onSurface ? Voxel.OnSurface : Voxel.FarAboveSurface;
            }

            return voxel;
        }

        private Voxel ApplyResetBrush(int index, int3 pi, float3 p, float distance)
        {
            if (distance >= 0) {
                var height = Heights[Utils.XYZToHeightIndex(pi, SizeVox)];
                var voxel = new Voxel(p.y + ChunkAltitude - height);
                if (Utils.IsOnSurface(pi, HeightmapScale.y, p.y + ChunkAltitude, SizeVox, Heights)) {
                    NativeCollections.Utils.SetZeroAt(Holes, Utils.XZToHoleIndex(pi.x, pi.z, SizeVox));
                }

                return voxel;
            }

            return Voxels[index];
        }

        public void Dispose()
        {
            Heights.Dispose();
            Voxels.Dispose();
            Holes.Dispose();
        }
    }
}