using UnityEngine;

namespace LDY.Death {
    // LDY 쪽 사망 판정의 공통 통로. 연출 컴포넌트가 없는 대상(복사 플레이어 등)은 기존 KillHitbox처럼 파괴한다.
    public static class PlayerKill {
        public static void Request(Collider2D hit, DeathCause cause, float? pullToX = null) {
            if (hit == null)
                return;

            var death = hit.GetComponentInParent<IPlayerDeath>();
            if (death != null) {
                death.Die(cause, pullToX);
                return;
            }

            var body = hit.attachedRigidbody;
            Object.Destroy(body != null ? body.gameObject : hit.gameObject);
        }
    }
}
