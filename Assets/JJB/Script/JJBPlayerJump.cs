using UnityEngine;
using UnityEngine.Serialization;

namespace JJB.Script
{
    public class JJBPlayerJump : MonoBehaviour
    {
        [Header("Jump")]
        [SerializeField] private float jumpSpeed = 12f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize;
        [SerializeField] private LayerMask groundLayer;
        
        [Header("Extra Settings")]
        [SerializeField] private float extraGravity = 30f;
        [SerializeField] private float gravityDelay = 0.15f;
        private float _timeInAir;
        
        private Rigidbody2D _rb;
        private bool _jumpRequested;
        
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void RequestJump()
        {
            _jumpRequested = true;
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            
            ProcessJumpRequest();
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

            _timeInAir = 0f;

            _rb.linearVelocityY = 0f;
            _rb.AddForceY(jumpSpeed, ForceMode2D.Impulse);

            IsGrounded = false;
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

        private bool CheckGrounded()
        {
            if (groundCheck == null)
            {
                return false;
            }
            
            return Physics2D.OverlapBox(
                groundCheck.position, 
                groundCheckSize, 
                0f, 
                groundLayer) != null;
        }

        private void OnDisable()
        {
            _jumpRequested = false;
            _timeInAir = 0f;
            IsGrounded = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            Gizmos.color = Color.wheat;
        }
    }
}