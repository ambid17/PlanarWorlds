using System.Collections.Generic;
using System.IO;
using Digger.Modules.Core.Sources;

namespace Digger.Modules.Runtime.Sources
{
    public static class DiggerSystemExtensions
    {
        public static void PersistAtRuntime(this DiggerSystem digger)
        {
            if (digger.DisablePersistence)
                return;

            if (!Directory.Exists(digger.PersistentRuntimePathData)) {
                Directory.CreateDirectory(digger.PersistentRuntimePathData);
            }

            foreach (var chunkToPersist in digger.ChunksToPersist) {
                chunkToPersist.Persist();
            }

            digger.ChunksToPersist.Clear();

            digger.Cutter.SaveTo(digger.TerrainHolesRuntimePath);
        }

        public static void DeleteDataPersistedAtRuntime(this DiggerSystem digger)
        {
            if (Directory.Exists(digger.PersistentRuntimePathData)) {
                Directory.Delete(digger.PersistentRuntimePathData, true);
            }
        }

        public static void OnPreprocessBuild(this DiggerSystem digger, bool includeVoxelData)
        {
#if UNITY_EDITOR
            if (Directory.Exists(digger.StreamingAssetsPathData))
                Directory.Delete(digger.StreamingAssetsPathData, true);

            digger.ChunksInStreamingAssets = null;

            if (includeVoxelData) {
                Directory.CreateDirectory(digger.StreamingAssetsPathData);

                var chunksInStreamingAssetsList = new List<Vector3i>();
                foreach (var p in Directory.GetFiles(digger.InternalPathData, $"*.{DiggerSystem.VoxelFileExtension}",
                             SearchOption.TopDirectoryOnly)) {
                    var fi = new FileInfo(p);
                    fi.CopyTo(Path.Combine(digger.StreamingAssetsPathData, fi.Name));
                    chunksInStreamingAssetsList.Add(Chunk.GetPositionFromName(fi.Name));
                }

                foreach (var p in Directory.GetFiles(digger.InternalPathData, $"*.{DiggerSystem.VoxelMetadataFileExtension}",
                             SearchOption.TopDirectoryOnly)) {
                    var fi = new FileInfo(p);
                    fi.CopyTo(Path.Combine(digger.StreamingAssetsPathData, fi.Name));
                }

                digger.ChunksInStreamingAssets = chunksInStreamingAssetsList.ToArray();
            }
#endif
        }
    }
}