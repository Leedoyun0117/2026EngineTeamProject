using JJB.Script;
using JJB.Script.Player;
using UnityEngine;

namespace LDY.Script
{
    [RequireComponent(typeof(JJBPlayer), typeof(JJBPlayerJump))]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputLock))]
    [DefaultExecutionOrder(-100)]
    public class JJBControlGate : MonoBehaviour, IPlayerControl, IJumpBlock
    {
        //컨트롤 관련 변수들
        private JJBPlayer _player;
        private JJBPlayerJump _jump;
        private Rigidbody2D _rb;
        private IInputLock _lock;
        private bool _suspended;
        private bool _jumpBlocked;

        private void Awake()
        {
            //필요한 컴포넌트 선언
            _player = GetComponent<JJBPlayer>();
            _jump = GetComponent<JJBPlayerJump>();
            _rb = GetComponent<Rigidbody2D>();
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        private void OnEnable()
        {
            _lock.LockChanged += HandleLockChanged; //잠금 상태가 바뀔 때 알림을 받겠다고 등록
            Apply(); //잠금과 정지 상태를 보고 JJBPlayer를 켤지 끌지 반영하는 함수
        }

        private void OnDisable()
        {
            _lock.LockChanged -= HandleLockChanged; // 사라졌을 때 조작이 불가능하게 구독해제 해줌
        }

        public void SetSuspended(bool suspended)
        {
            if (_suspended == suspended) //받은 suspend가 참이라면 리턴하고
                return;

            _suspended = suspended; //거짓이라면 참을 넣고 올려줌
            Apply();

            if (suspended) //참이라면 
                ClearPendingJump(); //ClearPendingJump 호출
        }

        private void HandleLockChanged(bool locked)
        {
            Apply(); 

            if (locked) //조작이 잠겨있다면
                _rb.linearVelocityX = 0f; //이동 불가
        }

        private void Apply()
        {
            if (_player == null) //플레이어가 없다면 리턴하고
                return;

            _player.enabled = !(_suspended || _lock.IsLocked); // 있다면 suspend가 참이거나 잠겨있다면 지금 움직이면 안 되기에 거짓이 나오게 됨
        }

        private void FixedUpdate()
        {
            if (_jumpBlocked)
                ClearPendingJump();
        }

        public void SetJumpBlocked(bool blocked)
        {
            _jumpBlocked = blocked;
            if (blocked)
            {
                _jump.enabled = false;
                return;
            }

            ClearPendingJump();
            _jump.enabled = true;
        }

        // JJBPlayerJump.OnDisable이 점프 요청 플래그를 초기화하는 점을 이용한다.
        private void ClearPendingJump()
        {
            if (_jump == null)
                return;

            bool wasEnabled = _jump.enabled;
            _jump.enabled = true;
            _jump.enabled = false;
            _jump.enabled = wasEnabled;
        }
    }
}
