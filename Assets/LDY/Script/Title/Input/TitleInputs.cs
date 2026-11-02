using System;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // JJBInput의 기존 Interact 액션을 PlayerInput에서 읽는다. JJB 코드는 OnInteract를 받지 않는다.
    public class PlayerInteractInput : IButtonInput {
        private readonly IInputLock _lock;
        private readonly PlayerInput _playerInput;
        private InputAction _action;

        public PlayerInteractInput(PlayerInput playerInput, IInputLock inputLock) {
            _playerInput = playerInput;
            _lock = inputLock;
        }

        // PlayerInput이 OnEnable에서 액션 에셋을 복제할 수 있어 첫 사용 시점에 찾는다.
        public bool Pressed {
            get {
                if (_lock.IsLocked)
                    return false;

                _action ??= _playerInput.actions.FindAction("Interact", true);
                return _action.WasPressedThisFrame();
            }
        }
    }

    public class MouseClickInput : IButtonInput {
        public bool Pressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public class KeyboardCancelInput : IButtonInput, IDisposable {
        private readonly InputAction _action = new("Cancel", InputActionType.Button, "<Keyboard>/escape");

        public bool Pressed => _action.WasPressedThisFrame();

        public void Dispose() {
            _action.Dispose();
        }

        public void Enable() {
            _action.Enable();
        }

        public void Disable() {
            _action.Disable();
        }
    }
}