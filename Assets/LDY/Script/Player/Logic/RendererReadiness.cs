namespace LDY.Script {
    public enum RendererReadiness {
        Ready,
        MissingAnimSet,
        Broken
    }

    public static class RendererReadinessRules {
        // 세트만 없으면 애니메이션 없이 기본 스프라이트를 유지한다. 소스나 렌더 대상이 없으면 동작할 수 없다.
        public static RendererReadiness Evaluate(bool hasSource, bool hasTarget, bool hasAnimSet) {
            if (!hasSource || !hasTarget)
                return RendererReadiness.Broken;

            return hasAnimSet ? RendererReadiness.Ready : RendererReadiness.MissingAnimSet;
        }
    }
}
