using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Digger.Modules.Core.Editor.Operations
{
    [OperationAttr("Dig", 1)]
    public class DigOperationEditor : ABasicOperationEditor, IScriptableOperationEditor
    {
        private readonly BasicOperation basicOperation = new BasicOperation();


        public void OnInspectorGUI()
        {
            var diggerSystem = Object.FindObjectOfType<DiggerSystem>();
            if (!diggerSystem)
                return;

            BrushInspectorGUI();

            opacity = EditorGUILayout.Slider(new GUIContent("Opacity", DiggerMasterEditor.shortcutsEnabled ? "Shortcut: keypad / or *" : ""), opacity, 0f, 1f);
            depth = EditorGUILayout.Slider("Depth", depth, -size.y, size.y);

            textureIndex = DiggerMasterEditor.TextureSelector(textureIndex, diggerSystem);

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

        protected override void PerformModification(Vector3 p)
        {
            var op = OperationAt(p);
            foreach (var diggerSystem in diggerSystems) {
                diggerSystem.Modify(op);
            }
        }

        public IOperation<VoxelModificationJob> OperationAt(Vector3 position)
        {
            var parameters = new ModificationParameters
            {
                Position = position,
                Brush = brush,
                Action = ActionType.Dig,
                TextureIndex = textureIndex,
                Opacity = opacity,
                Size = size,
                StalagmiteUpsideDown = upsideDown,
                OpacityIsTarget = false,
                Callback = null
            };

            basicOperation.Params = parameters;
            return basicOperation;
        }
    }
}