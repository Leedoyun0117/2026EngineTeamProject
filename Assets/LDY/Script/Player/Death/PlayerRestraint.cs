using UnityEngine;

namespace LDY.Script {
    // 죽는 동안 플레이어를 멈춘다. Alt는 먼저 끝내야 복귀 처리가 물리 설정을 되돌리지 않는다.
    internal static class PlayerRestraint {
        public static void Apply(GameObject player) {
            StopAlt(player);
            FreezeBody(player);
            DisableColliders(player);
        }

        private static void StopAlt(GameObject player) {
            foreach (var alt in player.GetComponentsInChildren<AltModeController>())
                alt.enabled = false;
        }

        private static void FreezeBody(GameObject player) {
            if (!player.TryGetComponent<Rigidbody2D>(out var body))
                return;

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        private static void DisableColliders(GameObject player) {
            foreach (var collider in player.GetComponentsInChildren<Collider2D>())
                collider.enabled = false;
        }

        // 스프라이트를 매 프레임 덮어쓰는 쪽(팀원 Animator, LDY PlayerRenderer)을 끈다.
        public static void MuteSpriteDrivers(GameObject player) {
            foreach (var animator in player.GetComponentsInChildren<Animator>())
                animator.enabled = false;
            foreach (var renderer in player.GetComponentsInChildren<PlayerRenderer>())
                renderer.enabled = false;
        }
    }

    internal static class PlayerSpriteLocator {
        public static SpriteRenderer Find(GameObject player) {
            var renderer = player.GetComponentInChildren<PlayerRenderer>();
            if (renderer != null && renderer.Target != null)
                return renderer.Target;

            foreach (var sprite in player.GetComponentsInChildren<SpriteRenderer>())
                if (sprite.enabled)
                    return sprite;

            return null;
        }
    }
}
