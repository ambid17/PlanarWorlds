using System.Collections.Generic;
using UnityEngine;

namespace Digger.Modules.Core.Sources.TerrainInterface
{
    public class HeightsFeeder
    {
        private readonly Dictionary<Vector2i, float[]> heightsPerChunk = new Dictionary<Vector2i, float[]>(new Vector2iComparer());
        private readonly DiggerSystem digger;
        private readonly TerrainData terrainData;
        private readonly int resolution;
        private readonly float resolutionInv;
        private readonly float toUVs;

        public HeightsFeeder(DiggerSystem digger, int resolution)
        {
            this.digger = digger;
            this.terrainData = digger.Terrain.terrainData;
            this.resolution = resolution;
            this.resolutionInv = 1f / resolution;
            this.toUVs = 1f / (resolution * terrainData.heightmapResolution);
        }

        public float GetHeight(int x, int z)
        {
            if (resolution == 1)
                return terrainData.GetHeight(x, z);

            var xr = x / resolution;
            var zr = z / resolution;
            return Utils.BilinearInterpolate(terrainData.GetHeight(xr, zr),
                terrainData.GetHeight(xr, zr + 1),
                terrainData.GetHeight(xr + 1, zr),
                terrainData.GetHeight(xr + 1, zr + 1),
                x % resolution * resolutionInv,
                z % resolution * resolutionInv);
        }

        public float[] GetHeights(Vector3i chunkPosition, Vector3i chunkVoxelPosition)
        {
            var cpos = new Vector2i(chunkPosition.x, chunkPosition.z);
            if (heightsPerChunk.TryGetValue(cpos, out var heightArray)) {
                return heightArray;
            }
            
            Utils.Profiler.BeginSample("VoxelChunk.FeedHeights");
            var size = digger.SizeVox + 2; // we take heights more then chunk size
            heightArray = new float[size * size];

            Utils.Profiler.BeginSample("VoxelChunk.FeedHeights>Loop");
            for (var xi = 0; xi < size; ++xi) {
                for (var zi = 0; zi < size; ++zi) {
                    heightArray[xi * size + zi] = GetHeight(chunkVoxelPosition.x + xi - 1, chunkVoxelPosition.z + zi - 1);
                }
            }

            Utils.Profiler.EndSample();
            Utils.Profiler.EndSample();
            heightsPerChunk.Add(cpos, heightArray);
            return heightArray;
        }
    }
}