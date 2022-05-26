using System;
using Digger.Modules.Core.Sources.Polygonizers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct MarchingCubesJob : IJobParallelFor
    {
        private struct WorkNorm
        {
            public float3 N0;
            public float3 N1;
            public float3 N2;
            public float3 N3;
            public float3 N4;
            public float3 N5;
            public float3 N6;
            public float3 N7;
        }

        private struct WorkVert
        {
            public float3 V0;
            public float3 V1;
            public float3 V2;
            public float3 V3;
            public float3 V4;
            public float3 V5;
            public float3 V6;
            public float3 V7;
            public float3 V8;
            public float3 V9;
            public float3 V10;
            public float3 V11;

            public float3 this[int i] {
                get {
                    switch (i) {
                        case 0: return V0;
                        case 1: return V1;
                        case 2: return V2;
                        case 3: return V3;
                        case 4: return V4;
                        case 5: return V5;
                        case 6: return V6;
                        case 7: return V7;
                        case 8: return V8;
                        case 9: return V9;
                        case 10: return V10;
                        case 11: return V11;
                    }

                    return float3.zero; // don't throw an exception to allow Burst compilation
                }
            }
        }

        private struct WorkVertIndices
        {
            public ushort Vi0;
            public ushort Vi1;
            public ushort Vi2;
            public ushort Vi3;
            public ushort Vi4;
            public ushort Vi5;
            public ushort Vi6;
            public ushort Vi7;
            public ushort Vi8;
            public ushort Vi9;
            public ushort Vi10;
            public ushort Vi11;

            public ushort this[int i] {
                get {
                    switch (i) {
                        case 0: return Vi0;
                        case 1: return Vi1;
                        case 2: return Vi2;
                        case 3: return Vi3;
                        case 4: return Vi4;
                        case 5: return Vi5;
                        case 6: return Vi6;
                        case 7: return Vi7;
                        case 8: return Vi8;
                        case 9: return Vi9;
                        case 10: return Vi10;
                        case 11: return Vi11;
                    }

                    return 0; // don't throw an exception to allow Burst compilation
                }
            }
        }

        public int SizeVox;
        public int SizeVox2;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<int> edgeTable;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<int> triTable;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<float3> corners;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<Voxel> voxels;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<float> alphamaps;

        [ReadOnly] [NativeDisableParallelForRestriction]
        private NativeArray<float3> normals;

        private int2 alphamapsSize;
        private int3 localAlphamapsSize;

        private NativeCollections.NativeCounter.Concurrent vertexCounter;
        private NativeCollections.NativeCounter.Concurrent triangleCounter;

        [NativeDisableParallelForRestriction]
        public NativeArray<VertexData> outVertexData;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<ushort> outTriangles;

        private float3 chunkWorldPosition;
        private float3 scale;
        private float2 uvScale;
        private int2 alphamapOrigin;
        private int lod;
        private TerrainMaterialType materialType;

        public float Isovalue;
        public byte AlteredOnly;
        public byte FullOutput;
        public byte IsBuiltInHDRP;
        public byte IsLowPolyStyle;


        public MarchingCubesJob(NativeArray<int> edgeTable,
            NativeArray<int> triTable,
            NativeArray<float3> corners,
            NativeCollections.NativeCounter.Concurrent vertexCounter,
            NativeCollections.NativeCounter.Concurrent triangleCounter,
            NativeArray<Voxel> voxels,
            NativeArray<float3> normals,
            NativeArray<float> alphamaps,
            PolyOut o,
            Vector3 scale,
            Vector2 uvScale,
            Vector3 chunkWorldPosition,
            int lod,
            int2 alphamapOrigin,
            int2 alphamapsSize,
            int3 localAlphamapsSize,
            TerrainMaterialType materialType)
        {
            this.edgeTable = edgeTable;
            this.triTable = triTable;
            this.corners = corners;
            this.vertexCounter = vertexCounter;
            this.triangleCounter = triangleCounter;
            this.voxels = voxels;
            this.normals = normals;
            this.alphamaps = alphamaps;

            this.outVertexData = o.outVertexData;
            this.outTriangles = o.outTriangles;

            this.SizeVox = 0;
            this.SizeVox2 = 0;
            this.Isovalue = 0;
            this.AlteredOnly = 1;
            this.FullOutput = 1;
            this.scale = scale;
            this.lod = lod;
            this.alphamapsSize = alphamapsSize;
            this.localAlphamapsSize = localAlphamapsSize;
            this.uvScale = uvScale;
            this.alphamapOrigin = alphamapOrigin;
            this.chunkWorldPosition = chunkWorldPosition;
            this.materialType = materialType;
            this.IsBuiltInHDRP = 0;
            this.IsLowPolyStyle = 0;
        }

        private static Voxel GetProminentVoxel(Voxel vA, Voxel vB)
        {
            var altA = vA.Alteration;
            if (altA == Voxel.OnSurface) return vA;

            var altB = vB.Alteration;
            if (altB == Voxel.OnSurface) return vB;

            // Use 'Altered' value of the most altered voxel to avoid artifacts (ie. prefer voxel which has been actually altered by VoxelModificationJob over a "virgin" voxel)
            if (altA > altB)
                return vA;

            if (altA < altB)
                return vB;

            return math.abs(vA.Value) < math.abs(vB.Value) ? vA : vB;
        }

        private float3 VertexInterp(float3 p1, float3 p2, Voxel vA, Voxel vB)
        {
            if (Utils.Approximately(vA.Value, 0))
                return p1;
            if (Utils.Approximately(vB.Value, 0))
                return p2;

            var mu = (Isovalue - vA.Value) / (vB.Value - vA.Value);
            return p1 + mu * (p2 - p1);
        }

        private float3 ComputeNormalAt(int xi, int yi, int zi, float voxelOriginValue)
        {
            return math.normalize(new float3(
                voxels[(xi + 1) * SizeVox2 + yi * SizeVox + zi].Value - voxelOriginValue,
                voxels[xi * SizeVox2 + (yi + 1) * SizeVox + zi].Value - voxelOriginValue,
                voxels[xi * SizeVox2 + yi * SizeVox + (zi + 1)].Value - voxelOriginValue
            ));
        }


        private unsafe void ComputeUVsAndColor(int3 pi, VertexData* v, float3 vertexRelativePos, Voxel voxel)
        {
            var alt = voxel.Alteration;
            if (alt == Voxel.Unaltered || alt == Voxel.OnSurface) {
                v->Normal = InterpolateNormal(pi.x, pi.z, vertexRelativePos.xz);
            }

            if (materialType == TerrainMaterialType.MicroSplat) {
                ComputeUVsAndColorForMicroSplat(v, voxel);
                return;
            }

            var uv = new float2((chunkWorldPosition.x + v->Vertex.x) * uvScale.x,
                (chunkWorldPosition.z + v->Vertex.z) * uvScale.y);

            v->UV1 = uv;

            if (alt == Voxel.Unaltered || alt == Voxel.OnSurface) {
                // near the terrain surface -> set same texture
                v->Color = GetControlAt(uv, 0);
                v->UV2 = GetControlAt(uv, 1);
                v->UV3 = GetControlAt(uv, 2);
                v->UV4 = GetControlAt(uv, 3);
            } else {
                var firstTextureIndex = voxel.FirstTextureIndex;
                var secondTextureIndex = voxel.SecondTextureIndex;
                var lerp = voxel.NormalizedTextureLerp;
                v->Color = GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 0);
                v->UV2 = GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 1);
                v->UV3 = GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 2);
                v->UV4 = GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 3);
            }

            if (IsBuiltInHDRP == 1) {
                v->UV3.xy = v->UV2.xy;
                v->UV4.xy = v->UV2.zw;
            }
        }

        private unsafe void ComputeUVsAndColorForMicroSplat(VertexData* v, Voxel voxel)
        {
            var uv = new float2((chunkWorldPosition.x + v->Vertex.x) * uvScale.x,
                (chunkWorldPosition.z + v->Vertex.z) * uvScale.y);
            v->UV1 = uv;

            if (voxel.Alteration == Voxel.Unaltered || voxel.Alteration == Voxel.OnSurface) {
                // near the terrain surface -> set same texture
                v->Color = new float4(
                    EncodeToFloat(GetControlAt(uv, 0)),
                    EncodeToFloat(GetControlAt(uv, 1)),
                    EncodeToFloat(GetControlAt(uv, 2)),
                    EncodeToFloat(GetControlAt(uv, 3))
                );
                v->UV2 = new float4(
                    0f,
                    0f,
                    EncodeToFloat(GetControlAt(uv, 4)),
                    EncodeToFloat(GetControlAt(uv, 5))
                );
                v->UV3 = new float4(
                    0f,
                    0f,
                    EncodeToFloat(GetControlAt(uv, 6)),
                    EncodeToFloat(GetControlAt(uv, 7))
                );
            } else {
                var firstTextureIndex = voxel.FirstTextureIndex;
                var secondTextureIndex = voxel.SecondTextureIndex;
                var lerp = voxel.NormalizedTextureLerp;

                v->Color = new float4(
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 0)),
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 1)),
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 2)),
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 3))
                );
                v->UV2 = new float4(
                    0f,
                    0f,
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 4)),
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 5))
                );
                v->UV3 = new float4(
                    0f,
                    0f,
                    EncodeToFloat(GetControlFor(firstTextureIndex, secondTextureIndex, lerp, 6)),
                    EncodeToFloat(new float4(voxel.NormalizedWetnessWeight, voxel.NormalizedPuddlesWeight,
                        voxel.NormalizedStreamsWeight, voxel.NormalizedLavaWeight))
                );
            }
        }

        private float4 GetControlAt(float2 uv, int index)
        {
            // adjust splatUVs so the edges of the terrain tile lie on pixel centers
            var splatUV = new float2(uv.x * (alphamapsSize.x - 1), uv.y * (alphamapsSize.y - 1));

            var wx = math.clamp(Convert.ToInt32(math.floor(splatUV.x)), 0, alphamapsSize.x - 2);
            var wz = math.clamp(Convert.ToInt32(math.floor(splatUV.y)), 0, alphamapsSize.y - 2);
            var relPos = splatUV - new float2(wx, wz);
            var x = math.clamp(wx - alphamapOrigin.x, 0, localAlphamapsSize.x - 2);
            var z = math.clamp(wz - alphamapOrigin.y, 0, localAlphamapsSize.y - 2);

            index *= 4;

            var mapCount = localAlphamapsSize.z;
            var ctrl = float4.zero;
            if (index + 0 < mapCount) {
                ctrl[0] = InterpolateAlphamap(index, mapCount, x, z, 0, relPos);
            }

            if (index + 1 < mapCount) {
                ctrl[1] = InterpolateAlphamap(index, mapCount, x, z, 1, relPos);
            }

            if (index + 2 < mapCount) {
                ctrl[2] = InterpolateAlphamap(index, mapCount, x, z, 2, relPos);
            }

            if (index + 3 < mapCount) {
                ctrl[3] = InterpolateAlphamap(index, mapCount, x, z, 3, relPos);
            }

            return ctrl;
        }

        private float InterpolateAlphamap(int index, int mapCount, int x, int z, int i, float2 relPos)
        {
            var a00 = alphamaps[x * localAlphamapsSize.y * mapCount + z * mapCount + index + i];
            var a01 = alphamaps[(x + 1) * localAlphamapsSize.y * mapCount + z * mapCount + index + i];
            var a10 = alphamaps[x * localAlphamapsSize.y * mapCount + (z + 1) * mapCount + index + i];
            var a11 = alphamaps[(x + 1) * localAlphamapsSize.y * mapCount + (z + 1) * mapCount + index + i];
            return Utils.BilinearInterpolate(a00, a01, a10, a11, relPos.y, relPos.x);
        }

        private float3 InterpolateNormal(int x, int z, float2 relPos)
        {
            if (relPos.x < 0 || relPos.x > 1f)
                return new float3(1, 0, 0);

            if (relPos.y < 0 || relPos.y > 1f)
                return new float3(0, 0, 1);

            var a00 = normals[Utils.XZToNormalIndex(x, z, SizeVox)];
            var a01 = normals[Utils.XZToNormalIndex(x + 1, z, SizeVox)];
            var a10 = normals[Utils.XZToNormalIndex(x, z + 1, SizeVox)];
            var a11 = normals[Utils.XZToNormalIndex(x + 1, z + 1, SizeVox)];
            return Utils.BilinearInterpolate(a00, a01, a10, a11, relPos.y, relPos.x);
        }

        private static float4 GetControlFor(uint firstTextureIndex, uint secondTextureIndex, float lerp, int index)
        {
            var ctrl = new float4(0, 0, 0, 0);
            if (index * 4 == firstTextureIndex)
                ctrl.x = 1f - lerp;
            else if (index * 4 == secondTextureIndex)
                ctrl.x = lerp;

            if (index * 4 + 1 == firstTextureIndex)
                ctrl.y = 1f - lerp;
            else if (index * 4 + 1 == secondTextureIndex)
                ctrl.y = lerp;

            if (index * 4 + 2 == firstTextureIndex)
                ctrl.z = 1f - lerp;
            else if (index * 4 + 2 == secondTextureIndex)
                ctrl.z = lerp;

            if (index * 4 + 3 == firstTextureIndex)
                ctrl.w = 1f - lerp;
            else if (index * 4 + 3 == secondTextureIndex)
                ctrl.w = lerp;

            return ctrl;
        }

        private static float EncodeToFloat(float4 enc)
        {
            var ex = (uint)(enc.x * 255);
            var ey = (uint)(enc.y * 255);
            var ez = (uint)(enc.z * 255);
            var ew = (uint)(enc.w * 255);
            var v = (ex << 24) + (ey << 16) + (ez << 8) + ew;
            return v / (256.0f * 256.0f * 256.0f * 256.0f);
        }


        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);

            if (pi.x >= SizeVox - lod - 1 ||
                pi.y >= SizeVox - lod - 1 ||
                pi.z >= SizeVox - lod - 1 ||
                pi.x % lod != 0 || pi.y % lod != 0 || pi.z % lod != 0)
                return;

            var v0 = voxels[pi.x * SizeVox * SizeVox + pi.y * SizeVox + pi.z];
            var v1 = voxels[(pi.x + lod) * SizeVox * SizeVox + pi.y * SizeVox + pi.z];
            var v2 = voxels[(pi.x + lod) * SizeVox * SizeVox + pi.y * SizeVox + (pi.z + lod)];
            var v3 = voxels[pi.x * SizeVox * SizeVox + pi.y * SizeVox + (pi.z + lod)];
            var v4 = voxels[pi.x * SizeVox * SizeVox + (pi.y + lod) * SizeVox + pi.z];
            var v5 = voxels[(pi.x + lod) * SizeVox * SizeVox + (pi.y + lod) * SizeVox + pi.z];
            var v6 = voxels[(pi.x + lod) * SizeVox * SizeVox + (pi.y + lod) * SizeVox + (pi.z + lod)];
            var v7 = voxels[pi.x * SizeVox * SizeVox + (pi.y + lod) * SizeVox + (pi.z + lod)];

            var alt0 = v0.Alteration;
            var alt1 = v1.Alteration;
            var alt2 = v2.Alteration;
            var alt3 = v3.Alteration;
            var alt4 = v4.Alteration;
            var alt5 = v5.Alteration;
            var alt6 = v6.Alteration;
            var alt7 = v7.Alteration;
            if (alt0 == Voxel.Hole ||
                alt1 == Voxel.Hole ||
                alt2 == Voxel.Hole ||
                alt3 == Voxel.Hole ||
                alt4 == Voxel.Hole ||
                alt5 == Voxel.Hole ||
                alt6 == Voxel.Hole ||
                alt7 == Voxel.Hole)
                return;

            if (AlteredOnly == 1) {
                if (alt0 == Voxel.Unaltered &&
                    alt1 == Voxel.Unaltered &&
                    alt2 == Voxel.Unaltered &&
                    alt3 == Voxel.Unaltered &&
                    alt4 == Voxel.Unaltered &&
                    alt5 == Voxel.Unaltered &&
                    alt6 == Voxel.Unaltered &&
                    alt7 == Voxel.Unaltered)
                    return;
            }

            var cubeindex = 0;
            if (v0.IsInside) cubeindex |= 1;
            if (v1.IsInside) cubeindex |= 2;
            if (v2.IsInside) cubeindex |= 4;
            if (v3.IsInside) cubeindex |= 8;
            if (v4.IsInside) cubeindex |= 16;
            if (v5.IsInside) cubeindex |= 32;
            if (v6.IsInside) cubeindex |= 64;
            if (v7.IsInside) cubeindex |= 128;

            /* Cube is entirely in/out of the surface */
            if (cubeindex == 0 || cubeindex == 255)
                return;

            var position = new float3
            {
                x = pi.x,
                y = pi.y,
                z = pi.z
            };

            var voxelNorm = new WorkNorm
            {
                N0 = ComputeNormalAt(pi.x, pi.y, pi.z, v0.Value),
                N1 = ComputeNormalAt((pi.x + lod), pi.y, pi.z, v1.Value),
                N2 = ComputeNormalAt((pi.x + lod), pi.y, (pi.z + lod), v2.Value),
                N3 = ComputeNormalAt(pi.x, pi.y, (pi.z + lod), v3.Value),
                N4 = ComputeNormalAt(pi.x, (pi.y + lod), pi.z, v4.Value),
                N5 = ComputeNormalAt((pi.x + lod), (pi.y + lod), pi.z, v5.Value),
                N6 = ComputeNormalAt((pi.x + lod), (pi.y + lod), (pi.z + lod), v6.Value),
                N7 = ComputeNormalAt(pi.x, (pi.y + lod), (pi.z + lod), v7.Value)
            };

            var wVert = new WorkVert();
            var wVertIndices = new WorkVertIndices();

            unsafe {
                /* Find the vertices where the surface intersects the cube */
                if ((edgeTable[cubeindex] & 1) != 0) {
                    var norm = VertexInterp(voxelNorm.N0, voxelNorm.N1, v0, v1);
                    var relPos = VertexInterp(corners[0], corners[1], v0, v1);
                    wVert.V0 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V0,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi0 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v0, v1);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 2) != 0) {
                    var norm = VertexInterp(voxelNorm.N1, voxelNorm.N2, v1, v2);
                    var relPos = VertexInterp(corners[1], corners[2], v1, v2);
                    wVert.V1 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V1,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi1 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v1, v2);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 4) != 0) {
                    var norm = VertexInterp(voxelNorm.N2, voxelNorm.N3, v2, v3);
                    var relPos = VertexInterp(corners[2], corners[3], v2, v3);
                    wVert.V2 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V2,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi2 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v2, v3);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 8) != 0) {
                    var norm = VertexInterp(voxelNorm.N3, voxelNorm.N0, v3, v0);
                    var relPos = VertexInterp(corners[3], corners[0], v3, v0);
                    wVert.V3 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V3,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi3 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v3, v0);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 16) != 0) {
                    var norm = VertexInterp(voxelNorm.N4, voxelNorm.N5, v4, v5);
                    var relPos = VertexInterp(corners[4], corners[5], v4, v5);
                    wVert.V4 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V4,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi4 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v4, v5);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 32) != 0) {
                    var norm = VertexInterp(voxelNorm.N5, voxelNorm.N6, v5, v6);
                    var relPos = VertexInterp(corners[5], corners[6], v5, v6);
                    wVert.V5 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V5,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi5 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v5, v6);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 64) != 0) {
                    var norm = VertexInterp(voxelNorm.N6, voxelNorm.N7, v6, v7);
                    var relPos = VertexInterp(corners[6], corners[7], v6, v7);
                    wVert.V6 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V6,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi6 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v6, v7);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 128) != 0) {
                    var norm = VertexInterp(voxelNorm.N7, voxelNorm.N4, v7, v4);
                    var relPos = VertexInterp(corners[7], corners[4], v7, v4);
                    wVert.V7 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V7,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi7 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v7, v4);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 256) != 0) {
                    var norm = VertexInterp(voxelNorm.N0, voxelNorm.N4, v0, v4);
                    var relPos = VertexInterp(corners[0], corners[4], v0, v4);
                    wVert.V8 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V8,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi8 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v0, v4);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 512) != 0) {
                    var norm = VertexInterp(voxelNorm.N1, voxelNorm.N5, v1, v5);
                    var relPos = VertexInterp(corners[1], corners[5], v1, v5);
                    wVert.V9 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V9,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi9 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v1, v5);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 1024) != 0) {
                    var norm = VertexInterp(voxelNorm.N2, voxelNorm.N6, v2, v6);
                    var relPos = VertexInterp(corners[2], corners[6], v2, v6);
                    wVert.V10 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V10,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi10 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v2, v6);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }

                if ((edgeTable[cubeindex] & 2048) != 0) {
                    var norm = VertexInterp(voxelNorm.N3, voxelNorm.N7, v3, v7);
                    var relPos = VertexInterp(corners[3], corners[7], v3, v7);
                    wVert.V11 = scale * (position + relPos * lod);
                    var vertIndex = vertexCounter.Increment() - 1;
                    if (vertIndex >= PolyOut.MaxVertexCount) return;
                    var v = new VertexData
                    {
                        Vertex = wVert.V11,
                        Normal = math.normalize(norm)
                    };
                    wVertIndices.Vi11 = (ushort)vertIndex;
                    if (FullOutput == 1) {
                        var vox = GetProminentVoxel(v3, v7);
                        ComputeUVsAndColor(pi, &v, relPos, vox);
                    }

                    outVertexData[vertIndex] = v;
                }
            }

            /* Create the triangles */
            if (triTable[cubeindex * 16 + 0] == -1) return;
            AddTriangle(0, cubeindex, ref wVert, ref wVertIndices);
            if (triTable[cubeindex * 16 + 3] == -1) return;
            AddTriangle(3, cubeindex, ref wVert, ref wVertIndices);
            if (triTable[cubeindex * 16 + 6] == -1) return;
            AddTriangle(6, cubeindex, ref wVert, ref wVertIndices);
            if (triTable[cubeindex * 16 + 9] == -1) return;
            AddTriangle(9, cubeindex, ref wVert, ref wVertIndices);
            if (triTable[cubeindex * 16 + 12] == -1) return;
            AddTriangle(12, cubeindex, ref wVert, ref wVertIndices);
        }

        private void AddTriangle(int i, int cubeindex, ref WorkVert wVert, ref WorkVertIndices wVertIndices)
        {
            var i1 = triTable[cubeindex * 16 + (i + 0)];
            var i2 = triTable[cubeindex * 16 + (i + 1)];
            var i3 = triTable[cubeindex * 16 + (i + 2)];
            var vert1 = wVert[i1];
            var vert2 = wVert[i2];
            var vert3 = wVert[i3];
            if (!Utils.Approximately(vert1, vert2) &&
                !Utils.Approximately(vert2, vert3) &&
                !Utils.Approximately(vert1, vert3) &&
                !Utils.AreColinear(vert1, vert2, vert3)) {
                var triIndex = triangleCounter.Increment() - 3;
                if (triIndex + 2 >= PolyOut.MaxTriangleCount) return;
                outTriangles[triIndex + 0] = wVertIndices[i1];
                outTriangles[triIndex + 1] = wVertIndices[i2];
                outTriangles[triIndex + 2] = wVertIndices[i3];
            }

            if (IsLowPolyStyle == 1) {
                var nrm = math.normalize(math.cross(vert2 - vert1, vert3 - vert1));
                var vertexData = outVertexData[wVertIndices[i1]];
                vertexData.Normal = nrm;
                outVertexData[wVertIndices[i1]] = vertexData;
                vertexData = outVertexData[wVertIndices[i2]];
                vertexData.Normal = nrm;
                outVertexData[wVertIndices[i2]] = vertexData;
                vertexData = outVertexData[wVertIndices[i3]];
                vertexData.Normal = nrm;
                outVertexData[wVertIndices[i3]] = vertexData;
            }
        }
    }
}