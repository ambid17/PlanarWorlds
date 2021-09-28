using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtils
{
    public static T[,] Unflatten2DArray<T>(T[] inputArray, int width, int height)
    {
        T[,] output = new T[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                output[i, j] = inputArray[i * width + j];
            }
        }
        return output;
    }

    public static T[,,] Unflatten3DArray<T>(T[] inputArray, int width, int height, int depth)
    {
        T[,,] output = new T[height, width, depth];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    output[i, j, k] = inputArray[i * width + j];
                }
            }
        }
        return output;
    }
}
