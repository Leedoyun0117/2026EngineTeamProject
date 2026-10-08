using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    public class AltKeyInput : IAltInput, IDisposable
    {
        private readonly InputAction _action = new InputAction("Alt", InputActionType.Button, "<Keyboard>/alt");
        private readonly IInputLock _lock;
        private bool _suppressed;

        public AltKeyInput(IInputLock inputLock)
        {
            _lock = inputLock;
        }

        public bool Pressed => Available && _action.WasPressedThisFrame();
        public bool Held => Available && _action.IsPressed();

        // 잠금 중이거나 포커스를 잃은 뒤 Alt를 완전히 뗄 때까지는 입력이 없는 것으로 본다.
        private bool Available
        {
            get
            {
                if (_lock.IsLocked || !Application.isFocused)
                    return false;

                if (_suppressed && !_action.IsPressed())
                    _suppressed = false;

                return !_suppressed;
            }
        }

        public void Enable()
        {
            _action.Enable();
            Application.focusChanged += HandleFocusChanged;
        }

        public void Disable()
        {
            Application.focusChanged -= HandleFocusChanged;
            _action.Disable();
        }

        public void Dispose()
        {
            Disable();
            _action.Dispose();
        }

        private void HandleFocusChanged(bool focused)
        {
            if (!focused)
                _suppressed = true;
        }
    }
}
