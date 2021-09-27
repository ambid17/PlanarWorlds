using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightTileContainer : MonoBehaviour
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

    [SerializeField]
    private Tile _left;
    [SerializeField]
    private Tile _right;
    [SerializeField]
    private Tile _top;
    [SerializeField]
    private Tile _bottom;

    public Dictionary<Vector3Int, Tile> GetHighlightTilesByBrushSize(Vector3Int centerPos,
        int brushSize)
    {
        Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

        if (brushSize == 1)
        {
            tiles.Add(centerPos, centre);
            return tiles;
        }

        // Convert the brush size to the number of tiles from the center
        int sizeFromCenter = brushSize - 1;

        for (int x = -sizeFromCenter; x <= sizeFromCenter; x++)
        {
            for (int y = -sizeFromCenter; y <= sizeFromCenter; y++)
            {
                tiles.Add(new Vector3Int(x + centerPos.x, y + centerPos.y, 0),
                    GetTileByPositionInBrush(x, y, sizeFromCenter));
            }
        }
        

        return tiles;
    }

    public Dictionary<Vector3Int, Tile> GetHighlightTilesForDrag(Vector3Int startPosition, Vector3Int endPosition)
    {
        
        Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

        if (startPosition == endPosition)
        {
            tiles.Add(startPosition, centre);
            return tiles;
        }

        Vector3Int offset = TerrainUtils.GetDragPaintOffset(ref startPosition, ref endPosition);
        
        if(offset.x != 0 && offset.y != 0)
        {
            tiles.Add(startPosition, _bottomLeft);
            tiles.Add(endPosition, _topRight);

            Vector3Int topLeftPosition = new Vector3Int(startPosition.x, startPosition.y + offset.y, 0);
            Vector3Int bottomRightPosition = new Vector3Int(startPosition.x + offset.x, startPosition.y, 0);

            tiles.Add(topLeftPosition, _topLeft);
            tiles.Add(bottomRightPosition, _bottomRight);
        }

        if(offset.x == 0)
        {
            tiles.Add(startPosition, _bottom);
            tiles.Add(endPosition, _top);
        }

        if (offset.y == 0)
        {
            tiles.Add(startPosition, _left);
            tiles.Add(endPosition, _right);
        }

        return tiles;
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
