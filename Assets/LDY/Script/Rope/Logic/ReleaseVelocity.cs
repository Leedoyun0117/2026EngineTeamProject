using UnityEngine;

namespace LDY.Script {
    public static class ReleaseVelocity {
        public static Vector2 Compute(PendulumState state, bool jump, float jumpBonus) {
            var velocity = Pendulum.TangentVelocity(state);
            return jump ? velocity + Vector2.up * jumpBonus : velocity;
        }
    }
}
