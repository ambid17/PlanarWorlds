using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.Polygonizers
{
    public class MarchingCubesPolygonizer : IPolygonizer
    {
        private readonly byte isLowPolyStyle;

        public MarchingCubesPolygonizer(bool lowPolyStyle = false)
        {
            this.isLowPolyStyle = (byte)(lowPolyStyle ? 1 : 0);
        }

        public Mesh BuildMesh(VoxelChunk chunk, int lod)
        {
            var edgeTable = NativeCollectionsPool.Instance.GetMCEdgeTable();
            var triTable = NativeCollectionsPool.Instance.GetMCTriTable();
            var corners = NativeCollectionsPool.Instance.GetMCCorners();
            var voxels = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob);
            var normals = new NativeArray<float3>(chunk.NormalArray, Allocator.TempJob);
            var alphamaps = new NativeArray<float>(chunk.AlphamapArray, Allocator.TempJob);
            var mcOut = NativeCollectionsPool.Instance.GetPolyOut();
            var vertexCounter = new NativeCounter(Allocator.TempJob);
            var triangleCounter = new NativeCounter(Allocator.TempJob, 3);

            var tData = chunk.Digger.Terrain.terrainData;
            var alphamapsSize = new int2(tData.alphamapWidth, tData.alphamapHeight);
            var uvScale = new Vector2(1f / tData.size.x, 1f / tData.size.z);
            var scale = new float3(chunk.Digger.HeightmapScale); // { y = 1f };

            // for retro-compatibility
            if (lod <= 0) lod = 1;

            // Set up the job data
            var jobData = new MarchingCubesJob(edgeTable,
                triTable,
                corners,
                vertexCounter.ToConcurrent(),
                triangleCounter.ToConcurrent(),
                voxels,
                normals,
                alphamaps,

                // out params
                mcOut,

                // misc
                scale,
                uvScale,
                chunk.WorldPosition,
                lod,
                chunk.AlphamapArrayOrigin,
                alphamapsSize,
                chunk.AlphamapArraySize,
                chunk.Digger.MaterialType);

            jobData.SizeVox = chunk.SizeVox;
            jobData.SizeVox2 = chunk.SizeVox * chunk.SizeVox;
            jobData.Isovalue = 0f;
            jobData.AlteredOnly = 1;
            jobData.FullOutput = 1;
            jobData.IsBuiltInHDRP = (byte)(chunk.Digger.MaterialType == TerrainMaterialType.HDRP ? 1 : 0);
            jobData.IsLowPolyStyle = isLowPolyStyle;


            // Schedule the job
            var currentJobHandle = jobData.Schedule(voxels.Length, 4);
            // Wait for the job to complete
            currentJobHandle.Complete();

            var vertexCount = vertexCounter.Count;
            var triangleCount = triangleCounter.Count;

            voxels.Dispose();
            normals.Dispose();
            alphamaps.Dispose();
            vertexCounter.Dispose();
            triangleCounter.Dispose();

            return VoxelChunk.ToMesh(chunk, mcOut, vertexCount, triangleCount);
        }
    }
}