using System.Collections.Generic;

namespace Digger.Modules.Core.Sources
{
    public sealed class Vector2iComparer : IEqualityComparer<Vector2i>
    {
        public int GetHashCode(Vector2i v)
        {
            return v.x ^ (v.y << 2);
        }

        public bool Equals(Vector2i v1, Vector2i v2)
        {
            return v1.x == v2.x &&
                   v1.y == v2.y;
        }
    }
}