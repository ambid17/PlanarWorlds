using System.Diagnostics.CodeAnalysis;

namespace Digger.Modules.Core.Sources
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct LoadType
    {
        public static LoadType Minimal = new LoadType()
        {
            loadVoxels = false,
            rebuildMeshes = false,
            syncVoxelsWithTerrain = false
        };

        public static LoadType Minimal_and_LoadVoxels = new LoadType()
        {
            loadVoxels = true,
            rebuildMeshes = false,
            syncVoxelsWithTerrain = false
        };

        public static LoadType Minimal_and_LoadVoxels_and_RebuildMeshes = new LoadType()
        {
            loadVoxels = true,
            rebuildMeshes = true,
            syncVoxelsWithTerrain = false
        };

        public static LoadType Minimal_and_LoadVoxels_and_SyncVoxelsWithTerrain_and_RebuildMeshes = new LoadType()
        {
            loadVoxels = true,
            rebuildMeshes = true,
            syncVoxelsWithTerrain = true
        };

        private bool loadVoxels;
        private bool rebuildMeshes;
        private bool syncVoxelsWithTerrain;

        public bool LoadVoxels => loadVoxels;
        public bool RebuildMeshes => rebuildMeshes;
        public bool SyncVoxelsWithTerrain => syncVoxelsWithTerrain;

        public override string ToString()
        {
            return $"LoadType(loadVoxels={loadVoxels}, rebuildMeshes={rebuildMeshes}, syncVoxelsWithTerrain={syncVoxelsWithTerrain})";
        }

        public bool Equals(LoadType other)
        {
            return loadVoxels == other.loadVoxels && rebuildMeshes == other.rebuildMeshes && syncVoxelsWithTerrain == other.syncVoxelsWithTerrain;
        }

        public override bool Equals(object obj)
        {
            return obj is LoadType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = loadVoxels.GetHashCode();
                hashCode = (hashCode * 397) ^ rebuildMeshes.GetHashCode();
                hashCode = (hashCode * 397) ^ syncVoxelsWithTerrain.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(LoadType a, LoadType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LoadType a, LoadType b)
        {
            return !(a == b);
        }
    }
}