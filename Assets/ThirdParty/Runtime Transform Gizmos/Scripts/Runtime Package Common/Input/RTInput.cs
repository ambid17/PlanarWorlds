using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RTG
{
    public static class RTInput
    {
        #if ENABLE_INPUT_SYSTEM
        private static Key[] _keyMap = new Key[319];        // 319 = largest key in KeyCode (ignoring Mouse and Joystick)

        static RTInput()
        {
            _keyMap[(int)KeyCode.None] = Key.None;
            _keyMap[(int)KeyCode.Space] = Key.Space;
            _keyMap[(int)KeyCode.Return] = Key.Enter;
            _keyMap[(int)KeyCode.Tab] = Key.Tab;
            _keyMap[(int)KeyCode.BackQuote] = Key.Backquote;
            _keyMap[(int)KeyCode.Quote] = Key.Quote;
            _keyMap[(int)KeyCode.Semicolon] = Key.Semicolon;
            _keyMap[(int)KeyCode.Comma] = Key.Comma;
            _keyMap[(int)KeyCode.Period] = Key.Period;
            _keyMap[(int)KeyCode.Slash] = Key.Slash;
            _keyMap[(int)KeyCode.Backslash] = Key.Backslash;
            _keyMap[(int)KeyCode.LeftBracket] = Key.LeftBracket;
            _keyMap[(int)KeyCode.RightBracket] = Key.RightBracket;
            _keyMap[(int)KeyCode.Equals] = Key.Equals;

            for (int c = 0; c < 26; ++c) 
                _keyMap[(int)KeyCode.A + c] = (Key)((int)Key.A + c);

            _keyMap[(int)KeyCode.Alpha0] = Key.Digit0;
            for (int c = 0; c < 9; ++c)
                _keyMap[(int)KeyCode.Alpha1 + c] = (Key)((int)Key.Digit1 + c);

            _keyMap[(int)KeyCode.LeftShift] = Key.LeftShift;
            _keyMap[(int)KeyCode.RightShift] = Key.RightShift;
            _keyMap[(int)KeyCode.LeftAlt] = Key.LeftAlt;
            _keyMap[(int)KeyCode.RightAlt] = Key.RightAlt;
            _keyMap[(int)KeyCode.AltGr] = Key.AltGr;
            _keyMap[(int)KeyCode.LeftControl] = Key.LeftCtrl;
            _keyMap[(int)KeyCode.RightControl] = Key.RightCtrl;
            _keyMap[(int)KeyCode.LeftWindows] = Key.LeftWindows;
            _keyMap[(int)KeyCode.LeftApple] = Key.LeftApple;
            _keyMap[(int)KeyCode.LeftCommand] = Key.LeftCommand;
            _keyMap[(int)KeyCode.RightWindows] = Key.RightWindows;
            _keyMap[(int)KeyCode.RightApple] = Key.RightApple;
            _keyMap[(int)KeyCode.RightCommand] = Key.RightCommand;
            // Key.ContextMenu ??? 
            _keyMap[(int)KeyCode.Escape] = Key.Escape;
            _keyMap[(int)KeyCode.LeftArrow] = Key.LeftArrow;
            _keyMap[(int)KeyCode.RightArrow] = Key.RightArrow;
            _keyMap[(int)KeyCode.UpArrow] = Key.UpArrow;
            _keyMap[(int)KeyCode.DownArrow] = Key.DownArrow;
            _keyMap[(int)KeyCode.Backspace] = Key.Backspace;
            _keyMap[(int)KeyCode.PageDown] = Key.PageDown;
            _keyMap[(int)KeyCode.PageUp] = Key.PageUp;
            _keyMap[(int)KeyCode.Home] = Key.Home;
            _keyMap[(int)KeyCode.End] = Key.End;
            _keyMap[(int)KeyCode.Insert] = Key.Insert;
            _keyMap[(int)KeyCode.Delete] = Key.Delete;
            _keyMap[(int)KeyCode.CapsLock] = Key.CapsLock;
            _keyMap[(int)KeyCode.Numlock] = Key.NumLock;
            // Key.PrintScreen ??? 
            _keyMap[(int)KeyCode.ScrollLock] = Key.ScrollLock;
            _keyMap[(int)KeyCode.Pause] = Key.Pause;
            _keyMap[(int)KeyCode.KeypadEnter] = Key.NumpadEnter;
            _keyMap[(int)KeyCode.KeypadDivide] = Key.NumpadDivide;
            _keyMap[(int)KeyCode.KeypadMultiply] = Key.NumpadMultiply;
            _keyMap[(int)KeyCode.KeypadPlus] = Key.NumpadPlus;
            _keyMap[(int)KeyCode.KeypadMinus] = Key.NumpadMinus;
            _keyMap[(int)KeyCode.KeypadPeriod] = Key.NumpadPeriod;
            _keyMap[(int)KeyCode.KeypadEquals] = Key.NumpadEquals;

            for (int c = 0; c < 10; ++c) 
                _keyMap[(int)KeyCode.Keypad0 + c] = (Key)((int)Key.Numpad0 + c);

            for (int c = 0; c < 15; ++c)
                _keyMap[(int)KeyCode.F1 + c] = (Key)((int)Key.F1 + c);

            // OEM1 ?
            // OEM2 ?
            // OEM3 ?
            // OEM4 ?
            // OEM5 ?
            // IMESelected ?
        }
        #endif

        public static Vector3 MousePosition 
        { 
            get 
            {
                #if ENABLE_INPUT_SYSTEM
                return Mouse.current.position.ReadValue();
                #else
                return Input.mousePosition;             
                #endif
            }
        }

        public static bool IsMousePresent
        {
            get
            {
                #if ENABLE_INPUT_SYSTEM
                return Mouse.current != null;
                #else
                return Input.mousePresent;
                #endif
            }
        }

        public static int TouchCount
        {
            get
            {
                #if ENABLE_INPUT_SYSTEM
                return Touchscreen.current.touches.Count;
                #else
                return Input.touchCount;
                #endif
            }
        }

        public static bool WasLeftMouseButtonPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool WasRightMouseButtonPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        public static bool WasMiddleMouseButtonPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.middleButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(2);
#endif
        }

        public static bool WasMouseButtonPressedThisFrame(int mouseButton)
        {
#if ENABLE_INPUT_SYSTEM
            if (mouseButton == 0) return Mouse.current.leftButton.wasPressedThisFrame;
            else if (mouseButton == 1) return Mouse.current.rightButton.wasPressedThisFrame;
            else if (mouseButton == 2) return Mouse.current.middleButton.wasPressedThisFrame;
            return false;
#else
            return Input.GetMouseButtonDown(mouseButton);
#endif
        }

        public static bool WasMouseButtonReleasedThisFrame(int mouseButton)
        {
#if ENABLE_INPUT_SYSTEM
            if (mouseButton == 0) return Mouse.current.leftButton.wasReleasedThisFrame;
            else if (mouseButton == 1) return Mouse.current.rightButton.wasReleasedThisFrame;
            else if (mouseButton == 2) return Mouse.current.middleButton.wasReleasedThisFrame;
            return false;
#else
            return Input.GetMouseButtonUp(mouseButton);
#endif
        }

        public static bool IsLeftMouseButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        public static bool IsRightMouseButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.isPressed;
#else
            return Input.GetMouseButton(1);
#endif
        }

        public static bool IsMiddleMouseButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.middleButton.isPressed;
