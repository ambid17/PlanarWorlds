using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Digger.Modules.Core.Sources;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Digger.Modules.Runtime.Sources
{
    /// <summary>
    /// This class contains some high-level methods to update NavMesh at runtime.
    /// </summary>
    /// <see cref="DiggerNavMeshUsageExample">Example of use</see>
    public class DiggerNavMeshRuntime : MonoBehaviour
    {
        private DiggerSystem[] diggerSystems;

        private NavMeshSurface[] surfaces;
        private List<NavMeshBuildSource>[] initialNavMeshBuildSourcesPerSurface;
        private List<NavMeshBuildSource>[] navMeshBuildSources;
        private Bounds[] initialBoundsPerSurface;
        private Bounds[] boundsPerSurface;

        private void Awake()
        {
            diggerSystems = FindObjectsOfType<DiggerSystem>();
            surfaces = FindObjectsOfType<NavMeshSurface>();
            initialNavMeshBuildSourcesPerSurface = new List<NavMeshBuildSource>[surfaces.Length];
            navMeshBuildSources = new List<NavMeshBuildSource>[surfaces.Length];
            initialBoundsPerSurface = new Bounds[surfaces.Length];
            boundsPerSurface = new Bounds[surfaces.Length];
        }

        /// <summary>
        /// Collects all NavMesh sources in the world. This should be called once, and only once, in the Start method of another MonoBehavior.
        /// </summary>
        public void CollectNavMeshSources()
        {
            var methodCollectSources = typeof(NavMeshSurface).GetMethod("CollectSources", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodCollectSources == null) {
                Debug.LogError("Cannot call method 'CollectSources' on NavMeshSurface. NavMesh support won't work.");
                return;
            }

            var methodCalculateWorldBounds = typeof(NavMeshSurface).GetMethod("CalculateWorldBounds", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodCalculateWorldBounds == null) {
                Debug.LogError("Cannot call method 'CalculateWorldBounds' on NavMeshSurface. NavMesh Digger support won't work.");
                return;
            }

            for (var i = 0; i < surfaces.Length; i++) {
                var surface = surfaces[i];
                var initialNavMeshBuildSources = (List<NavMeshBuildSource>) methodCollectSources.Invoke(surface, null);
                initialNavMeshBuildSources.RemoveAll(x => x.component != null && x.component.gameObject.GetComponent<ChunkObject>() != null);

#if UNITY_EDITOR
                if (surface.useGeometry == NavMeshCollectGeometry.RenderMeshes) {
                    var batchingStaticObjects = initialNavMeshBuildSources
                                                .Where(x => x.component != null && !(x.component is Terrain) &&
                                                            GameObjectUtility.AreStaticEditorFlagsSet(x.component.gameObject, StaticEditorFlags.BatchingStatic))
                                                .Select(x => x.component.gameObject)
                                                .Take(100);
                    foreach (var batchingStaticObject in batchingStaticObjects) {
                        Debug.LogWarning(
                            $"NavMesh sources include object \"<b>{batchingStaticObject.name}</b>\" that has 'Batching Static' enabled. <b>This will cause errors</b> if NavMesh is " +
                            "built or updated at runtime. You should either disable 'Batching Static' or change NavMeshSurface setting 'Use Geometry' to" +
                            "'Physics Colliders'.");
                    }
                }
#endif

                initialNavMeshBuildSourcesPerSurface[i] = initialNavMeshBuildSources;
                initialBoundsPerSurface[i] = (Bounds) methodCalculateWorldBounds.Invoke(surface, new object[] {initialNavMeshBuildSources});
                navMeshBuildSources[i] = new List<NavMeshBuildSource>(initialNavMeshBuildSources.Capacity + 100);
            }
        }

        /// <summary>
        /// Incrementally and asynchronously updates the NavMesh. Call this when you want the NavMesh to be refreshed, but avoid calling this every frame
        /// to limit the impact on performance.
        /// </summary>
        public void UpdateNavMeshAsync()
        {
            RefreshNavMeshSources();
            StartCoroutine(UpdateNavMeshCoroutine(null));
        }

        /// <summary>
        /// Incrementally and asynchronously updates the NavMesh. Call this when you want the NavMesh to be refreshed, but avoid calling this every frame
        /// to limit the impact on performance.
        /// </summary>
        /// <param name="callback">Callback method to be invoked once NavMesh has been updated</param>
        public void UpdateNavMeshAsync(Action callback)
        {
            RefreshNavMeshSources();
            StartCoroutine(UpdateNavMeshCoroutine(callback));
        }

        private void RefreshNavMeshSources()
        {
            for (var i = 0; i < surfaces.Length; i++) {
                var nmsrc = navMeshBuildSources[i];
                nmsrc.Clear();
                nmsrc.AddRange(initialNavMeshBuildSourcesPerSurface[i]);
                boundsPerSurface[i] = initialBoundsPerSurface[i];
                foreach (var digger in diggerSystems) {
                    digger.AddNavMeshSources(nmsrc);
                    var b = digger.Bounds;
                    boundsPerSurface[i] = ExpandBounds(boundsPerSurface[i], b.min, b.max);
                }
            }
        }

        private IEnumerator UpdateNavMeshCoroutine(Action callback)
        {
            for (var i = 0; i < surfaces.Length; i++) {
                var surface = surfaces[i];
                var nmsrc = navMeshBuildSources[i];
                if (nmsrc.Count == 0) {
                    surface.RemoveData();
                    continue;
                }

                if (!surface.navMeshData)
                    surface.navMeshData = InitializeBakeData(surface);

                var op = NavMeshBuilder.UpdateNavMeshDataAsync(surface.navMeshData, surface.GetBuildSettings(), nmsrc, boundsPerSurface[i]);
                yield return op;
                surface.RemoveData();
                surface.AddData();
            }

            callback?.Invoke();
        }

        private static Bounds ExpandBounds(Bounds bounds, Vector3 min, Vector3 max)
        {
            if (bounds.min.x < min.x) {
                min.x = bounds.min.x;
            }

            if (bounds.min.y < min.y) {
                min.y = bounds.min.y;
            }

            if (bounds.min.z < min.z) {
                min.z = bounds.min.z;
            }

            if (bounds.max.x > max.x) {
                max.x = bounds.max.x;
            }

            if (bounds.max.y > max.y) {
                max.y = bounds.max.y;
            }

            if (bounds.max.z > max.z) {
                max.z = bounds.max.z;
            }

            bounds.SetMinMax(min, max);
            return bounds;
        }

        private static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();
            return NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds
                                                   , surface.transform.position, surface.transform.rotation);
        }
    }
}