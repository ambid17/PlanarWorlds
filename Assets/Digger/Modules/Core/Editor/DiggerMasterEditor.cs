using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.TerrainInterface;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Digger.Modules.Core.Editor
{
    [CustomEditor(typeof(DiggerMaster))]
    public class DiggerMasterEditor : UnityEditor.Editor
    {
        private const float RaycastLength = 3000;

        private DiggerMaster master;
        private DiggerSystem[] diggerSystems;
        private IOperationEditor operationEditor;

        private List<Type> operationEditors;

        private int selectedOperationEditorIndex {
            get => EditorPrefs.GetInt("diggerMaster_selectedOperationEditorIndex", 0);
            set => EditorPrefs.SetInt("diggerMaster_selectedOperationEditorIndex", value);
        }

        private int activeTab {
            get => EditorPrefs.GetInt("diggerMaster_activeTab", 0);
            set => EditorPrefs.SetInt("diggerMaster_activeTab", value);
        }

        public static bool shortcutsEnabled {
            get => EditorPrefs.GetBool("diggerMaster_shortcutsEnabled", true);
            set => EditorPrefs.SetBool("diggerMaster_shortcutsEnabled", value);
        }

        public void OnEnable()
        {
            master = (DiggerMaster)target;
            CheckDiggerVersion();
            diggerSystems = FindObjectsOfType<DiggerSystem>();
            foreach (var diggerSystem in diggerSystems) {
                DiggerSystemEditor.Init(diggerSystem, false);
            }

            var type = typeof(IOperationEditor);
            operationEditors = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface)
                .ToList();
            operationEditors.Sort(new OperationAttr.Comparer());

            if (selectedOperationEditorIndex < 0 || selectedOperationEditorIndex >= operationEditors.Count) {
                selectedOperationEditorIndex = 0;
            }

            if (operationEditor == null && operationEditors.Count > 0) {
                operationEditor = (IOperationEditor)Activator.CreateInstance(operationEditors[selectedOperationEditorIndex]);
            }

            operationEditor?.OnDisable();
            operationEditor?.OnEnable();

            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
            Undo.undoRedoPerformed -= UndoCallback;
            Undo.undoRedoPerformed += UndoCallback;

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoCallback;
            SceneView.duringSceneGui -= OnScene;
            operationEditor?.OnDisable();
        }

        private static void UndoCallback()
        {
            var diggers = FindObjectsOfType<DiggerSystem>();
            foreach (var digger in diggers) {
                digger.DoUndo();
            }
        }

        public override void OnInspectorGUI()
        {
            activeTab = GUILayout.Toolbar(activeTab, new[]
            {
                EditorGUIUtility.TrTextContentWithIcon("Edit", "d_TerrainInspector.TerrainToolSplat"),
                EditorGUIUtility.TrTextContentWithIcon("Settings", "d_TerrainInspector.TerrainToolSettings"),
                EditorGUIUtility.TrTextContentWithIcon("Help", "_Help")
            });
            switch (activeTab) {
                case 0:
                    OnInspectorGUIEditTab();
                    break;
                case 1:
                    OnInspectorGUISettingsTab();
                    break;
                case 2:
                    OnInspectorGUIHelpTab();
                    break;
                default:
                    activeTab = 0;
                    break;
            }
        }


        public void OnInspectorGUIHelpTab()
        {
            EditorGUILayout.HelpBox("Thanks for using Digger!\n\n" +
                                    "Need help? Checkout the documentation and join us on Discord to get support!\n\n" +
                                    "Want to help the developer and support the project? Please write a review on the Asset Store!",
                MessageType.Info);


            if (GUILayout.Button("Open documentation")) {
                Application.OpenURL("https://ofux.github.io/Digger-Documentation/");
            }

            if (GUILayout.Button("Open Digger Asset Store page")) {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/digger-terrain-caves-overhangs-135178");
            }

            if (GUILayout.Button("Open Digger PRO Asset Store page")) {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/digger-pro-149753");
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Support is on Discord:", EditorStyles.boldLabel, GUILayout.Width(140));
            var style = new GUIStyle(EditorStyles.textField);
            EditorGUILayout.SelectableLabel("https://discord.gg/C2X6C6s", style, GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();
        }

        public void OnInspectorGUISettingsTab()
        {
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            master.SceneDataFolder = EditorGUILayout.TextField("Scene data folder", master.SceneDataFolder);
            EditorGUILayout.HelpBox($"Digger data for this scene can be found in {master.SceneDataPath}",
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "Don\'t forget to backup this folder (including the \".internal\" folder) as well when you backup your project.",
                MessageType.Warning);
            EditorGUILayout.Space();

            shortcutsEnabled = EditorGUILayout.Toggle("Enable shortcuts", shortcutsEnabled);
            if (shortcutsEnabled) {
                EditorGUILayout.HelpBox("Change Brush: B\n" +
                                        "Change Action: N\n" +
                                        "Brush size: keypad - or +\n" +
                                        "Opacity: keypad / or *",
                    MessageType.Info);
            }

            EditorGUILayout.Space();

            var showUnderlyingObjects = EditorGUILayout.Toggle("Show underlying objects", master.ShowUnderlyingObjects);
            if (showUnderlyingObjects != master.ShowUnderlyingObjects) {
                master.ShowUnderlyingObjects = showUnderlyingObjects;
                var diggers = FindObjectsOfType<DiggerSystem>();
                foreach (var digger in diggers) {
                    digger.ShowDebug = true;
                    foreach (Transform child in digger.transform) {
                        child.gameObject.hideFlags = showUnderlyingObjects
                            ? HideFlags.None
                            : HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    }
                }

                EditorApplication.DirtyHierarchyWindowSorting();
                EditorApplication.RepaintHierarchyWindow();
                if (showUnderlyingObjects) {
                    EditorUtility.DisplayDialog("Please reload the scene",
                        "You need to reload the scene (or restart Unity) in order to see chunk objects under LOD Groups.",
                        "Ok");
                }
            }

            EditorGUILayout.HelpBox(
                "Enable this to reveal all objects created by Digger in the hierarchy. Digger creates objects as children of your terrain(s).",
                MessageType.Info);
            EditorGUILayout.Space();

            var newLayer = EditorGUILayout.LayerField("Layer", master.Layer);
            EditorGUILayout.HelpBox("You can change the layer of meshes/objects generated by Digger.",
                MessageType.Info);
            if (newLayer != master.Layer && EditorUtility.DisplayDialog(
                    $"Set new layer: {LayerMask.LayerToName(newLayer)}",
                    "Digger must recompute internal chunks for the new layer setting to take effect.\n\n" +
                    "This operation is not destructive, but can be long.\n\n" +
                    "Do you want to proceed?", "Yes", "Cancel")) {
                master.Layer = newLayer;
                DoReload();
            }

            var newTag = EditorGUILayout.TagField("Tag", master.ChunksTag);
            EditorGUILayout.HelpBox("You can change the tag of objects generated by Digger.",
                MessageType.Info);
            if (newTag != master.ChunksTag && EditorUtility.DisplayDialog(
                    $"Set new tag: {newTag}",
                    "Digger must recompute internal chunks for the new tag setting to take effect.\n\n" +
                    "This operation is not destructive, but can be long.\n\n" +
                    "Do you want to proceed?", "Yes", "Cancel")) {
                master.ChunksTag = newTag;
                DoReload();
            }

            EditorGUILayout.Space();
            var newChunkSize = EditorGUILayout.IntPopup("Chunk size", master.ChunkSize, new[] { "16", "32", "64" },
                new[] { 17, 33, 65 });
            EditorGUILayout.HelpBox(
                "Lowering the size of chunks improves real-time editing performance, but also creates more meshes.",
                MessageType.Info);
            if (newChunkSize != master.ChunkSize && EditorUtility.DisplayDialog("Change chunk size & clear everything",
                    "All modifications must be cleared for new chunk size to take effect.\n\n" +
                    "THIS WILL CLEAR ALL MODIFICATIONS MADE WITH DIGGER.\n\n" +
                    "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees that were removed by Digger won't be restored.\n\n" +
                    "This operation CANNOT BE UNDONE.\n\n" +
                    "Are you sure you want to proceed?", "Yes, clear it", "Cancel")) {
                master.ChunkSize = newChunkSize;
                DoClear();
            }

            EditorGUILayout.Space();
            var newResolutionMult = EditorGUILayout.IntPopup("Resolution", master.ResolutionMult,
                new[] { "x1", "x2", "x4", "x8" }, new[] { 1, 2, 4, 8 });
            if (newResolutionMult != master.ResolutionMult && EditorUtility.DisplayDialog(
                    "Change resolution & clear everything",
                    "All modifications must be cleared for new resolution to take effect.\n\n" +
                    "THIS WILL CLEAR ALL MODIFICATIONS MADE WITH DIGGER.\n\n" +
                    "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees that were removed by Digger won't be restored.\n\n" +
                    "This operation CANNOT BE UNDONE.\n\n" +
                    "Are you sure you want to proceed?", "Yes, clear it", "Cancel")) {
                master.ResolutionMult = newResolutionMult;
                DoClear();
            }

            EditorGUILayout.HelpBox(
                "If your heightmaps have a low resolution, you might want to set this to x2, x4 or x8 to generate " +
                "meshes with higher resolution and finer details. " +
                "However, keep in mind that the higher the resolution is, the more performance will be impacted.",
                MessageType.Info);

            EditorGUILayout.Space();
            var newAutoVoxelHeight = EditorGUILayout.Toggle("Auto Voxel Height", master.AutoVoxelHeight);
            if (newAutoVoxelHeight != master.AutoVoxelHeight && EditorUtility.DisplayDialog(
                    "Change voxel height & clear everything",
                    "All modifications must be cleared for new voxel height to take effect.\n\n" +
                    "THIS WILL CLEAR ALL MODIFICATIONS MADE WITH DIGGER.\n\n" +
                    "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees that were removed by Digger won't be restored.\n\n" +
                    "This operation CANNOT BE UNDONE.\n\n" +
                    "Are you sure you want to proceed?", "Yes, clear it", "Cancel")) {
                master.AutoVoxelHeight = newAutoVoxelHeight;
                DoClear();
            }

            if (!master.AutoVoxelHeight) {
                var newVoxelHeight = EditorGUILayout.DelayedFloatField("Voxel Height", master.VoxelHeight);
                if (!Mathf.Approximately(newVoxelHeight, master.VoxelHeight) && EditorUtility.DisplayDialog(
                        "Change voxel height & clear everything",
                        "All modifications must be cleared for new voxel height to take effect.\n\n" +
                        "THIS WILL CLEAR ALL MODIFICATIONS MADE WITH DIGGER.\n\n" +
                        "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees that were removed by Digger won't be restored.\n\n" +
                        "This operation CANNOT BE UNDONE.\n\n" +
                        "Are you sure you want to proceed?", "Yes, clear it", "Cancel")) {
                    master.VoxelHeight = newVoxelHeight;
                    DoClear();
                }
            }

            EditorGUILayout.HelpBox(
                "You can change the height of each voxel to suit your needs. When 'Auto Voxel Height' is enabled, " +
                "the height of voxels will be set accordingly with the heightmap resolution.",
                MessageType.Info);

            EditorGUILayout.Space();
            var newEnableOcclusionCulling =
                EditorGUILayout.Toggle("Enable Occlusion Culling", master.EnableOcclusionCulling);
            if (newEnableOcclusionCulling != master.EnableOcclusionCulling && EditorUtility.DisplayDialog(
                    $"{(newEnableOcclusionCulling ? "Enable" : "Disable")} Occlusion Culling",
                    "Digger must recompute internal chunks for the new Occlusion Culling setting to take effect.\n\n" +
                    "This operation is not destructive, but can be long.\n\n" +
                    "Do you want to proceed?", "Yes", "Cancel")) {
                master.EnableOcclusionCulling = newEnableOcclusionCulling;
                DoReload();
            }

            EditorGUILayout.HelpBox(
                "Occlusion Culling is not very stable on some versions of Unity, and Unity throws some errors (while it shouldn't) " +
                "when baking Occlusion Culling if some meshes have multiple materials for a single submesh. This is why it is disabled by default on Digger.\n\n" +
                "Note: this setting has no effect if DiggerMasterRuntime (with Digger Runtime module only) is present in the scene as chunks are not static anymore in such case.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("LOD Settings", EditorStyles.boldLabel);
            var newCreateLODs = EditorGUILayout.Toggle("Enable LODs generation", master.CreateLODs);
            if (newCreateLODs != master.CreateLODs && EditorUtility.DisplayDialog(
                    $"{(newCreateLODs ? "Enable" : "Disable")} LODs generation",
                    "Digger must recompute internal chunks for the new LODs generation setting to take effect.\n\n" +
                    "This operation is not destructive, but can be long.\n\n" +
                    "Do you want to proceed?", "Yes", "Cancel")) {
                master.CreateLODs = newCreateLODs;
                DoReload();
            }

            if (master.CreateLODs) {
                if (FindObjectOfType<ADiggerRuntimeMonoBehaviour>()) {
                    EditorGUILayout.HelpBox(
                        "It is recommended to disable LODs generation when using Digger at runtime to improve generation speed.",
                        MessageType.Warning);
                }

                EditorGUILayout.LabelField("Screen Relative Transition Height of LODs:");
                master.ScreenRelativeTransitionHeightLod0 = EditorGUILayout.Slider("    LOD 0",
                    master.ScreenRelativeTransitionHeightLod0, 0.002f, 0.9f);
                master.ScreenRelativeTransitionHeightLod1 = EditorGUILayout.Slider("    LOD 1",
                    master.ScreenRelativeTransitionHeightLod1, 0.001f,
                    master.ScreenRelativeTransitionHeightLod0 - 0.001f);
                master.ColliderLodIndex = EditorGUILayout.IntSlider(
                    new GUIContent("Collider LOD",
                        "LOD that will hold the collider. Increasing it will produce mesh colliders with fewer vertices but also less accuracy."),
                    master.ColliderLodIndex, 0, 2);
            }

            EditorGUILayout.Space();
            OnInspectorGUIClearButtons();
        }

        public void OnInspectorGUIEditTab()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editing", EditorStyles.boldLabel);

            HandleShortcuts();

            var idx = EditorGUILayout.Popup("Operation", selectedOperationEditorIndex,
                operationEditors.Select(type => type.GetCustomAttribute(typeof(OperationAttr)) is OperationAttr attr ? attr.Name : type.Name).ToArray());
            if (idx != selectedOperationEditorIndex) {
                ChangeSelectedOperation(idx);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            operationEditor?.OnInspectorGUI();

            EditorGUILayout.Space();
            OnInspectorGUIClearButtons();
        }

        private void ChangeSelectedOperation(int newIndex)
        {
            selectedOperationEditorIndex = newIndex;
            operationEditor?.OnDisable();
            operationEditor = (IOperationEditor)Activator.CreateInstance(operationEditors[selectedOperationEditorIndex]);
            operationEditor.OnEnable();
            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            operationEditor?.OnSceneGUI();
        }

        public static int TextureSelector(int textureIndex, DiggerSystem diggerSystem)
        {
            GUIStyle gridList = "GridList";
            var errorMessage = new GUIContent("No texture to display.\n\n" +
                                              "You have to add at least one layer to the terrain with " +
                                              "both a texture and a normal map. Then, click on 'Sync & Refresh'.");
            if (diggerSystem.MaterialType == TerrainMaterialType.CTS) {
                errorMessage =
                    new GUIContent(
                        "CTS does not support vertex control. You can't choose which texture to paint. Texture will be picked-up from the terrain above.");
            }

            return EditorUtils.AspectSelectionGrid(textureIndex, diggerSystem.TerrainTextures, 64,
                gridList, errorMessage);
        }

        private void HandleShortcuts()
        {
            if (!shortcutsEnabled)
                return;

            var current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.N) {
                var idx = (selectedOperationEditorIndex + 1) % operationEditors.Count;
                ChangeSelectedOperation(idx);
            }
        }

        private void OnScene(SceneView sceneview)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            if (e.type == EventType.Layout || e.type == EventType.Repaint) {
                HandleUtility.AddDefaultControl(controlId);
                return;
            }

            operationEditor?.OnScene(this, sceneview);
        }

        private void OnInspectorGUIClearButtons()
        {
            EditorGUILayout.LabelField("Utils", EditorStyles.boldLabel);
            var col = GUI.backgroundColor;

            if (GUILayout.Button(new GUIContent("Save Meshes as Assets", "Save meshes as assets so they are not serialized within the scene file. " +
                                                                         "This will make the scene file a lot lighter and can reduce loading time."))) {
                SaveMeshesAsAssets();
            }

            GUI.backgroundColor = new Color(0.47f, 1f, 0.46f);
            var doReload = GUILayout.Button(new GUIContent("Sync with terrain(s) & Refresh", "Synchronize Digger with terrain data and recompute " +
                                                                                             "all modifications made with Digger. Press this button if you made any " +
                                                                                             "change to your terrain(s), like raising or lowering height, changing textures, etc."));
            if (doReload) {
                DoReload();
            }

            GUI.backgroundColor = new Color(1f, 0.55f, 0.57f);
            var doClear = GUILayout.Button("Clear") && EditorUtility.DisplayDialog("Clear",
                "This will clear all modifications made with Digger.\n" +
                "This operation CANNOT BE UNDONE.\n\n" +
                "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees that were removed by Digger won't be restored.\n\n" +
                "Are you sure you want to proceed?", "Yes, clear it", "Cancel");
            if (doClear) {
                DoClear();
            }

            GUI.backgroundColor = col;
        }

        private static void DoClear()
        {
            var diggers = FindObjectsOfType<DiggerSystem>();

            try {
                AssetDatabase.StartAssetEditing();
                foreach (var digger in diggers) {
                    digger.Clear();
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();

            try {
                AssetDatabase.StartAssetEditing();
                foreach (var digger in diggers) {
                    DiggerSystemEditor.Init(digger, true);
                    Undo.ClearUndo(digger);
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            GUIUtility.ExitGUI();
        }

        private static void DoReload()
        {
            var diggers = FindObjectsOfType<DiggerSystem>();
            try {
                AssetDatabase.StartAssetEditing();
                foreach (var digger in diggers) {
                    DiggerSystemEditor.Init(digger, true);
                    Undo.ClearUndo(digger);
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            GUIUtility.ExitGUI();
        }


        public static RaycastHit? GetIntersectionWithTerrainOrDigger(Ray ray)
        {
            if (Physics.Raycast(ray, out var hit, RaycastLength, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)) {
                return hit;
            }

            return null;
        }

        public static RaycastHit? GetIntersectionWithTerrain(Ray ray)
        {
            var hits = Physics.RaycastAll(ray, RaycastLength, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            foreach (var hit in hits) {
                if (hit.transform.GetComponent<Terrain>() != null) {
                    return hit;
                }
            }

            return null;
        }

        [MenuItem("Tools/Digger/Setup terrains", false, 1)]
        public static void SetupTerrains()
        {
            if (!FindObjectOfType<DiggerMaster>()) {
                var goMaster = new GameObject("Digger Master");
                goMaster.transform.localPosition = Vector3.zero;
                goMaster.transform.localRotation = Quaternion.identity;
                goMaster.transform.localScale = Vector3.one;
                var master = goMaster.AddComponent<DiggerMaster>();
                master.CreateDirs();
            }

            var isCTS = false;
            var lightmapStaticWarn = false;
            var terrains = FindObjectsOfType<Terrain>();
            try {
                AssetDatabase.StartAssetEditing();

                foreach (var terrain in terrains) {
                    var existingDiggers = terrain.gameObject.GetComponentsInChildren<DiggerSystem>();
                    if (existingDiggers.Count(system => system.Terrain.GetInstanceID() == terrain.GetInstanceID()) ==
                        0) {
                        var go = new GameObject("Digger");
                        go.transform.parent = terrain.transform;
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;
                        var digger = go.AddComponent<DiggerSystem>();
                        DiggerSystemEditor.Init(digger, true);
                        isCTS = isCTS || digger.MaterialType == TerrainMaterialType.CTS;
                        lightmapStaticWarn =
                            lightmapStaticWarn || GameObjectUtility.GetStaticEditorFlags(terrain.gameObject).HasFlag(StaticEditorFlags.ContributeGI);
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (lightmapStaticWarn) {
                const string title = "Warning: Contribute GI is enabled";
                const string text = "It is recommended to disable 'Contribute GI' on terrains " +
                                    "when using Digger. Otherwise there might be a visual difference between " +
                                    "Digger meshes and the terrains.\n\n" +
                                    "To disable Global Illumination (GI) on a terrain, go to terrain settings and uncheck " +
                                    "'Contribute GI'.";
                if (!EditorUtility.DisplayDialog(title, text, "Ok", "Terrain settings?")) {
                    Application.OpenURL("https://docs.unity3d.com/Manual/terrain-OtherSettings.html");
                }
            }

            if (isCTS) {
                EditorUtility.DisplayDialog("Warning - CTS",
                    "Digger has detected CTS on your terrain(s) and has been setup accordingly.\n\n" +
                    "You may have to close the scene and open it again (or restart Unity) to " +
                    "force it to refresh before using Digger.", "Ok");
            }
        }


        [MenuItem("Tools/Digger/Remove Digger from the scene", false, 4)]
        public static void RemoveDiggerFromTerrains()
        {
            var confirm = EditorUtility.DisplayDialog("Remove Digger from the scene",
                "You are about to completely remove Digger from the scene and clear all related Digger data.\n" +
                "Terrain holes will be removed, but unlike undo (Ctrl+Z), details objects and trees won't be restored.\n\n" +
                "This operation CANNOT BE UNDONE.\n\n" +
                "Are you sure you want to proceed?", "Yes, remove Digger", "Cancel");
            if (!confirm)
                return;

            var terrains = FindObjectsOfType<Terrain>();
            foreach (var terrain in terrains) {
                var digger = terrain.gameObject.GetComponentInChildren<DiggerSystem>();
                if (digger) {
                    digger.Clear();
                    DestroyImmediate(digger.gameObject);
                }
            }

            var diggerMasters = FindObjectsOfType<ADiggerMonoBehaviour>();
            foreach (var diggerMaster in diggerMasters) {
                DestroyImmediate(diggerMaster.gameObject);
            }

            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static void LoadAllChunks(Scene scene)
        {
            var diggers = FindObjectsOfTypeInScene<DiggerSystem>(scene);
            foreach (var diggerSystem in diggers) {
                DiggerSystemEditor.Init(diggerSystem, false);
                Undo.ClearUndo(diggerSystem);
            }
        }

        public static void OnEnterPlayMode(Scene scene)
        {
            var diggers = FindObjectsOfTypeInScene<DiggerSystem>(scene);
            foreach (var digger in diggers) {
                Undo.ClearUndo(digger);
                Undo.ClearUndo(digger.Terrain);
            }

            var cutters = FindObjectsOfTypeInScene<TerrainCutter>(scene);
            foreach (var cutter in cutters) {
                cutter.OnEnterPlayMode();
            }
        }

        private static List<T> FindObjectsOfTypeInScene<T>(Scene scene) where T : MonoBehaviour
        {
            var list = new List<T>();
            var rootObjects = scene.GetRootGameObjects();
            foreach (var rootObject in rootObjects) {
                var obj = rootObject.GetComponentInChildren<T>();
                if (obj) {
                    list.Add(obj);
                }
            }

            return list;
        }

        [MenuItem("Tools/Digger/Save Meshes as Assets", false, 30)]
        public static void SaveMeshesAsAssets()
        {
            try {
                AssetDatabase.StartAssetEditing();
                var diggers = FindObjectsOfType<DiggerSystem>();
                foreach (var digger in diggers) {
                    digger.SaveMeshesAsAssets();
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Tools/Digger/Check Digger data", false, 31)]
        public static void CheckDiggerVersion()
        {
            var warned = false;
            var diggers = FindObjectsOfType<DiggerSystem>();
            foreach (var digger in diggers) {
                var ver = digger.GetDiggerVersion();
                if (ver != DiggerSystem.DiggerVersion) {
                    if (!warned) {
                        warned = true;
                        EditorUtility.DisplayDialog("New Digger version",
                            "Looks like Digger was updated. Digger is going to synchronize and reload all its data " +
                            "to ensure compatibility. This may take a while.\n\nDon't forget to save your scene once this is done.",
                            "Ok");
                    }

                    // ensure retro-compatibility before 4.0
                    if (ver < 40 || ver >= 10000 && ver < 10040) {
                        var diggerMaster = FindObjectOfType<DiggerMaster>();
                        if (diggerMaster) {
                            diggerMaster.AutoVoxelHeight = false;
                            diggerMaster.VoxelHeight = 1f;
                        }
                    }

                    DiggerSystemEditor.Init(digger, true);
                    Undo.ClearUndo(digger);
                }
            }

            if (warned) {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Undo.ClearAll();
            }
        }

        public static GameObject LoadAssetWithLabel(string label)
        {
            var guids = AssetDatabase.FindAssets($"l:{label}");
            if (guids == null || guids.Length == 0) {
                return null;
            }

            // we loop but there should be only one item in the list
            foreach (var guid in guids) {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var labels = AssetDatabase.GetLabels(asset);
                if (labels != null && labels.Contains(label)) {
                    return asset;
                }
            }

            return null;
        }


        public static string GetReticleLabel(string label)
        {
            if (GraphicsSettings.renderPipelineAsset == null) {
                return label;
            }

            if (GraphicsSettings.renderPipelineAsset.name.Contains("HDRenderPipeline") || GraphicsSettings.renderPipelineAsset.name.Contains("HDRP")) {
                return label + "HDRP";
            }

            return label + "SRP";
        }

        private void OnBeforeAssemblyReload()
        {
            NativeCollectionsPool.Instance.Dispose();
        }

        private void OnAfterAssemblyReload()
        {
            CheckDiggerVersion();
        }
    }
}