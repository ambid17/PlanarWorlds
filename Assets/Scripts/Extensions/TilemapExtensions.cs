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
}
