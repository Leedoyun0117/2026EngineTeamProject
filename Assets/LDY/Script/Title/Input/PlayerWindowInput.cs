using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 창 입력 구현. 플레이어 입력은 기존 JJBInput 액션과 입력 잠금을 그대로 따르고, W/S와 마우스는 장치를 직접 읽는다.
    public class PlayerWindowInput : IWindowInput
    {
        private readonly PlayerInput _playerInput;
        private readonly IInputLock _lock;
        private readonly IButtonInput _interact;
        private InputAction _move;
        private InputAction _jump;

        public PlayerWindowInput(PlayerInput playerInput, IInputLock inputLock, IButtonInput interact)
        {
            _playerInput = playerInput;
            _lock = inputLock;
            _interact = interact;
        }

        public float Horizontal
        {
            get
            {
                if (_lock.IsLocked)
                    return 0f;

                _move ??= _playerInput.actions.FindAction("Move", true);
                return _move.ReadValue<Vector2>().x;
            }
        }

        public bool InteractPressed => _interact.Pressed;

        public bool JumpPressed
        {
            get
            {
                if (_lock.IsLocked)
                    return false;

                _jump ??= _playerInput.actions.FindAction("Jump", true);
                return _jump.WasPressedThisFrame();
            }
        }

        public int VerticalStep
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                    return 0;

                if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                    return 1;
                if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                    return -1;
                return 0;
            }
        }

        public float VerticalHeld
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                    return 0f;

                float value = 0f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    value += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    value -= 1f;
                return value;
            }
        }

        public float Scroll
        {
            get
            {
                if (Mouse.current == null)
                    return 0f;

                float y = Mouse.current.scroll.ReadValue().y;
                return y > 0f ? 1f : y < 0f ? -1f : 0f;
            }
        }

        public bool PointerPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        public bool PointerHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;
    }
}
