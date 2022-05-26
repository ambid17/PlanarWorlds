using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    public static class TerrainUtils
    {
        public static Vector3i TerrainRelativePositionToHolePosition(TerrainData terrainData, Vector3 terrainRelativePosition)
        {
            return new Vector3i(terrainRelativePosition.x / terrainData.size.x * terrainData.holesResolution,
                                terrainRelativePosition.y,
                                terrainRelativePosition.z / terrainData.size.z * terrainData.holesResolution);
        }

        public static int2 AlphamapPositionToDetailMapPosition(TerrainData terrainData, int x, int y)
        {
            return new int2(
                x * terrainData.detailWidth / terrainData.alphamapWidth,
                y * terrainData.detailHeight / terrainData.alphamapHeight
            );
        }

        public static Vector3 UVToWorldPosition(TerrainData tData, Vector3 uvPosition)
        {
            return new Vector3(uvPosition.x * tData.size.x,
                               uvPosition.y * tData.size.y,
                               uvPosition.z * tData.size.z);
        }
    }
}