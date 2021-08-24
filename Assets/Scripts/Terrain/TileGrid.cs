using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileGrid", menuName = "ScriptableObjects/TileGrid")]
public class TileGrid : ScriptableObject
{
    public Sprite sprite;

    // Tiles that make up the grid 
    private readonly Tile _bottomLeft;
    private readonly Tile _bottomMiddle;
    private readonly Tile _bottomRight;

    private readonly Tile _middleLeft;
    private readonly Tile _middleMiddle;
    private readonly Tile _middleRight;

    private readonly Tile _topLeft;
    private readonly Tile _topMiddle;
    private readonly Tile _topRight;

    private readonly Dictionary<(int, int), Tile> TileCoordinateMap = new Dictionary<(int, int), Tile>();

    private void Awake()
    {
        InitCoordinateMap();
    }

    private void InitCoordinateMap()
    {
        TileCoordinateMap.Add((0, 0), _bottomLeft);
        TileCoordinateMap.Add((1, 0), _middleLeft);
        TileCoordinateMap.Add((2, 0), _middleRight);

        TileCoordinateMap.Add((0, 1), _middleLeft);
        TileCoordinateMap.Add((1, 1), _middleMiddle);
        TileCoordinateMap.Add((2, 1), _middleRight);

        TileCoordinateMap.Add((0, 2), _topLeft);
        TileCoordinateMap.Add((1, 2), _topMiddle);
        TileCoordinateMap.Add((2, 2), _topRight);
    }

    public Tile GetTileByPosition(int x, int y, Vector3Int offset)
    {
        if (offset.x == 0 || offset.y == 0)
            return _middleMiddle;

        bool isLeft = x == 0;
        bool isMiddleX = x > 0 && x < offset.x;
        bool isRight = x == offset.x;

        bool isBottom = y == 0;
        bool isMiddleY = y > 0 && y < offset.y;
        bool isTop = y == offset.y;

        if (isLeft && isBottom)
        {
            return _bottomLeft;
        }
        if (isLeft && isMiddleY)
        {
            return _middleLeft;
        }
        if (isLeft && isTop)
        {
            return _topLeft;
        }

        if (isMiddleX && isBottom)
        {
            return _bottomMiddle;
        }
        if (isMiddleX && isMiddleY)
        {
            return _middleMiddle;
        }
        if (isMiddleX && isTop)
        {
            return _topMiddle;
        }

        if (isRight && isBottom)
        {
            return _bottomRight;
        }
        if (isRight && isMiddleY)
        {
            return _middleRight;
        }
        if (isRight && isTop)
        {
            return _topRight;
        }

        return default;
    }
}
