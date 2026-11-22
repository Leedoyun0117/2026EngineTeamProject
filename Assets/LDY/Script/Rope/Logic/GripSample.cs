using UnityEngine;

namespace LDY.Script {
    // 한 시점의 스프라이트 정보: 변환기, 좌우 반전 여부, 발 유무와 발 픽셀 위치.
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
