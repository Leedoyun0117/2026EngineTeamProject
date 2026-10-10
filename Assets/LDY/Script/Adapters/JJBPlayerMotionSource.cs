using JJB.Script;
using UnityEngine;

namespace LDY.Script
{
    // PlayerRenderer가 읽는 움직임 정보를 JJB 플레이어에서 가져온다. 읽기만 하고 JJB 상태는 바꾸지 않는다.
    [RequireComponent(typeof(Rigidbody2D), typeof(JJBPlayerJump), typeof(PlayerInputLock))]
    public class JJBPlayerMotionSource : MonoBehaviour, IPlayerMotionSource
    {
        private const float GroundNormalY = 0.5f;

        [Tooltip("이 속도 이하의 가로 속도는 방향 변경으로 보지 않는다")]
        [SerializeField, Min(0f)] private float facingDeadZone = 0.05f;

        private Rigidbody2D _rb;
        private JJBPlayerJump _jump;
        private IInputLock _lock;
        private readonly ContactPoint2D[] _contacts = new ContactPoint2D[8];

        public Vector2 Velocity => _rb.linearVelocity;
        public bool IsInputLocked => _lock.IsLocked;

        public int FacingSign
        {
            get
            {
                float vx = _rb.linearVelocity.x;
                return Mathf.Abs(vx) <= facingDeadZone ? 0 : (vx > 0f ? 1 : -1);
            }
        }

        // JJBPlayerJump를 끄면(설정창에서 점프 차단) 땅 판정이 갱신되지 않고 false로 초기화된다.
        // 그때는 물리 접촉으로 대신 판정한다.
        public bool IsGrounded => _jump.enabled ? _jump.IsGrounded : IsTouchingGround();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _jump = GetComponent<JJBPlayerJump>();
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        private bool IsTouchingGround()
        {
            int count = _rb.GetContacts(_contacts);
            for (int i = 0; i < count; i++)
            {
                if (_contacts[i].normal.y > GroundNormalY)
                    return true;
            }

            return false;
        }
    }
}
