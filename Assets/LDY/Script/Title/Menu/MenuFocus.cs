using UnityEngine;

namespace LDY.Script
{
    // owner로 구분해서 선택 방식끼리 서로 강조를 덮어쓰지 않게 함
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
