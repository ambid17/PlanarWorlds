using System;
using System.Collections;
using System.Collections.Generic;
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Operations;
using Unity.Jobs;
using UnityEngine;

namespace Digger.Modules.Runtime.Sources
{
    /// <summary>
    /// To support real-time / in-game editing, add this component into the scene and use its 'Modify' method to edit the terrain.
    /// </summary>
    /// <see cref="DiggerRuntimeUsageExample">Example of use</see>
    [DefaultExecutionOrder(-10)]
    public class DiggerMasterRuntime : ADiggerRuntimeMonoBehaviour
    {
        public bool enablePersistence;
        private DiggerSystem[] diggerSystems;

        private bool isRunningAsync;
        private readonly Queue<ModificationParameters> buffer = new Queue<ModificationParameters>();
        private int bufferSize = 1;

        private readonly BasicOperation basicOperation = new BasicOperation();
        private readonly KernelOperation kernelOperation = new KernelOperation();

        /// <summary>
        /// True when a modification is currently being done asynchronously.
        /// </summary>
        public bool IsRunningAsync => isRunningAsync;

        public int BufferSize {
            get => bufferSize;
            set => bufferSize = Math.Max(1, value);
        }

        /// <summary>
        /// Modify the terrain at runtime by performing the operation synchronously.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        public void Modify<T>(IOperation<T> operation) where T : struct, IJobParallelFor
        {
            if (isRunningAsync) {
                Debug.LogError("Cannot Modify as asynchronous modification is already in progress");
                return;
            }

            foreach (var diggerSystem in diggerSystems) {
                diggerSystem.Modify(operation);
            }
        }


        /// <summary>
        /// Modify the terrain at runtime by performing the requested action.
        /// </summary>
        /// <param name="position">Position where you want to edit the terrain</param>
        /// <param name="brush">Brush type</param>
        /// <param name="action">Action type</param>
        /// <param name="textureIndex">Index of the texture to be used (starting from 0 to 7). See DiggerMaster inspector to know what texture corresponds to an index.</param>
        /// <param name="opacity">Strength/intensity of edit</param>
        /// <param name="size">Size of edit</param>
        /// <param name="stalagmiteHeight">Height of stalagmite (only when Brush is stalagmite)</param>
        /// <param name="stalagmiteUpsideDown">Defines if stalagmite is upside-down or not (only when Brush is stalagmite)</param>
        /// <param name="opacityIsTarget">If true when painting texture, the weight of the texture will be directly set to the given opacity</param>
        public void Modify(Vector3 position, BrushType brush, ActionType action, int textureIndex, float opacity,
            float size, float stalagmiteHeight = 8f, bool stalagmiteUpsideDown = false, bool opacityIsTarget = false)
        {
            if (action == ActionType.Smooth && brush != BrushType.Sphere) {
                Debug.LogError("Smooth action only supports Sphere brush");
                return;
            }

            var p = new ModificationParameters
            {
                Position = position,
                Brush = brush,
                Action = action,
                TextureIndex = textureIndex,
                Opacity = opacity,
                Size = size,
                StalagmiteUpsideDown = stalagmiteUpsideDown,
                OpacityIsTarget = opacityIsTarget,
                Callback = null
            };

            if (action == ActionType.Smooth || action == ActionType.BETA_Sharpen) {
                kernelOperation.Params = p;
                Modify(kernelOperation);
            } else {
                basicOperation.Params = p;
                Modify(basicOperation);
            }
        }

        /// <summary>
        /// Modify the terrain at runtime by performing the operation asynchronously to have less impact on frame rate.
        /// It is NOT possible to call this method if another asynchronous modification is already in progress.
        /// Use "IsRunningAsync" property to check if an asynchronous modification is already in progress.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="callback">Callback method called once modification is done</param>
        public IEnumerator ModifyAsync<T>(IOperation<T> operation, Action callback = null) where T : struct, IJobParallelFor
        {
            if (isRunningAsync) {
                Debug.LogError("Cannot Modify as asynchronous modification is already in progress");
                yield break;
            }

            isRunningAsync = true;

            foreach (var diggerSystem in diggerSystems) {
                var area = operation.GetAreaToModify(diggerSystem);
                if (!area.NeedsModification)
                    continue;

                yield return diggerSystem.ModifyAsync(operation);
            }

            foreach (var diggerSystem in diggerSystems) {
                var area = operation.GetAreaToModify(diggerSystem);
                if (!area.NeedsModification)
                    continue;

                diggerSystem.ApplyModify();
            }

            isRunningAsync = false;
            callback?.Invoke();
        }

