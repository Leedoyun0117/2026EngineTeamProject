namespace LDY.PlayerMerge {
    // 씬 인스턴스를 최종본으로 바꿀 때 옮기는 오버라이드는 루트의 위치·회전·이름뿐이다. 나머지는 보고만 한다.
    public static class SwapOverrideRule {
        private static readonly string[] CarriedTransformPrefixes = { "m_LocalPosition", "m_LocalRotation", "m_LocalEulerAnglesHint" };
        private const string NameProperty = "m_Name";
        private const string ActiveProperty = "m_IsActive";

        public static bool IsCarried(bool onRootTransform, bool onRootGameObject, string propertyPath) {
            if (onRootGameObject)
                return propertyPath == NameProperty || propertyPath == ActiveProperty;

            if (!onRootTransform)
                return false;

            foreach (var prefix in CarriedTransformPrefixes)
                if (propertyPath.StartsWith(prefix))
                    return true;

            return false;
        }
    }
}
