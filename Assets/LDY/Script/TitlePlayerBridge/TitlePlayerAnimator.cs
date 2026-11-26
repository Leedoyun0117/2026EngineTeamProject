using LDY.TitlePlayer;
using UnityEngine;

namespace LDY.Script {
    // 커서 캐릭터 애니메이션(PlayerRenderer)에 움직임을 넘긴다. 좌우 뒤집기는 TitlePlayerFacing이 맡으므로 FacingSign은 0.
    [RequireComponent(typeof(TitlePlayerMotor), typeof(PlayerInputLock))]
    public sealed class TitlePlayerAnimator : MonoBehaviour, IPlayerMotionSource {
        private IMotorReadout _motor;
        private IInputLock _lock;

        private void Awake() {
            _motor = GetComponent<TitlePlayerMotor>();
            _lock = GetComponent<PlayerInputLock>().Lock;
        }

        public Vector2 Velocity => _motor.Velocity;
        public bool IsGrounded => _motor.IsGrounded;
        public int FacingSign => 0;
        public bool IsInputLocked => _lock.IsLocked;
    }
}
