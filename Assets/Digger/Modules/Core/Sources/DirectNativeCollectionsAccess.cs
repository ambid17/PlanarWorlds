using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    public static unsafe class DirectNativeCollectionsAccess
    {
        public static void CopyTo<T>(NativeSlice<T> slice, T[] destination) where T : struct
        {
            if (slice.Length != destination.Length) {
                throw new ArgumentException("Source and destination arrays must have the same length");
            }

            try {
                var stride = slice.Stride;
                var bufferPtr = slice.GetUnsafeReadOnlyPtr();
                for (var i = 0; i < destination.Length; ++i) {
                    destination[i] = UnsafeUtility.ReadArrayElementWithStride<T>(bufferPtr, i, stride);
                }
            }
            catch (Exception e) {
                Debug.LogWarning("Failed to make a direct copy. Falling back to CopyTo method. Exception was: " + e);
                slice.CopyTo(destination);
            }
        }
    }
}