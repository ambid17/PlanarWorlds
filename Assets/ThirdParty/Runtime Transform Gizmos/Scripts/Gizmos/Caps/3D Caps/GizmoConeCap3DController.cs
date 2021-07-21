﻿using UnityEngine;

namespace RTG
{
    public class GizmoConeCap3DController : GizmoCap3DController
    {
        public GizmoConeCap3DController(GizmoCap3DControllerData controllerData)
            :base(controllerData)
        {
        }

        public override void UpdateHandles()
        {
            _data.CapHandle.Set3DShapeVisible(_data.PyramidIndex, false);
            _data.CapHandle.Set3DShapeVisible(_data.TrPrismIndex, false);
            _data.CapHandle.Set3DShapeVisible(_data.BoxIndex, false);
            _data.CapHandle.Set3DShapeVisible(_data.SphereIndex, false);
            _data.CapHandle.Set3DShapeVisible(_data.ConeIndex, _data.Cap.IsVisible);
        }

        public override void UpdateTransforms(float zoomFactor)
        {
            var cap = _data.Cap;

            _data.Cone.BaseCenter = cap.Position;
            _data.Cone.Rotation = cap.Rotation;
            _data.Cone.BaseRadius = cap.GetRealConeRadius(zoomFactor);
            _data.Cone.Height = cap.GetRealConeHeight(zoomFactor);
        }

        public override void CapSlider3D(Vector3 sliderDirection, Vector3 sliderEndPt, float zoomFactor)
        {
            _data.Cap.AlignTransformAxis(1, AxisSign.Positive, sliderDirection);
            _data.Cap.Position = sliderEndPt;
        }

        public override void CapSlider3DInvert(Vector3 sliderDirection, Vector3 sliderEndPt, float zoomFactor)
        {
            _data.Cap.AlignTransformAxis(1, AxisSign.Positive, -sliderDirection);
            _data.Cap.Position = sliderEndPt + sliderDirection * GetSliderAlignedRealLength(zoomFactor);
        }

        public override float GetSliderAlignedRealLength(float zoomFactor)
        {
            return _data.Cap.GetRealConeHeight(zoomFactor);
        }
    }
}
