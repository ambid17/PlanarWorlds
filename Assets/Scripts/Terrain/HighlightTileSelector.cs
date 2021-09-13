using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightTileSelector : MonoBehaviour
{
    public Tile centre;

    [SerializeField]
    private Tile _bottomLeft;

    [SerializeField]
    private Tile _bottomRight;

    [SerializeField]
    private Tile _topLeft;

    [SerializeField]
    private Tile _topRight;

    public Dictionary<Vector3Int, Tile> GetHighlightTilesByBrushSize(Vector3Int centerPos, int brushSize)
    {
        Dictionary<Vector3Int, Tile> tilesWithPositions = new Dictionary<Vector3Int, Tile>();

        if (brushSize == 1)
        {
            tilesWithPositions.Add(centerPos, centre);
            return tilesWithPositions;
        }

        // Convert the brush size to the number of tiles from the center
        int sizeFromCenter = brushSize - 1;

        for (int x = -sizeFromCenter; x <= sizeFromCenter; x++)
        {
            for (int y = -sizeFromCenter; y <= sizeFromCenter; y++)
            {
                tilesWithPositions.Add(new Vector3Int(x + centerPos.x, y + centerPos.y, 0),
                    GetTileByPositionInBrush(x, y, sizeFromCenter));
            }
        }

        return tilesWithPositions;
    }

    private Tile GetTileByPositionInBrush(int x, int y, int sizeFromCenter)
    {
        // Get either one of the four corner tiles or the default 
        if (x == -sizeFromCenter && y == -sizeFromCenter)
            return _bottomLeft;
        if (x == sizeFromCenter && y == -sizeFromCenter)
            return _bottomRight;
        if (x == -sizeFromCenter && y == sizeFromCenter)
            return _topLeft;
        if (x == sizeFromCenter && y == sizeFromCenter)
            return _topRight;

        return null;
    }
}
