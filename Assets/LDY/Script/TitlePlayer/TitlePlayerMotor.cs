using LDY.TitlePlayer.Core;
using UnityEngine;

namespace LDY.TitlePlayer {
    // Rigidbody2D 이동·점프와 접지 판정만 한다. 규칙 계산은 MotorRules에 맡긴다.
    // 잠금 어댑터(OnEnable에서 SetLocked 호출)보다 먼저 Awake가 돌아야 해서 순서를 앞당긴다.
    [DefaultExecutionOrder(-150)]
    [RequireComponent(typeof(Rigidbody2D), typeof(TitlePlayerInput))]
    public sealed class TitlePlayerMotor : MonoBehaviour, IMovementLock, IMotorReadout {
        [SerializeField] private TitlePlayerTuning tuning;
        [SerializeField] private Transform groundCheck;

        private Rigidbody2D _rb;
        private TitlePlayerInput _input;
        private MotorState _state;
        private bool _locked;
        private bool _suspended;
        private bool _jumpBlocked;

        public Vector2 Velocity => _rb.linearVelocity;
        public bool IsGrounded { get; private set; }

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<TitlePlayerInput>();
            if (tuning == null) {
                Debug.LogError("[TitlePlayerMotor] TitlePlayerTuning이 비었다", this);
                enabled = false;
            }
        }

        private void FixedUpdate() {
            IsGrounded = CheckGround();
            var frame = new MotorFrame(_input.MoveX, _input.ConsumeJump(), _input.JumpHeld, IsGrounded,
                _rb.linearVelocityY, _locked, _suspended, _jumpBlocked, Time.fixedDeltaTime);
            var result = MotorRules.Step(tuning.Motor, _state, frame);
            _state = result.State;
            if (result.VelocityX.HasValue) _rb.linearVelocityX = result.VelocityX.Value;
            if (result.VelocityY.HasValue) _rb.linearVelocityY = result.VelocityY.Value;
            if (result.ForceY != 0f) _rb.AddForceY(result.ForceY);
        }

        public void SetLocked(bool locked) {
            if (_locked == locked) return;
            _locked = locked;
            if (!locked) return;
            _rb.linearVelocityX = 0f;
            ResetJump();
        }

        public void SetSuspended(bool suspended) {
            if (_suspended == suspended) return;
            _suspended = suspended;
            if (suspended) ResetJump();
        }

        public void SetJumpBlocked(bool blocked) {
            _jumpBlocked = blocked;
            _input.ConsumeJump();
        }

        private void ResetJump() {
            _input.Clear();
            _state = default;
        }

        private bool CheckGround() {
            if (groundCheck == null) return false;
            return Physics2D.OverlapBox(groundCheck.position, tuning.groundCheckSize, 0f, tuning.groundLayer) != null;
        }

        private void OnDrawGizmosSelected() {
            if (groundCheck == null || tuning == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, tuning.groundCheckSize);
        }
    }
}
