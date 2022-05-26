using UnityEngine;

namespace Digger.Modules.Core.Sources.Polygonizers
{
    public interface IPolygonizer
    {
        Mesh BuildMesh(VoxelChunk chunk, int lod);
    }
}