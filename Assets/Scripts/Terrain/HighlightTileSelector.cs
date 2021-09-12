using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightTileSelector : MonoBehaviour
{
    // Todo: Make this private once the drag erase indicator has been done
    [SerializeField]
    public Tile _centre;

    [SerializeField]
    private Tile _bottomLeft;

    [SerializeField]
    private Tile _bottomRight;

    [SerializeField]
    private Tile _topLeft;

    [SerializeField]
    private Tile _topRight;

    public Dictionary<Vector3Int, Tile> GetTilesByBrushSize(Vector3Int centerPos, int brushSize)
    {
        Dictionary<Vector3Int, Tile> tilesWithPositions = new Dictionary<Vector3Int, Tile>();

        if (brushSize == 1)
        {
            tilesWithPositions.Add(centerPos, _centre);
            return tilesWithPositions;
        }

        // Convert the brush size to the number of tiles from the center
        int sizeFromCenter = brushSize - 1;

        for (int x = -sizeFromCenter; x <= sizeFromCenter; x++)
        {
            for (int y = -sizeFromCenter; y <= sizeFromCenter; y++)
            {
                // Default to an empty cell 
                Tile tile = null;

                // Get either one of the four corner tiles or the default 
                if (x == -sizeFromCenter && y == -sizeFromCenter)
                    tile = _bottomLeft;
                else if (x == sizeFromCenter && y == -sizeFromCenter)
                    tile = _bottomRight;
                else if (x == -sizeFromCenter && y == sizeFromCenter)
                    tile = _topLeft;
                else if (x == sizeFromCenter && y == sizeFromCenter)
                    tile = _topRight;

                tilesWithPositions.Add(new Vector3Int(x + centerPos.x, y + centerPos.y, 0), tile);
            }
        }

        return tilesWithPositions;
    }
}
