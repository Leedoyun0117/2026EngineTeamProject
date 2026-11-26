using System.Collections.Generic;
using LDY.Script;

namespace LDY.Death {
    public static class DeathClipOrder {
        private static readonly PlayerAnimId[] TrappedThenDeath = { PlayerAnimId.Trapped, PlayerAnimId.Death };
        private static readonly PlayerAnimId[] DeathOnly = { PlayerAnimId.Death };

        public static IReadOnlyList<PlayerAnimId> Puppet => DeathOnly;

        public static IReadOnlyList<PlayerAnimId> For(DeathCause cause) =>
            cause == DeathCause.Trap ? TrappedThenDeath : DeathOnly;
    }

    public enum DestroyVerdict {
        Ignore,
        SpawnPuppet
    }

    // 재시작 요청 전에 플레이어가 사라지면(팀원 Destroy 등) 대역이 연출과 재시작을 이어받는다.
    public static class DestroyJudge {
        public static DestroyVerdict Judge(DeathPhase phase, bool quitting, bool sceneUnloading) {
            if (quitting || sceneUnloading || phase == DeathPhase.RestartRequested)
                return DestroyVerdict.Ignore;

            return DestroyVerdict.SpawnPuppet;
        }
    }

    // 보이던 스프라이트와 PPU가 다른 클립을 같은 픽셀 크기로 보이게 하는 배율.
    public static class PixelScale {
        public static float Ratio(float shownPixelsPerUnit, float clipPixelsPerUnit) {
            if (shownPixelsPerUnit <= 0f || clipPixelsPerUnit <= 0f)
                return 1f;

            return clipPixelsPerUnit / shownPixelsPerUnit;
        }
    }
}
