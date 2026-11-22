using UnityEngine;

namespace LDY.Script {
    // 놓을 때 플레이어에게 줄 이탈 속도 계산.
    public static class ReleaseVelocity {
        // 접선 속도, 점프(Space)면 위쪽 보너스 추가
        public static Vector2 Compute(PendulumState state, bool jump, float jumpBonus) {
            var velocity = Pendulum.TangentVelocity(state);
            return jump ? velocity + Vector2.up * jumpBonus : velocity;
        }
    }
}
