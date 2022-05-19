using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.Core.Sources.TerrainInterface
{
    [DefaultExecutionOrder(-11)] // Execute right before DiggerMasterRuntime to avoid issues when editing asynchronously
    public class TerrainCutter : MonoBehaviour
    {
        private const int LargeFileBufferSize = 32768;

        [SerializeField] private DiggerSystem digger;

        private bool mustApply;
        private bool mustPersist;
        private TerrainData terrainData;
        private bool needsSync;
        private int holesResolution;
        private int voxResolution;
        private int sizeOfChunk;
        private int sizeOfChunkHoles;

        private class HolesEntry
        {
            public bool[,] TerrainHoles;
            public int[] DiggerHoles;
        }

        private Dictionary<Vector2i, HolesEntry> holesPerChunk;

        private struct DelayedHoles
        {
            public int XBase;
            public int YBase;
            public bool[,] Holes;
        }

        private readonly Queue<DelayedHoles> delayedHolesToApply = new Queue<DelayedHoles>(250);

        private class UndoRecord
        {
            public bool[,] UndoHoles;
            public List<int[,]> UndoDetails;
            public TreeInstance[] UndoTrees;
        }

        private readonly Dictionary<long, UndoRecord> undoRecords = new Dictionary<long, UndoRecord>();


        public void Apply(bool persist)
        {
            if (Application.isEditor && !Application.isPlaying) {
                ApplyInternal(persist);
            } else {
                mustApply = true;
                mustPersist = persist;
            }
        }

        private void Update()
        {
            if (mustApply) {
                ApplyInternal(mustPersist);
                mustApply = false;
                mustPersist = false;
            }
        }


        public static TerrainCutter CreateInstance(DiggerSystem digger)
        {
            var cutter = digger.gameObject.AddComponent<TerrainCutter>();
            cutter.digger = digger;
            cutter.Refresh();
            return cutter;
        }

        public void Refresh()
        {
            terrainData = digger.Terrain.terrainData;
            terrainData.enableHolesTextureCompression = false;
            holesResolution = terrainData.holesResolution;
            voxResolution = digger.ResolutionMult;
            sizeOfChunk = digger.SizeVox;
            sizeOfChunkHoles = sizeOfChunk / voxResolution;
            holesPerChunk = new Dictionary<Vector2i, HolesEntry>(100, new Vector2iComparer());
            // force initialisation of holes texture
            terrainData.SetHoles(0, 0, terrainData.GetHoles(0, 0, 1, 1));
        }

        private HolesEntry GetCreateHoles(Vector2i chunkPosition, Vector3i voxelPosition)
        {
            if (holesPerChunk.TryGetValue(chunkPosition, out var holes)) {
                return holes;
            }

            var mx = voxelPosition.x / voxResolution;
            var mz = voxelPosition.z / voxResolution;

            var diggerHoles = terrainData.GetHoles(
                mx,
                mz,
                math.min(sizeOfChunkHoles, holesResolution - mx),
                math.min(sizeOfChunkHoles, holesResolution - mz));

            var diggerHolesFlat = new int[sizeOfChunk * sizeOfChunk];
            for (var x = 0; x < sizeOfChunk; ++x) {
                for (var z = 0; z < sizeOfChunk; ++z) {
                    var safeX = math.min(x / voxResolution, diggerHoles.GetLength(0) - 1);
                    var safeZ = math.min(z / voxResolution, diggerHoles.GetLength(1) - 1);
                    diggerHolesFlat[z * sizeOfChunk + x] = diggerHoles[safeX, safeZ] ? 0 : 1;
                }
            }

            holes = new HolesEntry
            {
                TerrainHoles = diggerHoles,
                DiggerHoles = diggerHolesFlat
            };
            holesPerChunk.Add(chunkPosition, holes);
            return holes;
        }

        public int[] GetHoles(Vector3i chunkPosition, Vector3i voxelPosition)
        {
            var entry = GetCreateHoles(new Vector2i(chunkPosition.x, chunkPosition.z), voxelPosition);
            return entry.DiggerHoles;
        }

        public void Cut(NativeArray<int> chunkHoles, Vector3i voxelPosition, Vector3i chunkPosition)
        {
            var holes = GetCreateHoles(new Vector2i(chunkPosition.x, chunkPosition.z), voxelPosition);

            for (var z = 0; z < holes.TerrainHoles.GetLength(1); ++z) {
                for (var x = 0; x < holes.TerrainHoles.GetLength(0); ++x) {
                    var noHole = holes.TerrainHoles[x, z];
                    for (var rz = 0; rz < voxResolution; ++rz) {
                        for (var rx = 0; rx < voxResolution; ++rx) {
                            var idx = (z * voxResolution + rz) * sizeOfChunk + (x * voxResolution + rx);
                            noHole = noHole && chunkHoles[idx] == 0;
                        }
                    }

                    holes.TerrainHoles[x, z] = noHole;
                }
            }

            for (var x = 0; x < sizeOfChunk; ++x) {
                for (var z = 0; z < sizeOfChunk; ++z) {
                    var safeX = math.min(x / voxResolution, holes.TerrainHoles.GetLength(0) - 1);
                    var safeZ = math.min(z / voxResolution, holes.TerrainHoles.GetLength(1) - 1);
                    holes.DiggerHoles[z * sizeOfChunk + x] = holes.TerrainHoles[safeX, safeZ] ? 0 : 1;
                }
            }

            delayedHolesToApply.Enqueue(new DelayedHoles
            {
                XBase = voxelPosition.x / voxResolution,
                YBase = voxelPosition.z / voxResolution,
                Holes = holes.TerrainHoles
            });
            Utils.Profiler.BeginSample("[Dig] Cutter20193.Cut");
            needsSync = true;
            Utils.Profiler.EndSample();
        }

        public void UnCut(NativeArray<int> chunkHoles, Vector3i voxelPosition, Vector3i chunkPosition)
        {
            var holes = GetCreateHoles(new Vector2i(chunkPosition.x, chunkPosition.z), voxelPosition);

            for (var z = 0; z < holes.TerrainHoles.GetLength(1); ++z) {
                for (var x = 0; x < holes.TerrainHoles.GetLength(0); ++x) {
                    var noHole = holes.TerrainHoles[x, z];
                    for (var rz = 0; rz < voxResolution; ++rz) {
                        for (var rx = 0; rx < voxResolution; ++rx) {
                            var idx = (z * voxResolution + rz) * sizeOfChunk + (x * voxResolution + rx);
                            noHole = noHole || chunkHoles[idx] == 0;
                        }
                    }

                    for (var rz = 0; rz < voxResolution; ++rz) {
                        for (var rx = 0; rx < voxResolution; ++rx) {
                            var idx = (z * voxResolution + rz) * sizeOfChunk + (x * voxResolution + rx);
                            noHole = noHole && chunkHoles[idx] == 0;
                        }
                    }

                    holes.TerrainHoles[x, z] = holes.TerrainHoles[x, z] || noHole;
                }
            }

            for (var x = 0; x < sizeOfChunk; ++x) {
                for (var z = 0; z < sizeOfChunk; ++z) {
                    var safeX = math.min(x / voxResolution, holes.TerrainHoles.GetLength(0) - 1);
                    var safeZ = math.min(z / voxResolution, holes.TerrainHoles.GetLength(1) - 1);
                    holes.DiggerHoles[z * sizeOfChunk + x] = holes.TerrainHoles[safeX, safeZ] ? 0 : 1;
                }
            }

            Utils.Profiler.BeginSample("[Dig] Cutter20193.UnCut");
            terrainData.SetHolesDelayLOD(voxelPosition.x / voxResolution, voxelPosition.z / voxResolution, holes.TerrainHoles);
            needsSync = true;
            Utils.Profiler.EndSample();
        }

        private void ApplyInternal(bool persist)
        {
            Utils.Profiler.BeginSample("[Dig] Cutter20193.Apply");
            if (needsSync) {
                needsSync = false;
                while (delayedHolesToApply.Count > 0) {
                    var toApply = delayedHolesToApply.Dequeue();
                    terrainData.SetHolesDelayLOD(toApply.XBase, toApply.YBase, toApply.Holes);
                }

                terrainData.SyncTexture(TerrainData.HolesTextureName);
            }

            Utils.Profiler.EndSample();
        }


#if UNITY_EDITOR
        private UndoRecord undoPlayMode;

        public void OnEnterPlayMode()
        {
            Utils.Profiler.BeginSample("[Cutter] OnEnterPlayMode");
            undoPlayMode = CreateUndoRecord();
            Utils.Profiler.EndSample();
        }

        public void OnExitPlayMode()
        {
            if (undoPlayMode == null) {
                Debug.LogError("undoPlayMode is null");
                return;
            }

            Utils.Profiler.BeginSample("[Cutter] OnExitPlayMode");
            RestoreUndoRecord(undoPlayMode);
            Utils.Profiler.EndSample();
        }

        public void RecordUndo(long version)
        {
            var undo = CreateUndoRecord();
            undoRecords.Remove(version);
            undoRecords.Add(version, undo);
        }

        public void PerformUndo(long version)
        {
            if (undoRecords.TryGetValue(version, out var undo)) {
                RestoreUndoRecord(undo);
            }
        }

        public void DeleteUndoRecord(long version)
        {
            undoRecords.Remove(version);
        }

        private UndoRecord CreateUndoRecord()
        {
            var undo = new UndoRecord();
            var tData = digger.Terrain.terrainData;
            var resolution = tData.holesResolution;
            undo.UndoHoles = tData.GetHoles(0, 0, resolution, resolution);

            undo.UndoDetails = new List<int[,]>();
            for (var i = 0; i < tData.detailPrototypes.Length; i++) {
                var details = tData.GetDetailLayer(0, 0, tData.detailWidth, tData.detailHeight, i);
                undo.UndoDetails.Add(details);
            }

            undo.UndoTrees = digger.Terrain.terrainData.treeInstances;
            return undo;
        }

        private void RestoreUndoRecord(UndoRecord undo)
        {
            Refresh();
            var tData = digger.Terrain.terrainData;
            tData.SetHoles(0, 0, undo.UndoHoles);

            for (var i = 0; i < tData.detailPrototypes.Length && i < undo.UndoDetails.Count; i++) {
                tData.SetDetailLayer(0, 0, i, undo.UndoDetails[i]);
            }

            tData.SetTreeInstances(undo.UndoTrees, false);
        }
#endif

        public void LoadFrom(string path)
        {
            if (!File.Exists(path))
                return;

            Refresh();
            var tData = digger.Terrain.terrainData;
            var resolution = tData.holesResolution;
            var holes = new bool[resolution, resolution];
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, LargeFileBufferSize)) {
                using (var reader = new BinaryReader(stream, Encoding.Default)) {
                    var res = reader.ReadInt32();
                    if (res == resolution) {
                        for (var x = 0; x < resolution; ++x) {
                            for (var z = 0; z < resolution; ++z) {
                                holes[z, x] = reader.ReadBoolean();
                            }
                        }
                    }
                }
            }

            tData.SetHoles(0, 0, holes);
        }

        public void SaveTo(string path)
        {
            var tData = digger.Terrain.terrainData;
            var resolution = tData.holesResolution;
            var holes = tData.GetHoles(0, 0, resolution, resolution);
            if (holes == null)
                return;

            if (File.Exists(path))
                File.Delete(path);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, LargeFileBufferSize)) {
                using (var writer = new BinaryWriter(stream, Encoding.Default)) {
                    writer.Write(resolution);
                    for (var x = 0; x < resolution; ++x) {
                        for (var z = 0; z < resolution; ++z) {
                            writer.Write(holes[z, x]);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
#if UNITY_EDITOR
            Utils.Profiler.BeginSample("[Dig] Cutter.Clear");
            Refresh();
            undoRecords.Clear();
            var resolution = digger.Terrain.terrainData.holesResolution;
            var holes = digger.Terrain.terrainData.GetHoles(0, 0, resolution, resolution);
            for (var x = 0; x < resolution; ++x) {
                for (var z = 0; z < resolution; ++z) {
                    holes[z, x] = true;
                }
            }

            digger.Terrain.terrainData.SetHoles(0, 0, holes);
            Utils.Profiler.EndSample();
#endif
        }
    }
}