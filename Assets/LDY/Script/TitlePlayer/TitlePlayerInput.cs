using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.TitlePlayer {
    // 프로젝트 방식대로 PlayerInput의 Move/Jump 액션을 읽는다. 점프 누름은 다음 FixedUpdate가 가져갈 때까지 보관한다.
    [RequireComponent(typeof(PlayerInput))]
    public sealed class TitlePlayerInput : MonoBehaviour {
        private const string MoveAction = "Move";
        private const string JumpAction = "Jump";

        private InputAction _move;
        private InputAction _jump;
        private bool _jumpPressed;

        public float MoveX { get; private set; }
        public bool JumpHeld { get; private set; }

        private void Awake() {
            var actions = GetComponent<PlayerInput>().actions;
            _move = actions.FindAction(MoveAction, true);
            _jump = actions.FindAction(JumpAction, true);
        }

        private void Update() {
            MoveX = _move.ReadValue<Vector2>().x;
            JumpHeld = _jump.IsPressed();
            if (_jump.WasPressedThisFrame()) _jumpPressed = true;
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
