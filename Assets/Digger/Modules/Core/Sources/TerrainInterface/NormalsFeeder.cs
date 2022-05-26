using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.TerrainInterface
{
    public class NormalsFeeder
    {
        private readonly Dictionary<Vector2i, float3[]> normalsPerChunk = new Dictionary<Vector2i, float3[]>(new Vector2iComparer());
        private readonly DiggerSystem digger;
        private readonly TerrainData terrainData;
        private readonly double toUVs;

        public NormalsFeeder(DiggerSystem digger, int resolution)
        {
            this.digger = digger;
            this.terrainData = digger.Terrain.terrainData;
            this.toUVs = 1.0 / (resolution * (terrainData.heightmapResolution-1));
        }

        private float3 GetNormal(int x, int z)
        {
            return terrainData.GetInterpolatedNormal((float)(x * toUVs), (float)(z * toUVs));
        }

        public float3[] GetNormals(Vector3i chunkPosition, Vector3i chunkVoxelPosition)
        {
            var cpos = new Vector2i(chunkPosition.x, chunkPosition.z);
            if (normalsPerChunk.TryGetValue(cpos, out var normalArray)) {
                return normalArray;
            }
            
            Utils.Profiler.BeginSample("VoxelChunk.GetNormals");
            var size = digger.SizeVox + 2; // we take heights more then chunk size
            normalArray = new float3[size * size];

            Utils.Profiler.BeginSample("VoxelChunk.GetNormals>Loop");
            for (var xi = 0; xi < size; ++xi) {
                for (var zi = 0; zi < size; ++zi) {
                    normalArray[xi * size + zi] = GetNormal(chunkVoxelPosition.x + xi - 1, chunkVoxelPosition.z + zi - 1);
                }
            }

            Utils.Profiler.EndSample();
            Utils.Profiler.EndSample();
            normalsPerChunk.Add(cpos, normalArray);
            return normalArray;
        }
    }
}