using System.Collections.Generic;

namespace LDY.SkillTargets.Core {
    // 대상 오브젝트의 현재 상태. 컴포넌트 타입 이름 → (필드 이름 → 값 문자열)
    public sealed class TargetSnapshot {
        public bool Found { get; set; } = true;
        public bool HasCollider { get; set; }
        public bool HasSpriteRenderer { get; set; }
        public bool HasRigidbody { get; set; }
        public string MaterialPath { get; set; }
        public int Layer { get; set; }
        public Dictionary<string, Dictionary<string, string>> Components { get; } =
            new Dictionary<string, Dictionary<string, string>>();

        public static TargetSnapshot NotFound() => new TargetSnapshot { Found = false };
    }
}
