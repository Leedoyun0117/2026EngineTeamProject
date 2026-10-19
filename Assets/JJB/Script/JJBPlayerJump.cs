using UnityEngine;
using UnityEngine.Serialization;

namespace JJB.Script
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JJBPlayerJump : MonoBehaviour
    {
        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpSpeed = 8f;
        [SerializeField, Min(0f)] private float maxJumpHoldTime = 0.3f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize;
        [SerializeField] private LayerMask groundLayer;

        [Header("Extra Gravity")]
        [SerializeField, Min(0f)] private float extraGravity = 30f;
        [SerializeField, Min(0f)] private float gravityDelay = 0.15f;

        private Rigidbody2D _rb;

        private bool _jumpRequested;
        private bool _canJump;

        private float _jumpHoldTime;
        private float _timeInAir;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void RequestJump()
        {
            _jumpRequested = true;
        }

        public void Tick(bool isJumpHeld)
        {
            IsGrounded = CheckGround();

            ProcessJumpRequest();
            ProcessJumpHold(isJumpHeld);
            UpdateAirTime();
            ApplyExtraGravity();
        }

        private void ProcessJumpRequest()
        {
            if (!_jumpRequested)
                return;

            _jumpRequested = false;

            if (!IsGrounded)
                return;

            _rb.linearVelocityY = jumpSpeed;

            _jumpHoldTime = 0f;
            _timeInAir = 0f;

            _canJump = true;
            IsGrounded = false;
        }

        private void ProcessJumpHold(bool isJumpHeld)
        {
            if (!_canJump)
                return;

            if (_rb.linearVelocityY <= 0f)
            {
                _canJump = false;
                return;
            }

            if (!isJumpHeld || _jumpHoldTime >= maxJumpHoldTime)
            {
                StopJump();
                return;
            }

            _jumpHoldTime += Time.fixedDeltaTime;

            if (_jumpHoldTime >= maxJumpHoldTime)
            {
                StopJump();
            }
        }

        private void StopJump()
        {
            if (_rb.linearVelocityY > 0f)
            {
                _rb.linearVelocityY = 0f;
            }

            _canJump = false;
        }

        private void UpdateAirTime()
        {
            if (IsGrounded)
            {
                _timeInAir = 0f;
                return;
            }

            _timeInAir += Time.fixedDeltaTime;
        }

        private void ApplyExtraGravity()
        {
            if (IsGrounded || _timeInAir <= gravityDelay)
                return;

            _rb.AddForceY(-extraGravity, ForceMode2D.Force);
        }

        private bool CheckGround()
        {
            if (groundCheck == null)
                return false;

            return Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            ) != null;
        }

        private void OnDisable()
        {
            _jumpRequested = false;
            _canJump = false;

            _jumpHoldTime = 0f;
            _timeInAir = 0f;

            IsGrounded = false;
        }

        private void OnDrawGizmos()
        {
            if (groundCheck == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            Gizmos.color = Color.wheat;
        }
    }
}