        /// <summary>
        /// Modify the terrain at runtime by performing the requested action asynchronously to have less impact on frame rate.
        /// It is NOT possible to call this method if another asynchronous modification is already in progress.
        /// Use "IsRunningAsync" property to check if an asynchronous modification is already in progress.
        /// </summary>
        /// <param name="position">Position where you want to edit the terrain</param>
        /// <param name="brush">Brush type</param>
        /// <param name="action">Action type</param>
        /// <param name="textureIndex">Index of the texture to be used (starting from 0 to 7). See DiggerMaster inspector to know what texture corresponds to an index.</param>
        /// <param name="opacity">Strength/intensity of edit</param>
        /// <param name="size">Size of edit</param>
        /// <param name="stalagmiteHeight">Height of stalagmite (only when Brush is stalagmite)</param>
        /// <param name="stalagmiteUpsideDown">Defines if stalagmite is upside-down or not (only when Brush is stalagmite)</param>
        /// <param name="opacityIsTarget">If true when painting texture, the weight of the texture will be directly set to the given opacity</param>
        /// <param name="callback">A callback function that will be called once modification is done</param>
        public IEnumerator ModifyAsync(Vector3 position, BrushType brush, ActionType action, int textureIndex, float opacity,
            float size, float stalagmiteHeight = 8f, bool stalagmiteUpsideDown = false, bool opacityIsTarget = false, Action callback = null)
        {
            if (action == ActionType.Smooth && brush != BrushType.Sphere) {
                Debug.LogError("Smooth action only supports Sphere brush");
                brush = BrushType.Sphere;
            }

            var p = new ModificationParameters
            {
                Position = position,
                Brush = brush,
                Action = action,
                TextureIndex = textureIndex,
                Opacity = opacity,
                Size = size,
                StalagmiteUpsideDown = stalagmiteUpsideDown,
                OpacityIsTarget = opacityIsTarget,
                Callback = callback
            };

            if (action == ActionType.Smooth || action == ActionType.BETA_Sharpen) {
                kernelOperation.Params = p;
                return ModifyAsync(kernelOperation, callback);
            }

            basicOperation.Params = p;
            return ModifyAsync(basicOperation, callback);
        }

        /// <see cref="ModifyAsync(UnityEngine.Vector3,BrushType,ActionType,int,float,float,bool,bool,float,bool,bool,System.Action)"/>
        /// <param name="p">Modification parameters</param>
        public IEnumerator ModifyAsync(ModificationParameters p)
        {
            return ModifyAsync(p.Position, p.Brush, p.Action, p.TextureIndex, p.Opacity, p.Size.x, p.Size.y, p.StalagmiteUpsideDown, p.OpacityIsTarget, p.Callback);
        }

        /// <summary>
        /// Modify the terrain at runtime by performing the requested action asynchronously to have less impact on frame rate.
        /// If another asynchronous modification is already in progress, this modification will be added to a buffer and will be performed later.
        /// If the buffer is full, the modification is discarded and this method returns false.
        /// </summary>
        /// <returns>True if the modification could be appended to the buffer. False if the buffer is full and modification is discarded.</returns>
        /// <param name="position">Position where you want to edit the terrain</param>
        /// <param name="brush">Brush type</param>
        /// <param name="action">Action type</param>
        /// <param name="textureIndex">Index of the texture to be used (starting from 0 to 7). See DiggerMaster inspector to know what texture corresponds to an index.</param>
        /// <param name="opacity">Strength/intensity of edit</param>
        /// <param name="size">Size of edit</param>
        /// <param name="stalagmiteHeight">Height of stalagmite (only when Brush is stalagmite)</param>
        /// <param name="stalagmiteUpsideDown">Defines if stalagmite is upside-down or not (only when Brush is stalagmite)</param>
        /// <param name="opacityIsTarget">If true when painting texture, the weight of the texture will be directly set to the given opacity</param>
        /// <param name="callback">A callback function that will be called once modification is done</param>
        public bool ModifyAsyncBuffured(Vector3 position, BrushType brush, ActionType action, int textureIndex, float opacity,
            float size, float stalagmiteHeight = 8f, bool stalagmiteUpsideDown = false, bool opacityIsTarget = false, Action callback = null)
        {
            if (buffer.Count >= BufferSize) {
                return false;
            }

            return ModifyAsyncBuffured(new ModificationParameters
            {
                Position = position,
                Brush = brush,
                Action = action,
                TextureIndex = textureIndex,
                Opacity = opacity,
                Size = size,
                StalagmiteUpsideDown = stalagmiteUpsideDown,
                OpacityIsTarget = opacityIsTarget,
                Callback = callback
            });
        }

