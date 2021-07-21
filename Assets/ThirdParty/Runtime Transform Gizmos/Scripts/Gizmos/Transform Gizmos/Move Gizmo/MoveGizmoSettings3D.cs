﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTG
{
    [Serializable]
    public class MoveGizmoSettings3D : Settings
    {
        [SerializeField]
        private GizmoObjectVertexSnapSettings _vertexSnapSettings = new GizmoObjectVertexSnapSettings();
        [SerializeField]
        private GizmoLineSlider3DSettings[] _sglSliderSettings = new GizmoLineSlider3DSettings[6];
        [SerializeField]
        private GizmoPlaneSlider3DSettings[] _dblSliderSettings = new GizmoPlaneSlider3DSettings[3];

        public GizmoObjectVertexSnapSettings VertexSnapSettings { get { return _vertexSnapSettings; } }
        public float LineSliderHoverEps { get { return _sglSliderSettings[0].LineHoverEps; } }
        public float BoxSliderHoverEps { get { return _sglSliderSettings[0].BoxHoverEps; } }
        public float CylinderSliderHoverEps { get { return _sglSliderSettings[0].CylinderHoverEps; } }
        public float XSnapStep { get { return GetSglSliderSettings(0, AxisSign.Positive).OffsetSnapStep; } }
        public float YSnapStep { get { return GetSglSliderSettings(1, AxisSign.Positive).OffsetSnapStep; } }
        public float ZSnapStep { get { return GetSglSliderSettings(2, AxisSign.Positive).OffsetSnapStep; } }
        public float DragSensitivity { get { return _sglSliderSettings[0].OffsetSensitivity; } }

        public MoveGizmoSettings3D()
        {
            for (int axisIndex = 0; axisIndex < _sglSliderSettings.Length; ++axisIndex)
            {
                _sglSliderSettings[axisIndex] = new GizmoLineSlider3DSettings();
            }

            for (int axisIndex = 0; axisIndex < _dblSliderSettings.Length; ++axisIndex)
            {
                _dblSliderSettings[axisIndex] = new GizmoPlaneSlider3DSettings();
                _dblSliderSettings[axisIndex].AreaHoverEps = 0.0f;
                _dblSliderSettings[axisIndex].BorderLineHoverEps = 0.0f;
                _dblSliderSettings[axisIndex].BorderBoxHoverEps = 0.0f;
            }
        }

        public void SetLineSliderHoverEps(float eps)
        {
            foreach (var settings in _sglSliderSettings)
                settings.LineHoverEps = eps;
        }

        public void SetBoxSliderHoverEps(float eps)
        {
            foreach (var settings in _sglSliderSettings)
            {
                settings.BoxHoverEps = eps;
            }
        }

        public void SetCylinderSliderHoverEps(float eps)
        {
            foreach (var settings in _sglSliderSettings)
            {
                settings.CylinderHoverEps = eps;
            }
        }

        public void SetXSnapStep(float snapStep)
        {
            GetSglSliderSettings(0, AxisSign.Positive).OffsetSnapStep = snapStep;
            GetSglSliderSettings(0, AxisSign.Negative).OffsetSnapStep = snapStep;
            GetDblSliderSettings(PlaneId.XY).OffsetSnapStepRight = snapStep;
            GetDblSliderSettings(PlaneId.ZX).OffsetSnapStepUp = snapStep;
        }

        public void SetYSnapStep(float snapStep)
        {
            GetSglSliderSettings(1, AxisSign.Positive).OffsetSnapStep = snapStep;
            GetSglSliderSettings(1, AxisSign.Negative).OffsetSnapStep = snapStep;
            GetDblSliderSettings(PlaneId.XY).OffsetSnapStepUp = snapStep;
            GetDblSliderSettings(PlaneId.YZ).OffsetSnapStepRight = snapStep;
        }

        public void SetZSnapStep(float snapStep)
        {
            GetSglSliderSettings(2, AxisSign.Positive).OffsetSnapStep = snapStep;
            GetSglSliderSettings(2, AxisSign.Negative).OffsetSnapStep = snapStep;
            GetDblSliderSettings(PlaneId.YZ).OffsetSnapStepUp = snapStep;
            GetDblSliderSettings(PlaneId.ZX).OffsetSnapStepRight = snapStep;
        }

        public void SetDragSensitivity(float sensitivity)
        {
            foreach (var settings in _sglSliderSettings)
                settings.OffsetSensitivity = sensitivity;

            foreach (var settings in _dblSliderSettings)
                settings.OffsetSensitivity = sensitivity;
        }

        public void ConnectSliderSettings(GizmoLineSlider3D slider, int axisIndex, AxisSign axisSign)
        {
            slider.SharedSettings = GetSglSliderSettings(axisIndex, axisSign);
        }

        public void ConnectDblSliderSettings(GizmoPlaneSlider3D dblSlider, PlaneId planeId)
        {
            dblSlider.SharedSettings = GetDblSliderSettings(planeId);
        }

        private GizmoLineSlider3DSettings GetSglSliderSettings(int axisIndex, AxisSign axisSign)
        {
            if (axisSign == AxisSign.Positive) return _sglSliderSettings[axisIndex];
            else return _sglSliderSettings[3 + axisIndex];
        }

        private GizmoPlaneSlider3DSettings GetDblSliderSettings(PlaneId planeId)
        {
            return _dblSliderSettings[(int)planeId];
        }

        #if UNITY_EDITOR
        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {
            float newFloat;

            EditorGUILayoutEx.SectionHeader("Hover epsilon");
            var content = new GUIContent();
            content.text = "Line slider";
            content.tooltip = "Controls the precision used when hovering line sliders.";          
            newFloat = EditorGUILayout.FloatField(content, LineSliderHoverEps);
            if (newFloat != LineSliderHoverEps)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetLineSliderHoverEps(newFloat);
            }

            content.text = "Box slider";
            content.tooltip = "Controls the precision used when hovering box sliders.";
            newFloat = EditorGUILayout.FloatField(content, BoxSliderHoverEps);
            if (newFloat != BoxSliderHoverEps)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetBoxSliderHoverEps(newFloat);
            }

            content.text = "Cylinder slider";
            content.tooltip = "Controls the precision used when hovering cylinder sliders.";
            newFloat = EditorGUILayout.FloatField(content, CylinderSliderHoverEps);
            if (newFloat != CylinderSliderHoverEps)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetCylinderSliderHoverEps(newFloat);
            }

            EditorGUILayout.Separator();
            EditorGUILayoutEx.SectionHeader("Snapping");
            content.text = "X";
            content.tooltip = "Allows you to specify the snap step for the X axis.";
            newFloat = EditorGUILayout.FloatField(content, XSnapStep);
            if (newFloat != XSnapStep)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetXSnapStep(newFloat);
            }

            content.text = "Y";
            content.tooltip = "Allows you to specify the snap step for the Y axis.";
            newFloat = EditorGUILayout.FloatField(content, YSnapStep);
            if (newFloat != YSnapStep)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetYSnapStep(newFloat);
            }

            content.text = "Z";
            content.tooltip = "Allows you to specify the snap step for the Z axis.";
            newFloat = EditorGUILayout.FloatField(content, ZSnapStep);
            if (newFloat != ZSnapStep)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetZSnapStep(newFloat);
            }

            EditorGUILayout.Separator();
            EditorGUILayoutEx.SectionHeader("Drag sensitivity");
            content.text = "Sensitivity";
            content.tooltip = "This value allows you to scale the slider drag speed.";
            newFloat = EditorGUILayout.FloatField(content, DragSensitivity);
            if (newFloat != DragSensitivity)
            {
                EditorUndoEx.Record(undoRecordObject);
                SetDragSensitivity(newFloat);
            }

            EditorGUILayout.Separator();
            EditorGUILayoutEx.SectionHeader("Vertex snapping");
            _vertexSnapSettings.RenderEditorGUI(undoRecordObject);
        }
        #endif
    }
}
