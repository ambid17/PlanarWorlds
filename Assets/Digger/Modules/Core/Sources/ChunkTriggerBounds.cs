using System;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    [Serializable]
    public struct ChunkTriggerBounds
    {
        private static readonly Vector3 MinMin = Vector3.zero;
        private readonly Vector3 maxMax;

        [SerializeField] private bool initialized;
        [SerializeField] private Vector3 min;
        [SerializeField] private Vector3 max;

        public Vector3 Min => min;
        public Vector3 Max => max;
        public bool IsVirgin => !initialized;

        public ChunkTriggerBounds(Vector3 heightmapScale, float sizeOfMesh)
        {
            this.initialized = false;
            this.min = Vector3.zero;
            this.max = Vector3.zero;
            this.maxMax = new Vector3(heightmapScale.x, 1f, heightmapScale.z) * sizeOfMesh;
        }

        public ChunkTriggerBounds(bool virgin, Vector3 min, Vector3 max, Vector3 heightmapScale, float sizeOfMesh)
        {
            this.initialized = !virgin;
            this.min = min;
            this.max = max;
            this.maxMax = heightmapScale * sizeOfMesh;
        }

        public void ExtendIfNeeded(float3 p)
        {
            if (p.x < MinMin.x)
                p.x = MinMin.x;
            if (p.y < MinMin.y)
                p.y = MinMin.y;
            if (p.z < MinMin.z)
                p.z = MinMin.z;

            if (p.x > maxMax.x)
                p.x = maxMax.x;
            if (p.y > maxMax.y)
                p.y = maxMax.y;
            if (p.z > maxMax.z)
                p.z = maxMax.z;

            if (!initialized) {
                initialized = true;
                min.x = p.x;
                min.y = p.y;
                min.z = p.z;
                max.x = p.x + 1;
                max.y = p.y + 1;
                max.z = p.z + 1;
                return;
            }

            if (p.x < min.x)
                min.x = p.x;
            if (p.y < min.y)
                min.y = p.y;
            if (p.z < min.z)
                min.z = p.z;

            if (p.x > max.x)
                max.x = p.x;
            if (p.y > max.y)
                max.y = p.y;
            if (p.z > max.z)
                max.z = p.z;
        }

        public Bounds ToBounds()
        {
            return new Bounds
            {
                min = min,
                max = max
            };
        }
    }
}