#else
            return Input.GetMouseButton(2);
#endif
        }

        public static bool IsMouseButtonPressed(int mouseButton)
        {
#if ENABLE_INPUT_SYSTEM
            if (mouseButton == 0) return Mouse.current.leftButton.isPressed;
            else if (mouseButton == 1) return Mouse.current.rightButton.isPressed;
            else if (mouseButton == 2) return Mouse.current.middleButton.isPressed;
            return false;
#else
            return Input.GetMouseButton(mouseButton);
#endif
        }

        public static bool WasMouseMoved()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.delta.ReadValue().sqrMagnitude != 0.0f;
#else
            return Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
#endif
        }

        public static float MouseAxisX()
        {
#if ENABLE_INPUT_SYSTEM
            return Input.GetAxis("Mouse X");
#else
            return Input.GetAxis("Mouse X");
#endif
        }

        public static float MouseAxisY()
        {
#if ENABLE_INPUT_SYSTEM
            return Input.GetAxis("Mouse Y");
#else
            return Input.GetAxis("Mouse Y");
#endif
        }

        public static float MouseScroll()
        {
#if ENABLE_INPUT_SYSTEM
            return Input.GetAxis("Mouse ScrollWheel");
#else
            return Input.GetAxis("Mouse ScrollWheel");
#endif
        }

        public static bool WasKeyPressedThisFrame(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current[_keyMap[(int)keyCode]].wasPressedThisFrame;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

        public static bool IsKeyPressed(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current[_keyMap[(int)keyCode]].isPressed;
#else
            return Input.GetKey(keyCode);
#endif
        }

        public static Vector2 TouchDelta(int touchIndex)
        {
#if ENABLE_INPUT_SYSTEM
            return Touchscreen.current.touches[touchIndex].delta.ReadValue();
#else
            return Input.GetTouch(touchIndex).deltaPosition;
            #endif
        }

        public static Vector2 TouchPosition(int touchIndex)
        {
#if ENABLE_INPUT_SYSTEM
            return Touchscreen.current.touches[touchIndex].position.ReadValue();
#else
            return Input.GetTouch(touchIndex).position;
#endif
        }

        public static bool TouchBegan(int touchIndex)
        {
#if ENABLE_INPUT_SYSTEM
            var phase = Touchscreen.current.touches[touchIndex].phase.ReadValue();
            return phase == UnityEngine.InputSystem.TouchPhase.Began;
#else
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Began;
#endif
        }

        public static bool TouchEndedOrCanceled(int touchIndex)
        {
#if ENABLE_INPUT_SYSTEM
            var phase = Touchscreen.current.touches[touchIndex].phase.ReadValue();
            return phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled;
#else
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Ended || touch.phase == UnityEngine.TouchPhase.Canceled;
#endif
        }

        public static bool TouchMoved(int touchIndex)
        {
#if ENABLE_INPUT_SYSTEM
            var phase = Touchscreen.current.touches[touchIndex].phase.ReadValue();
            return phase == UnityEngine.InputSystem.TouchPhase.Moved;
#else
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Moved;
#endif
        }
    }
}