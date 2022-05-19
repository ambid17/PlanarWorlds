using System;

namespace Digger.Modules.Core.Sources
{
    [Serializable]
    public struct Vector2i
    {
        public int x;
        public int y;

        public static readonly Vector2i zero = new Vector2i(0, 0);
        public static readonly Vector2i one = new Vector2i(1, 1);


        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int DistanceSquared(Vector2i a, Vector2i b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            return dx * dx + dy * dy;
        }

        public int DistanceSquared(Vector2i v)
        {
            return DistanceSquared(this, v);
        }

        public override int GetHashCode()
        {
            return x ^ (y << 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2i))
                return false;
            var vector = (Vector2i) other;
            return x == vector.x &&
                   y == vector.y;
        }

        public bool Equals(Vector2i vector)
        {
            return x == vector.x &&
                   y == vector.y;
        }

        public override string ToString()
        {
            return "Vector2i(" + x + " " + y + ")";
        }

        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.x == b.x &&
                   a.y == b.y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return a.x != b.x ||
                   a.y != b.y;
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x - b.x, a.y - b.y);
        }

        public static Vector2i operator -(Vector2i a)
        {
            return new Vector2i(-a.x, -a.y);
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x + b.x, a.y + b.y);
        }

        public static Vector2i operator *(Vector2i a, int b)
        {
            return new Vector2i(a.x * b, a.y * b);
        }

        public static Vector2i operator *(int b, Vector2i a)
        {
            return new Vector2i(a.x * b, a.y * b);
        }

        public static Vector2i operator /(Vector2i a, int b)
        {
            return new Vector2i(a.x / b, a.y / b);
        }
    }
}