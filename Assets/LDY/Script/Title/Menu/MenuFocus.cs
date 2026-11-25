namespace LDY.Script {
    // owner로 구분해서 선택 방식끼리 서로 강조를 덮어쓰지 않게 함
    public class MenuFocus {
        private IHighlightable _current;
        private object _owner;

        public void Set(object owner, IHighlightable icon) {
            _owner = owner;
            if (ReferenceEquals(_current, icon))
                return;

            if (IsAlive(_current))
                _current.SetHighlighted(false);

            _current = icon;
            if (IsAlive(_current))
                _current.SetHighlighted(true);
        }

        public void Release(object owner) {
            if (_owner == owner)
                Set(null, null);
        }

        // 파괴된 컴포넌트(MenuIcon)는 Unity 방식으로 null 취급한다(인터페이스로 바뀌기 전과 같은 판정).
        private static bool IsAlive(IHighlightable target) {
            return target is UnityEngine.Object unityObject ? unityObject != null : target != null;
        }
    }
}
