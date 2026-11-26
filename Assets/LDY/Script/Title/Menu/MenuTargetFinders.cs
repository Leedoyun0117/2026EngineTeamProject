using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // 목록은 참조로 들고 있어서, 호출한 쪽이 목록 내용을 바꾸면 다음 Find부터 따라간다.
    public class StandingMenuFinder : IMenuTargetFinder {
        private readonly Collider2D _body;
        private readonly IMenuGate _gate;
        private readonly IReadOnlyList<IStandTarget> _icons;

        public StandingMenuFinder(IReadOnlyList<IStandTarget> icons, Collider2D body, IMenuGate gate) {
            _icons = icons;
            _body = body;
            _gate = gate;
        }

        public IMenuTarget Find() {
            for (var i = 0; i < _icons.Count; i++) {
                var icon = _icons[i];
                if (icon.IsUsable(_gate) && icon.IsStoodOn(_body))
                    return icon;
            }

            return null;
        }
    }

    public class HoverMenuFinder : IMenuTargetFinder {
        private readonly IAltStatus _alt;
        private readonly IMenuGate _gate;
        private readonly IReadOnlyList<MenuIcon> _icons;
        private readonly IMonitorSpace _space;

        public HoverMenuFinder(IReadOnlyList<MenuIcon> icons, IAltStatus alt, IMonitorSpace space, IMenuGate gate) {
            _icons = icons;
            _alt = alt;
            _space = space;
            _gate = gate;
        }

        public IMenuTarget Find() {
            if (!_space.PointerInside)
                return null;

            var cursor = _alt.CursorWorldPosition;
            for (var i = 0; i < _icons.Count; i++) {
                var icon = _icons[i];
                if (icon.IsUsable(_gate) && icon.ContainsPoint(cursor))
                    return icon;
            }

            return null;
        }
    }
}