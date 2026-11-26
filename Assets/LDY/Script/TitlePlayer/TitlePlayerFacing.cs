using LDY.TitlePlayer.Core;
using UnityEngine;

namespace LDY.TitlePlayer {
    // 좌우 뒤집기는 이 컴포넌트만 한다. 데드존 안에서는 마지막 방향을 유지한다.
    [RequireComponent(typeof(TitlePlayerMotor))]
    public sealed class TitlePlayerFacing : MonoBehaviour {
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] [Min(0f)] private float deadZone = 0.05f;

        private IMotorReadout _motor;
        private int _sign = 1;

        private void Awake() {
            _motor = GetComponent<TitlePlayerMotor>();
            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        }

        // 멈춰 있을 때는 손대지 않아서 Alt 복귀가 되돌려 놓은 방향을 덮어쓰지 않는다.
        private void LateUpdate() {
            var velocityX = _motor.Velocity.x;
            _sign = FacingRule.Next(velocityX, deadZone, _sign);
            if (sprite == null || Mathf.Abs(velocityX) <= deadZone) return;
            sprite.flipX = _sign < 0;
        }
    }
}
