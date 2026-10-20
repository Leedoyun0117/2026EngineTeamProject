using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    public class StandingMenuFinder : IMenuTargetFinder
    {
        private readonly IReadOnlyList<MenuIcon> _icons;
        private readonly Collider2D _body;
        private readonly IMenuGate _gate;

        public StandingMenuFinder(IReadOnlyList<MenuIcon> icons, Collider2D body, IMenuGate gate)
        {
            _icons = icons;
            _body = body;
            _gate = gate;
        }

        public MenuIcon Find()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                MenuIcon icon = _icons[i];
                if (icon.IsUsable(_gate) && icon.IsStoodOn(_body))
                    return icon;
            }

            return null;
        }
    }

    public class HoverMenuFinder : IMenuTargetFinder
    {
        private readonly IReadOnlyList<MenuIcon> _icons;
        private readonly IAltStatus _alt;
        private readonly IMonitorSpace _space;
        private readonly IMenuGate _gate;

        public HoverMenuFinder(IReadOnlyList<MenuIcon> icons, IAltStatus alt, IMonitorSpace space, IMenuGate gate)
        {
            _icons = icons;
            _alt = alt;
            _space = space;
            _gate = gate;
        }

        public MenuIcon Find()
        {
            if (!_space.PointerInside)
                return null;

            Vector2 cursor = _alt.CursorWorldPosition;
            for (int i = 0; i < _icons.Count; i++)
            {
                MenuIcon icon = _icons[i];
                if (icon.IsUsable(_gate) && icon.ContainsPoint(cursor))
                    return icon;
            }

            return null;
        }
    }
}
