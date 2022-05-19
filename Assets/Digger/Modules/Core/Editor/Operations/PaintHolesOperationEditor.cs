using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Digger.Modules.Core.Editor.Operations
{
    [OperationAttr("Paint Holes", 4)]
    public class PaintHolesOperationEditor : ABasicOperationEditor, IOperationEditor
    {
        private readonly BasicOperation basicOperation = new BasicOperation();


        public void OnInspectorGUI()
        {
            var diggerSystem = Object.FindObjectOfType<DiggerSystem>();
            if (!diggerSystem)
                return;

            BrushInspectorGUI();

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

        private IOperation<VoxelModificationJob> OperationAt(Vector3 position)
        {
            var parameters = new ModificationParameters
            {
                Position = position,
                Brush = brush,
                Action = ActionType.PaintHoles,
                TextureIndex = 0,
                Opacity = Event.current.control ? -1f : 1f,
                Size = size,
                StalagmiteUpsideDown = false,
                OpacityIsTarget = false,
                Callback = null
            };

            basicOperation.Params = parameters;
            return basicOperation;
        }
    }
}