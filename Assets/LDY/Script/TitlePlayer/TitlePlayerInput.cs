using LDY.TitlePlayer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.TitlePlayer {
    // 프로젝트 방식대로 PlayerInput의 Move/Jump 액션을 읽는다. 점프 누름은 다음 FixedUpdate가 가져갈 때까지 보관한다.
    // 액션이 없으면 그 입력만 0/false가 되고 에러는 한 번만 남는다(PlayerInputActions).
    [RequireComponent(typeof(PlayerInput))]
    public sealed class TitlePlayerInput : MonoBehaviour {
        private OptionalLookup<InputAction> _move;
        private OptionalLookup<InputAction> _jump;
        private bool _jumpPressed;

        public float MoveX { get; private set; }
        public bool JumpHeld { get; private set; }

        private void Awake() {
            var input = GetComponent<PlayerInput>();
            _move = PlayerInputActions.Lookup(input, InputActionNames.Move, this);
            _jump = PlayerInputActions.Lookup(input, InputActionNames.Jump, this);
        }

        private void Update() {
            var move = _move.Get();
            var jump = _jump.Get();
            MoveX = move != null ? move.ReadValue<Vector2>().x : 0f;
            JumpHeld = jump != null && jump.IsPressed();
            if (jump != null && jump.WasPressedThisFrame()) _jumpPressed = true;
        }

        private void OnDisable() => Clear();

        public bool ConsumeJump() {
            var pressed = _jumpPressed;
            _jumpPressed = false;
            return pressed;
        }

        public void Clear() {
            MoveX = 0f;
            JumpHeld = false;
            _jumpPressed = false;
        }
    }
}
