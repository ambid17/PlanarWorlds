using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.TerrainInterface
{
    public class AlphamapsFeeder
    {
        private readonly Dictionary<Vector2i, AlphamapInfo> alphamapsPerChunk = new Dictionary<Vector2i, AlphamapInfo>(new Vector2iComparer());
        private readonly DiggerSystem digger;

        public struct AlphamapInfo
        {
            public float[] AlphamapArray;
            public int3 AlphamapArraySize;
            public int2 AlphamapArrayOrigin;
        }

        public AlphamapsFeeder(DiggerSystem digger)
        {
            this.digger = digger;
        }

        public AlphamapInfo GetAlphamaps(Vector3i chunkPosition, Vector3 worldPosition, int sizeOfMesh)
        {
            var cpos = new Vector2i(chunkPosition.x, chunkPosition.z);
            if (alphamapsPerChunk.TryGetValue(cpos, out var alphamapInfo)) {
                return alphamapInfo;
            }

            Utils.Profiler.BeginSample("GetAlphamaps");
            var tData = digger.Terrain.terrainData;
            var alphamapsSize = new int2(tData.alphamapWidth, tData.alphamapHeight);
            var uvScale = new Vector2(1f / tData.size.x,
                1f / tData.size.z);
            var uv00 = new Vector2((worldPosition.x + 0) * uvScale.x, (worldPosition.z + 0) * uvScale.y);
            var uv11 = new Vector2((worldPosition.x + sizeOfMesh * digger.HeightmapScale.x) * uvScale.x,
                (worldPosition.z + sizeOfMesh * digger.HeightmapScale.z) * uvScale.y);
            var a00 = new Vector2(uv00.x * (alphamapsSize.x - 0), uv00.y * (alphamapsSize.y - 0));
            var a11 = new Vector2(uv11.x * (alphamapsSize.x - 0), uv11.y * (alphamapsSize.y - 0));

            var a00I = new int2(Math.Min(Math.Max(Convert.ToInt32(Math.Floor(a00.x)) - 1, 0), alphamapsSize.x),
                Math.Min(Math.Max(Convert.ToInt32(Math.Floor(a00.y)) - 1, 0), alphamapsSize.y));
            var a11I = new int2(Math.Min(Math.Max(Convert.ToInt32(Math.Ceiling(a11.x)) + 3, 0), alphamapsSize.x),
                Math.Min(Math.Max(Convert.ToInt32(Math.Ceiling(a11.y)) + 3, 0), alphamapsSize.y));

            alphamapInfo = new AlphamapInfo
            {
                AlphamapArray = GrabAlphamaps(a00I, a11I, out var alphamapCount),
                AlphamapArrayOrigin = a00I
            };
            alphamapInfo.AlphamapArraySize.xy = a11I - a00I;
            alphamapInfo.AlphamapArraySize.z = alphamapCount;

            alphamapsPerChunk.Add(cpos, alphamapInfo);
            Utils.Profiler.EndSample();

            return alphamapInfo;
        }

        private float[] GrabAlphamaps(int2 from, int2 to, out int alphamapCount)
        {
            var size = to - from;
            var tData = digger.Terrain.terrainData;
            var tAlphamaps = tData.GetAlphamaps(from.x, from.y, size.x, size.y);

            var sx = tAlphamaps.GetLength(1);
            var sz = tAlphamaps.GetLength(0);
            alphamapCount = tAlphamaps.GetLength(2);
            var alphamaps = new float[size.x * size.y * alphamapCount];
            for (var x = 0; x < size.x; ++x) {
                for (var z = 0; z < size.y; ++z) {
                    for (var map = 0; map < alphamapCount; ++map) {
                        var a = x < sx && z < sz ? tAlphamaps[z, x, map] : 0f;
                        alphamaps[x * size.y * alphamapCount + z * alphamapCount + map] = a;
                    }
                }
            }

            return alphamaps;
        }
    }
}