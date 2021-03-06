using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapExtensions
{
    public static void ClearTiles(this Tilemap self, IEnumerable<Vector3Int> tilePositions)
    {
        foreach (Vector3Int position in tilePositions)
        {
            self.SetTile(position, null);
        }
    }

    /// <summary>
    ///     Constrain tile within a certain Bounding box <see cref="Bounds"/>
    /// </summary>
    /// <param name="self"></param>
    /// <param name="cellPosition"></param>
    /// <param name="tile"></param>
    /// <param name="bounds"></param>
    public static void SetTile(this Tilemap self, Vector3Int cellPosition, Tile tile, Bounds bounds)
    {
        if (!bounds.Contains(self.CellToWorld(cellPosition)))
            return;

        self.SetTile(cellPosition, tile);
    }
}
