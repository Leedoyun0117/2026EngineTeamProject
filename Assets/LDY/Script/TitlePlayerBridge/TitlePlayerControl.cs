using LDY.TitlePlayer;
using UnityEngine;

namespace LDY.Script {
    // 기존 LDY 시스템(PlayerInputLock, Alt/SuspendRequestHub, 창의 점프 차단)을 TitlePlayerMotor에 잇는다.
    // 스테이지 플레이어의 JJBControlGate와 같은 역할을 JJB 없이 한다.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(TitlePlayerMotor), typeof(PlayerInputLock))]
    public sealed class TitlePlayerControl : MonoBehaviour, IPlayerControl, IJumpBlock {
        private IMovementLock _motor;
        private IInputLock _lock;

        private void Awake() {
            _motor = GetComponent<TitlePlayerMotor>();
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        private void OnEnable() {
            _lock.LockChanged += _motor.SetLocked;
            _motor.SetLocked(_lock.IsLocked);
        }

        private void OnDisable() => _lock.LockChanged -= _motor.SetLocked;

        public void SetSuspended(bool suspended) => _motor.SetSuspended(suspended);

        public void SetJumpBlocked(bool blocked) => _motor.SetJumpBlocked(blocked);
    }
}
