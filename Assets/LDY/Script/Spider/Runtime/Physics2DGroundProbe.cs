using UnityEngine;

namespace LDY.Spider {
    public sealed class Physics2DGroundProbe : IGroundProbe {
        private const float ProbeDistance = 60f;
        private readonly LayerMask _mask;

        public Physics2DGroundProbe(LayerMask mask) {
            _mask = mask;
        }

        public bool TryGetGroundY(float x, float fromY, out float y) {
            var hit = Physics2D.Raycast(new Vector2(x, fromY), Vector2.down, ProbeDistance, _mask);
            y = hit.collider != null ? hit.point.y : 0f;
            return hit.collider != null;
        }
    }
}
