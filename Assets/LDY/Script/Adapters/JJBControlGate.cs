using JJB.Script;
using UnityEngine;

namespace LDY.Script
{
    // JJB 플레이어 코드와 닿는 유일한 지점. 입력 잠금(컷씬/복귀)과 Alt 정지를 JJBPlayer on/off로 반영한다.
    [RequireComponent(typeof(JJBPlayer), typeof(JJBPlayerJump))]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputLock))]
    public class JJBControlGate : MonoBehaviour, IPlayerControl, IJumpBlock
    {
        private JJBPlayer _player;
        private JJBPlayerJump _jump;
        private Rigidbody2D _rb;
        private IInputLock _lock;
        private bool _suspended;

        private void Awake()
        {
            _player = GetComponent<JJBPlayer>();
            _jump = GetComponent<JJBPlayerJump>();
            _rb = GetComponent<Rigidbody2D>();
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        private void OnEnable()
        {
            _lock.LockChanged += HandleLockChanged;
            Apply();
        }

        private void OnDisable()
        {
            _lock.LockChanged -= HandleLockChanged;
        }

        public void SetSuspended(bool suspended)
        {
            if (_suspended == suspended)
                return;

            _suspended = suspended;
            Apply();

            if (suspended)
                ClearPendingJump();
        }

        private void HandleLockChanged(bool locked)
        {
            Apply();

            if (locked)
                _rb.linearVelocityX = 0f;
        }

        private void Apply()
        {
            if (_player == null)
                return;

            _player.enabled = !(_suspended || _lock.IsLocked);
        }

        // JJBPlayerJump를 꺼 두면 FixedUpdate가 돌지 않아 점프 요청이 처리되지 않는다.
        // 꺼진 동안에도 RequestJump는 플래그를 세우므로, 풀 때는 껐다 켜서 남은 요청을 지운다.
        public void SetJumpBlocked(bool blocked)
        {
            if (blocked)
            {
                _jump.enabled = false;
                return;
            }

            _jump.enabled = true;
            _jump.enabled = false;
            _jump.enabled = true;
        }

        // JJBPlayerJump.OnDisable이 점프 요청 플래그를 초기화하는 점을 이용한다.
        private void ClearPendingJump()
        {
            if (_jump == null || !_jump.enabled)
                return;

            _jump.enabled = false;
            _jump.enabled = true;
        }
    }
}
