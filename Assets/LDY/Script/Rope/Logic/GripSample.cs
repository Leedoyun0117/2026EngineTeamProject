using UnityEngine;

namespace LDY.Script {
    public readonly struct GripSample {
        public readonly GripLayout Layout;
        public readonly bool Flipped;
        public readonly bool HasFoot;
        public readonly Vector2 FootPixel;

        public GripSample(GripLayout layout, bool flipped, bool hasFoot, Vector2 footPixel) {
            Layout = layout;
            Flipped = flipped;
            HasFoot = hasFoot;
            FootPixel = footPixel;
        }
    }
}
