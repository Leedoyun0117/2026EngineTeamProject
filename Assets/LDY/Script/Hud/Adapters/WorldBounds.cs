using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud.Adapters {
    internal static class WorldBounds {
        public static HudRect Of(GameObject target) {
            if (target == null)
                return default;

            if (target.TryGetComponent<Collider2D>(out var own) && own.enabled)
                return From(own.bounds);

            var child = target.GetComponentInChildren<Collider2D>();
            if (child != null)
                return From(child.bounds);

            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
                return From(renderer.bounds);

            var position = target.transform.position;
            return new HudRect(position.x, position.y, 0f, 0f);
        }

        private static HudRect From(Bounds bounds) =>
            new HudRect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
    }
}
