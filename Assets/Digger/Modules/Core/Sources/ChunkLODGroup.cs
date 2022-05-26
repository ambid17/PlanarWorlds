using UnityEngine;

namespace Digger.Modules.Core.Sources
{
    public class ChunkLODGroup : MonoBehaviour
    {
        public int LODCount => chunks.Length;
        [SerializeField] private LODGroup lodGroup;
        [SerializeField] private ChunkObject[] chunks;

        internal static ChunkLODGroup Create(Vector3i chunkPosition,
                                             Chunk chunk,
                                             DiggerSystem digger,
                                             Terrain terrain,
                                             Material[] materials,
                                             int layer,
                                             string tag)
        {
            Utils.Profiler.BeginSample("ChunkLODGroup.Create");
            var go = new GameObject(GetName(chunkPosition));
            go.layer = layer;
            go.tag = tag;
            go.transform.parent = chunk.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var chunkLodGroup = go.AddComponent<ChunkLODGroup>();

            if (digger.CreateLODs) {
                var lodGroup = go.AddComponent<LODGroup>();
                chunkLodGroup.chunks = new[]
                {
                    ChunkObject.Create(1, chunkPosition, chunkLodGroup, digger.ColliderLodIndex == 0, digger, terrain,
                                       materials, layer, tag),
                    ChunkObject.Create(2, chunkPosition, chunkLodGroup, digger.ColliderLodIndex == 1, digger, terrain,
                                       materials, layer, tag),
                    ChunkObject.Create(4, chunkPosition, chunkLodGroup, digger.ColliderLodIndex == 2, digger, terrain,
                                       materials, layer, tag)
                };
                var renderers = new Renderer[chunkLodGroup.chunks.Length];
                for (var i = 0; i < renderers.Length; ++i) {
                    renderers[i] = chunkLodGroup.chunks[i].GetComponent<MeshRenderer>();
                }

                var lods = new[]
                {
                    new LOD(digger.ScreenRelativeTransitionHeightLod0, new[] {renderers[0]}),
                    new LOD(digger.ScreenRelativeTransitionHeightLod1, new[] {renderers[1]}),
                    new LOD(0f, new[] {renderers[2]})
                };
                lodGroup.SetLODs(lods);
                chunkLodGroup.lodGroup = lodGroup;
            } else {
                chunkLodGroup.chunks = new[]
                {
                    ChunkObject.Create(1, chunkPosition, chunkLodGroup, true, digger, terrain, materials, layer, tag)
                };
            }

            Utils.Profiler.EndSample();
            return chunkLodGroup;
        }

        public Mesh GetMeshForNavigation()
        {
            return chunks[0].Mesh;
        }

        public void UpdateStaticEditorFlags(bool enableOcclusionCulling)
        {
            for (var i = 0; i < chunks.Length; i++) {
                chunks[i].UpdateStaticEditorFlags(IndexToLod(i), enableOcclusionCulling);
            }
        }

        private static string GetName(Vector3i chunkPosition)
        {
            return $"ChunkLODGroup_{chunkPosition.x}_{chunkPosition.y}_{chunkPosition.z}";
        }

        public bool PostBuild(int lodIndex, Mesh visualMesh, Mesh collisionMesh, ChunkTriggerBounds bounds)
        {
            var hasVisualMesh = chunks[lodIndex].PostBuild(visualMesh, collisionMesh, bounds);
            if (LODCount > 1) {
                lodGroup.RecalculateBounds();
            }

            return hasVisualMesh;
        }

#if UNITY_EDITOR
        public void SaveMeshesAsAssets(DiggerSystem digger)
        {
            for (var i = 0; i < chunks.Length; i++) {
                var chunk = chunks[i];
                chunk.SaveMeshesAsAssets(digger, IndexToLod(i));
            }
        }
#endif

        public static int IndexToLod(int lod)
        {
            return 1 << lod;
        }
    }
}