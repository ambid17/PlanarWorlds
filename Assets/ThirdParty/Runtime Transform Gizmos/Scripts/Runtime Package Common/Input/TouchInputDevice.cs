using System;
using UnityEngine;

namespace RTG
{
    public class TouchInputDevice : InputDeviceBase
    {
        private int _maxNumberOfTouches;

        public int MaxNumberOfTouches { get { return _maxNumberOfTouches; } }
        public int TouchCount { get { return RTInput.TouchCount; } }
        public override InputDeviceType DeviceType { get { return InputDeviceType.Touch; } }

        public TouchInputDevice(int maxNumberOfTouches)
        {
            _maxNumberOfTouches = Mathf.Max(1, maxNumberOfTouches);
        }

        public override Vector3 GetFrameDelta()
        {
            if (TouchCount != 0) return RTInput.TouchDelta(0);
            return Vector3.zero;
        }

        public override Ray GetRay(Camera camera)
        {
            Ray ray = new Ray(Vector3.zero, Vector3.zero);
            if (TouchCount != 0)
            {
                ray = camera.ScreenPointToRay(RTInput.TouchPosition(0));
            }
            return ray;
        }

        public override Vector3 GetPositionYAxisUp()
        {
            if (TouchCount != 0) return RTInput.TouchPosition(0);
            return Vector3.zero;
        }

        public override bool HasPointer()
        {
            return TouchCount != 0;
        }

        public override bool IsButtonPressed(int buttonIndex)
        {
            int touchCount = TouchCount;
            if (buttonIndex >= touchCount || touchCount > MaxNumberOfTouches) return false;
            return true;
        }

        public override bool WasButtonPressedInCurrentFrame(int buttonIndex)
        {
            int touchCount = TouchCount;
            if (buttonIndex >= touchCount || touchCount > MaxNumberOfTouches) return false;
            return RTInput.TouchBegan(buttonIndex);
        }

        public override bool WasButtonReleasedInCurrentFrame(int buttonIndex)
        {
            int touchCount = TouchCount;
            if (buttonIndex >= touchCount || touchCount > MaxNumberOfTouches) return false;

            return RTInput.TouchEndedOrCanceled(buttonIndex);
        }

        public override bool WasMoved()
        {
            int touchCount = TouchCount;
            if (touchCount != 0)
            {
                for (int touchIndex = 0; touchIndex < touchCount; ++touchIndex)
                {
                    if (touchIndex >= MaxNumberOfTouches) return false;

                    if (RTInput.TouchMoved(touchIndex)) return true;
                }
            }
            return false;
        }

        protected override void UpateFrameDeltas()
        {
            // No implementation needed since for a touch device the delta
            // information is stored inside the 'Touch' struct.
        }
    }
}
