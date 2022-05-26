using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    public static class Utils
    {
        public static class Profiler
        {
            [Conditional("DIGGER_PROFILING")]
            public static void BeginSample(string name)
            {
                UnityEngine.Profiling.Profiler.BeginSample("[Dig] " + name);
            }

            [Conditional("DIGGER_PROFILING")]
            public static void EndSample()
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static class D
        {
            [Conditional("DIGGER_DEBUGGING")]
            public static void Log(string message)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        public static float3 VoxelToUnityPosition(int3 voxelPosition, float3 heightmapScale)
        {
            return voxelPosition * heightmapScale;
        }
        
        public static int3 UnityToVoxelPosition(float3 position, float3 heightmapScale)
        {
            return new int3(position / heightmapScale);
        }


        public static int3 IndexToXYZ(int index, int sizeVox, int sizeVox2)
        {
            var xi = index / sizeVox2;
            var yi = (index - xi * sizeVox2) / sizeVox;
            var zi = index - xi * sizeVox2 - yi * sizeVox;
            return new int3(xi, yi, zi);
        }

        public static int XYZToHeightIndex(int3 pi, int sizeVox)
        {
            return (pi.x + 1) * (sizeVox + 2) + pi.z + 1;
        }
        
        public static int XZToNormalIndex(int px, int pz, int sizeVox)
        {
            return (px + 1) * (sizeVox + 2) + pz + 1;
        }

        public static int XZToHoleIndex(int px, int pz, int sizeVox)
        {
            return px * sizeVox + pz;
        }

        public static bool IsOnSurface(int3 pi, float voxelHeight, float voxelAltitude, int sizeVox, NativeArray<float> heights)
        {
            var minHeight = float.PositiveInfinity;
            var maxHeight = float.NegativeInfinity;
            for (var x = pi.x - 1; x <= pi.x + 1; ++x) {
                for (var z = pi.z - 1; z <= pi.z + 1; ++z) {
                    if (x < -1 || x >= sizeVox + 1 || z < -1 || z >= sizeVox + 1)
                        continue;

                    var terrainHeight = heights[XYZToHeightIndex(new int3(x, 0, z), sizeVox)];
                    if (math.abs(voxelAltitude - terrainHeight) <= voxelHeight)
                        return true;

                    minHeight = math.min(minHeight, terrainHeight);
                    maxHeight = math.max(maxHeight, terrainHeight);
                }
            }

            return voxelAltitude >= minHeight - voxelHeight && voxelAltitude <= maxHeight + voxelHeight;
        }

        public static bool IsOnHole(int3 pi, int sizeVox, NativeArray<int> holes)
        {
            return holes[XYZToHoleIndex(pi + new int3(-1,0,-1), sizeVox)] != 0 ||
                   holes[XYZToHoleIndex(pi + new int3(-1,0,+0), sizeVox)] != 0 ||
                   holes[XYZToHoleIndex(pi + new int3(+0,0,-1), sizeVox)] != 0 ||
                   holes[XYZToHoleIndex(pi + new int3(+0,0,+0), sizeVox)] != 0;
        }

        private static int XYZToHoleIndex(int3 pi, int sizeVox)
        {
            return math.clamp(pi.x, 0, sizeVox-1) * sizeVox + math.clamp(pi.z, 0, sizeVox-1);
        }

        public static Voxel AdjustAlteration(Voxel voxel, int3 pi, float voxelHeight, float voxelAltitude, float terrainHeightValue, int sizeVox, NativeArray<float> heights)
        {
            var alt = voxel.Alteration;
            if (alt == Voxel.Unaltered || alt == Voxel.Hole)
                return voxel;

            if (voxel.IsAlteredFarSurface && IsOnSurface(pi, voxelHeight,voxelAltitude, sizeVox, heights)) {
                voxel.Alteration = Voxel.NearAboveSurface;
            }

            if (voxel.Value > terrainHeightValue) {
                switch (voxel.Alteration) {
                    case Voxel.FarAboveSurface:
                        voxel.Alteration = Voxel.FarBelowSurface;
                        break;
                    case Voxel.NearAboveSurface:
                        voxel.Alteration = Voxel.NearBelowSurface;
                        break;
                }
            } else {
                switch (voxel.Alteration) {
                    case Voxel.FarBelowSurface:
                        voxel.Alteration = Voxel.FarAboveSurface;
                        break;
                    case Voxel.NearBelowSurface:
                        voxel.Alteration = Voxel.NearAboveSurface;
                        break;
                }
            }

            return voxel;
        }

        public static bool Approximately(Color a, Color b)
        {
            return math.abs(a.r - b.r) < 1e-5f &&
                   math.abs(a.g - b.g) < 1e-5f &&
                   math.abs(a.b - b.b) < 1e-5f &&
                   math.abs(a.a - b.a) < 1e-5f;
        }

        public static bool Approximately(Vector3 a, Vector3 b)
        {
            return math.abs(a.x - b.x) < 1e-5f &&
                   math.abs(a.y - b.y) < 1e-5f &&
                   math.abs(a.z - b.z) < 1e-5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float3 a, float3 b)
        {
            var d = math.abs(a - b);
            return d.x < 1e-5f &&
                   d.y < 1e-5f &&
                   d.z < 1e-5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            return math.abs(a - b) < 1e-5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreColinear(float3 a, float3 b, float3 c)
        {
            return Approximately(math.cross(b - a, c - a), float3.zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BilinearInterpolate(float f00, float f01, float f10, float f11, float x, float y)
        {
            var oneMinX = 1.0f - x;
            var oneMinY = 1.0f - y;
            return oneMinX * (oneMinY * f00 + y * f01) +
                   x * (oneMinY * f10 + y * f11);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 BilinearInterpolate(float3 f00, float3 f01, float3 f10, float3 f11, float x, float y)
        {
            var oneMinX = 1.0f - x;
            var oneMinY = 1.0f - y;
            return oneMinX * (oneMinY * f00 + y * f01) +
                   x * (oneMinY * f10 + y * f11);
        }

        public static Vector3 TriangleInterpolate(int2 a, int2 b, int2 c, int2 p)
        {
            var di = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
            if (di == 0)
                return -Vector3.one;

            var d = (double)di;
            var wa = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / d;
            var wb = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / d;
            var wc = 1 - wa - wb;
            return new Vector3((float)wa, (float)wb, (float)wc);
        }

        public static int2 Min(int2 a, int2 b, int2 c)
        {
            return math.min(a, math.min(b, c));
        }

        public static int2 Max(int2 a, int2 b, int2 c)
        {
            return math.max(a, math.max(b, c));
        }

        public static T[] ToArray<T>(NativeArray<T> src, int length) where T : struct
        {
            var dst = new T[length];
            NativeArray<T>.Copy(src, dst, length);
            return dst;
        }

        public static byte[] GetBytes(string path)
        {
#if ((!UNITY_ANDROID && !UNITY_WEBGL) || UNITY_EDITOR)
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
#else
            var uri = path;
            if (!uri.StartsWith("jar:") && !uri.StartsWith("file:")) {
                if (File.Exists(uri)) {
                    uri = Path.GetFullPath(uri);
                }

                uri = $"file://{uri}";
            }

            using (var webRequest = UnityEngine.Networking.UnityWebRequest.Get(uri)) {
                var op = webRequest.SendWebRequest();
                while (!op.isDone) {
                }

                if (!webRequest.isNetworkError) {
                    var data = webRequest.downloadHandler.data;
                    return data != null && data.Length > 0 ? data : null;
                }

                UnityEngine.Debug.LogError($"Failed to load URI '{uri}' with error: {webRequest.error}");

                return File.Exists(uri) ? File.ReadAllBytes(path) : null;
            }
#endif
        }
    }
}