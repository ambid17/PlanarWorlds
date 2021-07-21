using System;
using UnityEngine;

namespace RTG
{
    public class MouseInputDevice : InputDeviceBase
    {
        private Vector3 _frameDelta;
        private Vector3 _mousePosInLastFrame;

        public override InputDeviceType DeviceType { get { return InputDeviceType.Mouse; } }

        public MouseInputDevice()
        {
            _frameDelta = Vector3.zero;
            _mousePosInLastFrame = RTInput.MousePosition;
        }

        public override Vector3 GetFrameDelta()
        {
            return _frameDelta;
        }

        public override Ray GetRay(Camera camera)
        {
            return camera.ScreenPointToRay(RTInput.MousePosition);
        }

        public override Vector3 GetPositionYAxisUp()
        {
            return RTInput.MousePosition;
        }

        public override bool HasPointer()
        {
            return RTInput.IsMousePresent;
        }

        public override bool IsButtonPressed(int buttonIndex)
        {
            return RTInput.IsMouseButtonPressed(buttonIndex);
        }

        public override bool WasButtonPressedInCurrentFrame(int buttonIndex)
        {
            return RTInput.WasMouseButtonPressedThisFrame(buttonIndex);
        }

        public override bool WasButtonReleasedInCurrentFrame(int buttonIndex)
        {
            return RTInput.WasMouseButtonReleasedThisFrame(buttonIndex);
        }

        public override bool WasMoved()
        {
            return RTInput.WasMouseMoved();
        }

        protected override void UpateFrameDeltas()
        {
            _frameDelta = RTInput.MousePosition - _mousePosInLastFrame;
            _mousePosInLastFrame = RTInput.MousePosition;
        }
    }
}
