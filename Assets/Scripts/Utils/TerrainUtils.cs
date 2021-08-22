using UnityEngine;

public static class TerrainUtils
{
    public static Vector3Int GetDragBoxPaintOffset(Vector3Int startPosition, Vector3Int endPosition)
    {
        // Set a default offset, which will be overridden if either x or y are negative
        Vector3Int offset = endPosition - startPosition;

        // Both axes are negative, so swap the start and end positions 
        if (offset.x < 0 && offset.y < 0)
        {
            Vector3Int temp = startPosition;
            startPosition = endPosition;
            endPosition = temp;
        }
        // y is negative, so swap the y only
        else if (offset.x >= 0 && offset.y < 0)
        {
            Vector3Int newStartPosition = new Vector3Int(startPosition.x, endPosition.y, 0);
            Vector3Int newEndPosition = new Vector3Int(endPosition.x, startPosition.y, 0);

            startPosition = newStartPosition;
            endPosition = newEndPosition;
        }
        // x is negative, so swap the x only 
        else if (offset.x < 0 && offset.y >= 0)
        {
            Vector3Int newStartPosition = new Vector3Int(endPosition.x, startPosition.y, 0);
            Vector3Int newEndPosition = new Vector3Int(startPosition.x, endPosition.y, 0);

            startPosition = newStartPosition;
            endPosition = newEndPosition;
        }

        offset = endPosition - startPosition;
        return offset;
    }
}
