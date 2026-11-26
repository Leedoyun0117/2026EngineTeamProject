using System;
using LDY.TitlePlayer;
using LDY.TitlePlayer.Core;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // JJBInput의 기존 Interact 액션을 PlayerInput에서 읽는다. JJB 코드는 OnInteract를 받지 않는다.
    // 액션이 없으면 상호작용만 꺼지고 에러는 한 번만 남는다.
    public class PlayerInteractInput : IButtonInput {
        private readonly OptionalLookup<InputAction> _action;
        private readonly IInputLock _lock;

        public PlayerInteractInput(PlayerInput playerInput, IInputLock inputLock) {
            _lock = inputLock;
            _action = PlayerInputActions.Lookup(playerInput, InputActionNames.Interact, playerInput);
        }

        // PlayerInput이 OnEnable에서 액션 에셋을 복제할 수 있어 첫 사용 시점에 찾는다.
        public bool Pressed {
            get {
                if (_lock.IsLocked)
                    return false;

                var action = _action.Get();
                return action != null && action.WasPressedThisFrame();
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