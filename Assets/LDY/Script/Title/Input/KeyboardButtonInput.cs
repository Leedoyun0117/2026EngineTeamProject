using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 지정한 키 하나를 LDY 쪽 InputAction으로 읽는다. JJBInput 액션은 건드리지 않는다.
    public class KeyboardButtonInput : IButtonInput, IDisposable
    {
        private readonly InputAction _action;

        public KeyboardButtonInput(string name, string binding)
        {
            _action = new InputAction(name, InputActionType.Button, binding);
        }

        public bool Pressed => _action.WasPressedThisFrame();

        public void Enable()
        {
            _action.Enable();
        }

        public void Disable()
        {
            _action.Disable();
        }

        public void Dispose()
        {
            _action.Dispose();
        }
    }
}
