using UnityEngine;

public static class TerrainUtils
{
    // This method Gets the offset, and also swaps the start/end position, so that the start position is in the bottom left
    public static Vector3Int GetDragPaintOffset(ref Vector3Int startPosition, ref Vector3Int endPosition)
    {
        // Set a default offset, which will be overridden if either x or y are negative
        Vector3Int offset = endPosition - startPosition;

        // x and y are negative, so swap the start and end positions 
        if (offset.x < 0 && offset.y < 0)
        {
            Vector3Int temp = startPosition;
            startPosition = endPosition;
            endPosition = temp;
        }
        // y is negative, so swap the y's only
        else if (offset.x >= 0 && offset.y < 0)
        {
            Vector3Int newStartPosition = new Vector3Int(startPosition.x, endPosition.y, 0);
            Vector3Int newEndPosition = new Vector3Int(endPosition.x, startPosition.y, 0);

            startPosition = newStartPosition;
            endPosition = newEndPosition;
        }
        // x is negative, so swap the x's only 
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
