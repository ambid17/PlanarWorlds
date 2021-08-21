using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileGrid", menuName = "ScriptableObjects/TileGrid")]
public class TileGrid : ScriptableObject
{
    public Tile topLeft, topMiddle, topRight, middleLeft, middleMiddle, middleRight, bottomLeft, bottomMiddle, bottomRight;

    public Tile GetTileByPosition(int x, int y, Vector3Int offset)
    {
        bool isLeft = x == 0;
        bool isMiddleX = x > 0 && x < offset.x;
        bool isRight = x == offset.x;

        bool isBottom = y == 0;
        bool isMiddleY = y > 0 && y < offset.y;
        bool isTop = y == offset.y;

        if (isLeft && isBottom)
        {
            return bottomLeft;
        }
        if (isLeft && isMiddleY)
        {
            return middleLeft;
        }
        if (isLeft && isTop)
        {
            return topLeft;
        }

        if (isMiddleX && isBottom)
        {
            return bottomMiddle;
        }
        if (isMiddleX && isMiddleY)
        {
            return middleMiddle;
        }
        if (isMiddleX && isTop)
        {
            return topMiddle;
        }

        if (isRight && isBottom)
        {
            return bottomRight;
        }
        if (isRight && isMiddleY)
        {
            return middleRight;
        }
        if (isRight && isTop)
        {
            return topRight;
        }

        return default;
    }
}
