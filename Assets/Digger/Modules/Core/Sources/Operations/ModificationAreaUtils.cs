using Unity.Mathematics;

namespace Digger.Modules.Core.Sources.Operations
{
    public static class ModificationAreaUtils
    {
        public static ModificationArea GetSphericalAreaToModify(DiggerSystem digger, float3 position, float radius)
        {
            return GetAABBAreaToModify(digger, position, new float3(radius, radius, radius));
        }

        public static ModificationArea GetAABBAreaToModify(DiggerSystem digger, float3 position, float3 size)
        {
            var operationTerrainPosition = position - new float3(digger.Terrain.transform.position);
            var p = operationTerrainPosition;
            p /= digger.HeightmapScale;

            var voxMargin = new Vector3i(
                (int)((size.x + digger.CutMargin.x) / digger.HeightmapScale.x) + 1,
                (int)((size.y + digger.CutMargin.y) / digger.HeightmapScale.y) + 1,
                (int)((size.z + digger.CutMargin.z) / digger.HeightmapScale.z) + 1);

            var voxMin = new Vector3i(p) - voxMargin;
            var voxMax = new Vector3i(p) + voxMargin;

            var min = voxMin / digger.SizeOfMesh;
            var max = voxMax / digger.SizeOfMesh;
            if (voxMin.x < 0)
                min.x--;
            if (voxMin.y < 0)
                min.y--;
            if (voxMin.z < 0)
                min.z--;
            if (voxMax.x < 0)
                max.x--;
            if (voxMax.y < 0)
                max.y--;
            if (voxMax.z < 0)
                max.z--;

            if (max.x < 0 || max.z < 0 || min.x > digger.TerrainChunkWidth || min.z > digger.TerrainChunkHeight) {
                return new ModificationArea
                {
                    NeedsModification = false
                };
            }

            if (min.x < 0)
                min.x = 0;
            if (min.z < 0)
                min.z = 0;
            if (max.x > digger.TerrainChunkWidth)
                max.x = digger.TerrainChunkWidth;
            if (max.z > digger.TerrainChunkHeight)
                max.z = digger.TerrainChunkHeight;

            var minMaxHeight = digger.GetMinMaxHeightWithin(voxMin, voxMax);
            if (min.y <= minMaxHeight.y && min.y > minMaxHeight.x) {
                min.y = minMaxHeight.x;
            }

            if (max.y >= minMaxHeight.x && max.y < minMaxHeight.y) {
                max.y = minMaxHeight.y;
            }

            return new ModificationArea
            {
                NeedsModification = true,
                Min = min,
                Max = max
            };
        }
    }
}