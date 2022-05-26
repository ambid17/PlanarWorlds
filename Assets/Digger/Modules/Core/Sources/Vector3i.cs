using System;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    [Serializable]
    public struct Vector3i
    {
        public int x;
        public int y;
        public int z;

        public static readonly Vector3i zero = new Vector3i(0, 0, 0);
        public static readonly Vector3i one = new Vector3i(1, 1, 1);
        public static readonly Vector3i two = new Vector3i(2, 2, 2);

        public static readonly Vector3i forward = new Vector3i(0, 0, 1);
        public static readonly Vector3i back = new Vector3i(0, 0, -1);
        public static readonly Vector3i up = new Vector3i(0, 1, 0);
        public static readonly Vector3i down = new Vector3i(0, -1, 0);
        public static readonly Vector3i left = new Vector3i(-1, 0, 0);
        public static readonly Vector3i right = new Vector3i(1, 0, 0);

        public static readonly Vector3i forward_right = new Vector3i(1, 0, 1);
        public static readonly Vector3i forward_left = new Vector3i(-1, 0, 1);
        public static readonly Vector3i forward_up = new Vector3i(0, 1, 1);
        public static readonly Vector3i forward_down = new Vector3i(0, -1, 1);
        public static readonly Vector3i back_right = new Vector3i(1, 0, -1);
        public static readonly Vector3i back_left = new Vector3i(-1, 0, -1);
        public static readonly Vector3i back_up = new Vector3i(0, 1, -1);
        public static readonly Vector3i back_down = new Vector3i(0, -1, -1);
        public static readonly Vector3i up_right = new Vector3i(1, 1, 0);
        public static readonly Vector3i up_left = new Vector3i(-1, 1, 0);
        public static readonly Vector3i down_right = new Vector3i(1, -1, 0);
        public static readonly Vector3i down_left = new Vector3i(-1, -1, 0);

        public static readonly Vector3i forward_right_up = new Vector3i(1, 1, 1);
        public static readonly Vector3i forward_right_down = new Vector3i(1, -1, 1);
        public static readonly Vector3i forward_left_up = new Vector3i(-1, 1, 1);
        public static readonly Vector3i forward_left_down = new Vector3i(-1, -1, 1);
        public static readonly Vector3i back_right_up = new Vector3i(1, 1, -1);
        public static readonly Vector3i back_right_down = new Vector3i(1, -1, -1);
        public static readonly Vector3i back_left_up = new Vector3i(-1, 1, -1);
        public static readonly Vector3i back_left_down = new Vector3i(-1, -1, -1);


        public static readonly Vector3i[] directions =
        {
            left, right,
            back, forward,
            down, up
        };

        public static readonly Vector3i[] allDirections =
        {
            left,
            right,
            back,
            forward,
            down,
            up,
            forward_right,
            forward_left,
            forward_up,
            forward_down,
            back_right,
            back_left,
            back_up,
            back_down,
            up_right,
            up_left,
            down_right,
            down_left,
            forward_right_up,
            forward_right_down,
            forward_left_up,
            forward_left_down,
            back_right_up,
            back_right_down,
            back_left_up,
            back_left_down
        };

        public static readonly Vector3i[] allDirectionsOrdered =
        {
            forward_right_up,
            forward_right_down,
            forward_left_up,
            forward_left_down,
            back_right_up,
            back_right_down,
            back_left_up,
            back_left_down,
            forward_right,
            forward_left,
            forward_up,
            forward_down,
            back_right,
            back_left,
            back_up,
            back_down,
            up_right,
            up_left,
            down_right,
            down_left,
            left,
            right,
            back,
            forward,
            down,
            up
        };

        public static readonly Vector3i[] planeDirections =
        {
            left,
            right,
            back,
            forward,
            forward_right,
            forward_left,
            back_right,
            back_left
        };

        public static readonly Vector3i[] planeDirectionsStraight =
        {
            left,
            right,
            back,
            forward
        };

        public static readonly Vector3i[] planeDirectionsStraightDown =
        {
            down_left,
            down_right,
            back_down,
            forward_down
        };

        public static int IndexOfDirection(Vector3i direction)
        {
            return Array.IndexOf(allDirections, direction);
        }

        public static bool AreNeighbours(Vector3i a, Vector3i b)
        {
            return (a.x == b.x || a.x == b.x + 1 || a.x == b.x - 1) &&
                   (a.y == b.y || a.y == b.y + 1 || a.y == b.y - 1) &&
                   (a.z == b.z || a.z == b.z + 1 || a.z == b.z - 1);
        }

        public static Vector3i GetNeighbourDirection(Vector3i a, Vector3i b)
        {
            return b - a;
        }

        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i(float x, float y, float z)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = (int) z;
        }

        public Vector3i(double x, double y, double z)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = (int) z;
        }

        public Vector3i(Vector3 v)
        {
            x = (int) v.x;
            y = (int) v.y;
            z = (int) v.z;
        }

        public static int DistanceSquared(Vector3i a, Vector3i b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public int DistanceSquared(Vector3i v)
        {
            return DistanceSquared(this, v);
        }

        public static int FlatDistanceSquared(Vector3i a, Vector3i b)
        {
            var dx = b.x - a.x;
            var dz = b.z - a.z;
            return dx * dx + dz * dz;
        }

        public int FlatDistanceSquared(Vector3i v)
        {
            return FlatDistanceSquared(this, v);
        }

        public static float Distance(Vector3i a, Vector3i b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var dz = b.z - a.z;
            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public float Distance(Vector3i v)
        {
            return Distance(this, v);
        }

        public bool IsInCubeArea(Vector3i cubeCenter, int cubeRadius)
        {
            var d = x - cubeCenter.x;
            if (-cubeRadius <= d && d <= cubeRadius) {
                d = y - cubeCenter.y;
                if (-cubeRadius <= d && d <= cubeRadius) {
                    d = z - cubeCenter.z;
                    if (-cubeRadius <= d && d <= cubeRadius) {
                        return true;
                    }
                }
            }

            return false;
        }

        public float MagnitudeSquared {
            get { return x * x + y * y + z * z; }
        }

        public override int GetHashCode()
        {
            return x ^ (y << 2) ^ (z >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3i))
                return false;
            var vector = (Vector3i) other;
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public bool Equals(Vector3i vector)
        {
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override string ToString()
        {
            return "Vector3i(" + x + " " + y + " " + z + ")";
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.x == b.x &&
                   a.y == b.y &&
                   a.z == b.z;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return a.x != b.x ||
                   a.y != b.y ||
                   a.z != b.z;
        }

        public int this[int i] {
            get {
                if (i == 0)
                    return x;
                if (i == 1)
                    return y;
                if (i == 2)
                    return z;

                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
            }
            set {
                if (i == 0)
                    x = value;
                if (i == 1)
                    y = value;
                if (i == 2)
                    z = value;

                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
            }
        }

        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3i operator -(Vector3i a)
        {
            return new Vector3i(-a.x, -a.y, -a.z);
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3i operator *(Vector3i a, int b)
        {
            return new Vector3i(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3i operator *(int b, Vector3i a)
        {
            return new Vector3i(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3i operator /(Vector3i a, int b)
        {
            return new Vector3i(a.x / b, a.y / b, a.z / b);
        }

        public static Vector3 operator *(Vector3i a, float b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator *(float b, Vector3i a)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static bool operator <(Vector3i a, Vector3i b)
        {
            return a.x < b.x && a.y < b.y && a.z < b.z;
        }

        public static bool operator >(Vector3i a, Vector3i b)
        {
            return a.x > b.x && a.y > b.y && a.z > b.z;
        }

        public static bool operator <=(Vector3i a, Vector3i b)
        {
            return a.x <= b.x && a.y <= b.y && a.z <= b.z;
        }

        public static bool operator >=(Vector3i a, Vector3i b)
        {
            return a.x >= b.x && a.y >= b.y && a.z >= b.z;
        }

        public static implicit operator Vector3(Vector3i v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}