using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    public class ListPool
    {
        private static readonly Vector4[] Vector4Array = new Vector4[65536];
        private static readonly List<Vector4> Vector4List = new List<Vector4>(65536);
        private static readonly List<Vector2> Vector2List1 = new List<Vector2>(65536);
        private static readonly List<Vector2> Vector2List2 = new List<Vector2>(65536);

        public static List<Vector4> ToVector4List(NativeArray<Vector4> src, int length)
        {
            NativeArray<Vector4>.Copy(src, Vector4Array, length);
            Vector4List.Clear();
            for (var i = 0; i < length; ++i) {
                Vector4List.Add(Vector4Array[i]);
            }

            return Vector4List;
        }

        public static void ToVector2Lists(NativeArray<Vector4> src, int length, out List<Vector2> vector2List1, out List<Vector2> vector2List2)
        {
            NativeArray<Vector4>.Copy(src, Vector4Array, length);
            Vector2List1.Clear();
            Vector2List2.Clear();
            for (var i = 0; i < length; ++i) {
                var vec = Vector4Array[i];
                Vector2List1.Add(new Vector2(vec.x, vec.y));
                Vector2List2.Add(new Vector2(vec.z, vec.w));
            }

            vector2List1 = Vector2List1;
            vector2List2 = Vector2List2;
        }
    }
}