        /// <see cref="ModifyAsyncBuffured(UnityEngine.Vector3,BrushType,ActionType,int,float,float,bool,bool,float,bool,bool,System.Action)"/>
        /// <param name="parameters">Modification parameters</param>
        public bool ModifyAsyncBuffured(ModificationParameters parameters)
        {
            if (buffer.Count >= BufferSize) {
                return false;
            }

            buffer.Enqueue(parameters);
            return true;
        }

        private void Update()
        {
            if (!isRunningAsync && buffer.Count > 0) {
                var parameters = buffer.Dequeue();
                StartCoroutine(ModifyAsync(parameters));
            }
        }

        /// <summary>
        /// Persists all modifications made with Digger since the last persist call.
        /// </summary>
        public void PersistAll()
        {
            if (isRunningAsync) {
                Debug.LogError("Cannot Persist as asynchronous modification is already in progress");
                return;
            }

            if (!Application.isEditor) {
                foreach (var diggerSystem in diggerSystems) {
                    diggerSystem.PersistAtRuntime();
                }
            } else {
                Debug.Log("Digger 'PersistAll' method has no effect in Unity editor");
            }
        }

        /// <summary>
        /// Deletes all Digger data that was persisted at runtime.
        /// </summary>
        public void DeleteAllPersistedData()
        {
            if (!Application.isEditor) {
                foreach (var diggerSystem in diggerSystems) {
                    diggerSystem.DeleteDataPersistedAtRuntime();
                }
            } else {
                Debug.Log("Digger 'DeleteAllPersistedData' method has no effect in Unity editor");
            }
        }

        /// <summary>
        /// When a path-prefix is specified, Digger will persist/delete data at "Application.persistentDataPath/DiggerData/pathPrefix/".
        /// Otherwise it will persist/delete data at "Application.persistentDataPath/DiggerData/"
        /// </summary>
        /// <param name="pathPrefix"></param>
        public void SetPersistenceDataPathPrefix(string pathPrefix)
        {
            // we do not use diggerSystems field as it might not be initialized when this method is called
            foreach (var diggerSystem in FindObjectsOfType<DiggerSystem>()) {
                diggerSystem.PersistenceSubPath = pathPrefix;
            }
        }

        /// <summary>
        /// Setups Digger on a new terrain that has been created at runtime.
        /// There MUST be at least one other terrain with Digger already setup at edit-time.
        /// </summary>
        /// <param name="terrain">Terrain on which Digger must be added</param>
        /// <param name="guid">Optionally, you can set a specific Digger GUID for this terrain. This is useful if you plan to persist data of terrains created at runtime to be able to load data back on the next launch.</param>
        public void SetupRuntimeTerrain(Terrain terrain, string guid = null)
        {
            var existingDiggerSystem = FindObjectOfType<DiggerSystem>();
            if (!existingDiggerSystem) {
                Debug.LogError(
                    "SetupRuntimeTerrain needs at least one terrain to be already setup with Digger. You must have at least one terrain with Digger on it " +
                    "to be able to setup other terrains at runtime");
                return;
            }

            var go = new GameObject("Digger");
            go.transform.parent = terrain.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var diggerSystem = go.AddComponent<DiggerSystem>();

            diggerSystem.Guid = guid;
            diggerSystem.PreInit(true);

            diggerSystem.PersistenceSubPath = existingDiggerSystem.PersistenceSubPath;
            diggerSystem.Materials = existingDiggerSystem.Materials;
            diggerSystem.TerrainTextures = existingDiggerSystem.TerrainTextures;
            diggerSystem.Terrain.terrainData.enableHolesTextureCompression = false;
            diggerSystem.Terrain.materialTemplate = existingDiggerSystem.Terrain.materialTemplate;

            diggerSystem.Init(Application.isEditor ? LoadType.Minimal : LoadType.Minimal_and_LoadVoxels);
            RefreshTerrainList();
        }

        /// <summary>
        /// Refreshes the list of available Digger systems at runtime.
        /// Call this after you delete a terrain at runtime (for example).
        /// </summary>
        public void RefreshTerrainList()
        {
            diggerSystems = FindObjectsOfType<DiggerSystem>();
        }

        private void Awake()
        {
            diggerSystems = FindObjectsOfType<DiggerSystem>();
            foreach (var diggerSystem in diggerSystems) {
                Init(diggerSystem);
            }
        }

        private void Init(DiggerSystem diggerSystem)
        {
            if (diggerSystem.IsInitialized)
                return;

            diggerSystem.Terrain.terrainData.enableHolesTextureCompression = false;
            diggerSystem.PreInit(enablePersistence);
            diggerSystem.Init(Application.isEditor ? LoadType.Minimal : LoadType.Minimal_and_LoadVoxels);
        }
    }
}