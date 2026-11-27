using System.Linq;

namespace LDY.PlayerMerge {
    // 최종 플레이어의 부품 목록. 출처: A(지빈) 게임플레이, B(태영) 스킬, C(LDY) 비주얼·잠금.
    public static class PlayerParts {
        public const string GroundCheck = "GroundCheck";
        public const string InteractPoint = "InteractPoint";
        public const string AbsorbPoint = "AbsorbPoint";
        public const string LegacyVisual = "Visual";
        public const string Visual = "PinkVisual";
        public const string SkillManager = "SkillManager_HTY";
        public const string CopySkill = "CopyPlayer_HTY";
        public const string CopySkillPrefabField = "_playerPrefab";
        public const string PlayerTag = "Player";
        public const string PlayerLayer = "Player";

        // JJBLevel이 A에 덮어쓴 값. A 프리팹 기본값(-0.68)은 루트 스케일 2 이전 값이라 쓰지 않는다.
        public const float JibinGroundCheckLocalY = -0.065f;

        public const float DefaultVisualScale = 0.8f;

        public static readonly string[] JibinChildren = { GroundCheck, InteractPoint, AbsorbPoint };

        public static readonly string[] JibinComponents = {
            "Rigidbody2D", "JJBPlayerMovement", "JJBPlayerJump", "JJBPlayerInteract", "JJBPlayerAbsorb"
        };

        public const string Collider = "CapsuleCollider2D";
        public const string Gimmick = "JJBPlayerGimmick";

        public static readonly string[] HtyRootComponents = { SkillManager, "TargetSelector_HTY", "Utility_HTY", "FixedJoint2D" };
        public static readonly string[] HtyChildren = { "Skills", "Player Light" };

        public static readonly string[] LegacyVisualComponents = { "SpriteRenderer", "Animator" };

        public static readonly string[] CopyRemovedComponents =
            HtyRootComponents.Concat(new[] { "AltModeController", "PlayerAbilityGate" }).ToArray();

        public static readonly string[] CommonRootComponents = {
            "Rigidbody2D", Collider, "JJBPlayer", "JJBPlayerInput", "JJBPlayerMovement", "JJBPlayerJump",
            "JJBPlayerInteract", "JJBPlayerAbsorb", Gimmick, "PlayerInput", "PlayerInputLock", "JJBControlGate"
        };

        public static readonly string[] BodyOnlyRootComponents =
            HtyRootComponents.Concat(new[] { "PlayerAbilityGate", "AltModeController" }).ToArray();

        public static readonly string[] VisualComponents = { "PlayerRenderer", "StagePlayerAnimBootstrap", "PlayerGripRig" };
    }
}
