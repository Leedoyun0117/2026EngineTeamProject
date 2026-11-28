namespace LDY.Hud.Core {
    public enum AbsorbPrompt {
        Hidden,
        Available,
        Blocked
    }

    public enum AbsorbOutcome {
        None,
        Success,
        AlreadyHolding,
        Rejected
    }

    public readonly struct AbsorbSnapshot {
        public bool HasTarget { get; }
        public bool IsHolding { get; }
        public bool IsLocked { get; }

        public AbsorbSnapshot(bool hasTarget, bool isHolding, bool isLocked) {
            HasTarget = hasTarget;
            IsHolding = isHolding;
            IsLocked = isLocked;
        }
    }

    // 흡수 안내 표시와 E 입력 결과 판정. before는 입력 직전 프레임 상태, after는 입력을 처리한 뒤 상태.
    public static class AbsorbPromptResolver {
        public static AbsorbPrompt Prompt(AbsorbSnapshot now, bool recentlyFailed = false) {
            if (now.IsLocked || !now.HasTarget)
                return AbsorbPrompt.Hidden;
            if (now.IsHolding || recentlyFailed)
                return AbsorbPrompt.Blocked;
            return AbsorbPrompt.Available;
        }

        public static AbsorbOutcome Judge(AbsorbSnapshot before, AbsorbSnapshot after) {
            if (before.IsLocked || !before.HasTarget)
                return AbsorbOutcome.None;
            if (before.IsHolding)
                return AbsorbOutcome.AlreadyHolding;
            return after.IsHolding ? AbsorbOutcome.Success : AbsorbOutcome.Rejected;
        }

        public static bool IsFailure(AbsorbOutcome outcome) =>
            outcome == AbsorbOutcome.AlreadyHolding || outcome == AbsorbOutcome.Rejected;
    }
}
