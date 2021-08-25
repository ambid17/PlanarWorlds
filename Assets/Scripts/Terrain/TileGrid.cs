using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileGrid", menuName = "ScriptableObjects/TileGrid")]
public class TileGrid : ScriptableObject
{
    [SerializeField] 
    private Sprite _sprite;

    // Default to the middle sprite in the grid 
    public Sprite Sprite => _sprite != null ? _sprite : middleMiddle.sprite;

    #region Grid Tiles 
    public Tile bottomLeft;
    public Tile bottomMiddle;
    public Tile bottomRight;

    public Tile middleLeft;
    public Tile middleMiddle;
    public Tile middleRight;

    public Tile topLeft;
    public Tile topMiddle;
    public Tile topRight;
    #endregion  

    public Tile GetTileByPosition(int x, int y, Vector3Int offset)
    {
        if (offset.x == 0 || offset.y == 0)
            return middleMiddle;

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
