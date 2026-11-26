using LDY.TitlePlayer;
using LDY.TitlePlayer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // 창 입력 구현. 플레이어 입력은 기존 JJBInput 액션과 입력 잠금을 그대로 따르고, W/S와 마우스는 장치를 직접 읽는다.
    // 액션이 없으면 그 입력만 0/false가 되고 에러는 한 번만 남는다.
    public class PlayerWindowInput : IWindowInput {
        private readonly IButtonInput _interact;
        private readonly OptionalLookup<InputAction> _jump;
        private readonly IInputLock _lock;
        private readonly OptionalLookup<InputAction> _move;

        public PlayerWindowInput(PlayerInput playerInput, IInputLock inputLock, IButtonInput interact) {
            _lock = inputLock;
            _interact = interact;
            _move = PlayerInputActions.Lookup(playerInput, InputActionNames.Move, playerInput);
            _jump = PlayerInputActions.Lookup(playerInput, InputActionNames.Jump, playerInput);
        }

        public float Horizontal {
            get {
                if (_lock.IsLocked)
                    return 0f;

                var move = _move.Get();
                return move != null ? move.ReadValue<Vector2>().x : 0f;
            }
        }

        public bool InteractPressed => _interact.Pressed;

        public bool JumpPressed {
            get {
                if (_lock.IsLocked)
                    return false;

                var jump = _jump.Get();
                return jump != null && jump.WasPressedThisFrame();
            }
        }

        public int VerticalStep {
            get {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return 0;

                if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                    return 1;
                if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                    return -1;
                return 0;
            }
        }

        public float VerticalHeld {
            get {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return 0f;

                var value = 0f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    value += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    value -= 1f;
                return value;
            }
        }

        public float Scroll {
            get {
                if (Mouse.current == null)
                    return 0f;

                var y = Mouse.current.scroll.ReadValue().y;
                return y > 0f ? 1f : y < 0f ? -1f : 0f;
            }
        }

        public bool PointerPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        public bool PointerHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;
    }
}