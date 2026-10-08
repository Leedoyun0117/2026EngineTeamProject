using UnityEngine;

namespace LDY.Script
{
    // 강조 중인 메뉴 항목 하나를 관리한다. 선택 방식(owner)이 바뀌어도 서로의 강조를 덮어쓰지 않는다.
    public class MenuFocus
    {
        private MenuIcon _current;
        private object _owner;

        public void Set(object owner, MenuIcon icon)
        {
            _owner = owner;
            if (_current == icon)
                return;

            if (_current != null)
                _current.SetHighlighted(false);

            _current = icon;
            if (_current != null)
                _current.SetHighlighted(true);
        }

        public void Release(object owner)
        {
            if (_owner == owner)
                Set(null, null);
        }
    }
}
