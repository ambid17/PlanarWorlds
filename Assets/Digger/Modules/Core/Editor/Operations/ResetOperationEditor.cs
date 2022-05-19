using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Digger.Modules.Core.Editor.Operations
{
    [OperationAttr("Reset", 1000000)]
    public class ResetOperationEditor : IOperationEditor
    {
        private DiggerSystem[] diggerSystems;
        private readonly BasicOperation basicOperation = new BasicOperation();

        private bool clicking;
        private bool keepingHeight;
        private float keptHeight;
        private bool warnedAboutPlayMode;

        private GameObject reticleSphere;

        private float size {
            get => EditorPrefs.GetFloat("diggerMaster_size", 3f);
            set => EditorPrefs.SetFloat("diggerMaster_size", value);
        }

        private float depth {
            get => EditorPrefs.GetFloat("diggerMaster_depth", 0f);
            set => EditorPrefs.SetFloat("diggerMaster_depth", value);
        }

        private GameObject ReticleSphere {
            get {
                if (!reticleSphere) {
                    var prefab = DiggerMasterEditor.LoadAssetWithLabel(DiggerMasterEditor.GetReticleLabel("Digger_SphereReticle"));
                    reticleSphere = Object.Instantiate(prefab);
                    reticleSphere.hideFlags = HideFlags.HideAndDontSave;
                }

                return reticleSphere;
            }
        }

        private GameObject Reticle => ReticleSphere;

        public void OnEnable()
        {
            diggerSystems = Object.FindObjectsOfType<DiggerSystem>();
        }

        public void OnDisable()
        {
            if (reticleSphere)
                Object.DestroyImmediate(reticleSphere);
        }

        public void OnInspectorGUI()
        {
            var diggerSystem = Object.FindObjectOfType<DiggerSystem>();
            if (!diggerSystem)
                return;

            size = EditorGUILayout.Slider(new GUIContent("Brush Size", DiggerMasterEditor.shortcutsEnabled ? "Shortcut: keypad - or +" : ""), size, 0.5f, 20f);
            depth = EditorGUILayout.Slider("Depth", depth, -size, size);

            EditorGUILayout.Space();
            keepingHeight = EditorGUILayout.ToggleLeft("Constrain reticle to given altitude", keepingHeight);
            keptHeight = EditorGUILayout.FloatField("Reticle constrained altitude", keptHeight);
            EditorGUILayout.HelpBox(
                "Press Shift to pick current reticle height.",
                MessageType.Info);
        }
        
        public void OnSceneGUI()
        {
        }

        public void OnScene(UnityEditor.Editor editor, SceneView sceneview)
        {
            var e = Event.current;
            HandleShortcuts(editor);

            if (!clicking && !e.alt && e.type == EventType.MouseDown && e.button == 0) {
                clicking = true;
                if (!Application.isPlaying) {
                    foreach (var diggerSystem in diggerSystems) {
                        diggerSystem.PrepareModification();
                    }
                }
            } else if (clicking && (e.type == EventType.MouseUp || e.type == EventType.MouseLeaveWindow ||
                                    (e.isKey && !e.control && !e.shift) ||
                                    e.alt || EditorWindow.mouseOverWindow == null ||
                                    EditorWindow.mouseOverWindow.GetType() != typeof(SceneView))) {
                clicking = false;
                if (!Application.isPlaying) {
                    foreach (var diggerSystem in diggerSystems) {
                        diggerSystem.PersistAndRecordUndo(false, true);
                    }
                }
            }

            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = DiggerMasterEditor.GetIntersectionWithTerrainOrDigger(ray);
            if (!hit.HasValue)
                return;

            var p = hit.Value.point + depth * ray.direction.normalized;

            if (e.shift) {
                keptHeight = p.y;
                editor.Repaint();
            }

            if (keepingHeight) {
                p.y = keptHeight;
            }

            UpdateReticlePosition(p);

            if (clicking) {
                if (Application.isPlaying) {
                    if (!warnedAboutPlayMode) {
                        warnedAboutPlayMode = true;
                        EditorUtility.DisplayDialog("Edit in play mode not allowed",
                            "Terrain cannot be edited by Digger while playing.\n\n" +
                            "Note for Digger Runtime: you *can* use DiggerMasterRuntime to edit the terrain while playing (so you can " +
                            "test your gameplay), but modifications made in play mode won't be persisted.", "Ok");
                    }
                } else {
                    warnedAboutPlayMode = false;
                    var parameters = new ModificationParameters
                    {
                        Position = p,
                        Brush = BrushType.Sphere,
                        Action = ActionType.Reset,
                        TextureIndex = 0,
                        Opacity = 1f,
                        Size = size,
                        OpacityIsTarget = false,
                        Callback = null
                    };

                    basicOperation.Params = parameters;
                    foreach (var diggerSystem in diggerSystems) {
                        diggerSystem.Modify(basicOperation);
                    }
                }
            }

            HandleUtility.Repaint();
        }

        private void UpdateReticlePosition(Vector3 position)
        {
            var reticle = Reticle.transform;
            reticle.position = position;
            reticle.localScale = 1.9f * size * Vector3.one;
            reticle.rotation = Quaternion.identity;
        }

        private void HandleShortcuts(UnityEditor.Editor editor)
        {
            if (!DiggerMasterEditor.shortcutsEnabled)
                return;

            var current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            switch (current.keyCode) {
                case KeyCode.KeypadMinus:
                    this.size -= 0.5f;
                    current.Use();
                    editor.Repaint();
                    break;
                case KeyCode.KeypadPlus:
                    this.size += 0.5f;
                    current.Use();
                    editor.Repaint();
                    break;
            }
        }
    }
}