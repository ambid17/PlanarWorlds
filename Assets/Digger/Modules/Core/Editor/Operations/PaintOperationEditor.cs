using System;
using System.Linq;
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Digger.Modules.Core.Editor.Operations
{
    [OperationAttr("Paint", 3)]
    public class PaintOperationEditor : ABasicOperationEditor, IOperationEditor
    {
        private readonly BasicOperation basicOperation = new BasicOperation();

        private MicroSplatPaintType paintType {
            get => (MicroSplatPaintType)EditorPrefs.GetInt("diggerMaster_microSplatPaintType",
                (int)MicroSplatPaintType.Texture);
            set => EditorPrefs.SetInt("diggerMaster_microSplatPaintType", (int)value);
        }

        private TerrainMaterialType MaterialType {
            get { return diggerSystems.Select(digger => digger.MaterialType).FirstOrDefault(); }
        }

        public void OnInspectorGUI()
        {
            var diggerSystem = Object.FindObjectOfType<DiggerSystem>();
            if (!diggerSystem)
                return;

            if (MaterialType == TerrainMaterialType.MicroSplat) {
                paintType = (MicroSplatPaintType)EditorGUILayout.EnumPopup("Type", paintType);
                opacityIsTarget = EditorGUILayout.Toggle("Opacity is target", opacityIsTarget);
            } else {
                paintType = MicroSplatPaintType.Texture;
                opacityIsTarget = false;
            }

            BrushInspectorGUI();

            opacity = EditorGUILayout.Slider(new GUIContent("Opacity", DiggerMasterEditor.shortcutsEnabled ? "Shortcut: keypad / or *" : ""), opacity, 0f, 1f);
            depth = EditorGUILayout.Slider("Depth", depth, -size.y, size.y);

            if (paintType == MicroSplatPaintType.Texture) {
                textureIndex = DiggerMasterEditor.TextureSelector(textureIndex, diggerSystem);
            }

            if (!opacityIsTarget) {
                EditorGUILayout.HelpBox(
                    "Hold Ctrl to remove the texture instead of adding it.",
                    MessageType.Info);
            }

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
                Action = ActionType.Paint,
                TextureIndex = GetFixedTextureIndex(),
                Opacity = Event.current.control ? -opacity : opacity,
                Size = size,
                StalagmiteUpsideDown = upsideDown,
                OpacityIsTarget = opacityIsTarget,
                Callback = null
            };

            basicOperation.Params = parameters;
            return basicOperation;
        }

        private int GetFixedTextureIndex()
        {
            if (paintType == MicroSplatPaintType.Wetness) {
                return 28;
            } else if (paintType == MicroSplatPaintType.Puddles) {
                return 29;
            } else if (paintType == MicroSplatPaintType.Stream) {
                return 30;
            } else if (paintType == MicroSplatPaintType.Lava) {
                return 31;
            } else {
                return textureIndex;
            }
        }
    }
}