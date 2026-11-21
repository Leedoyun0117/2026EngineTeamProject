using UnityEngine;

namespace LDY.Script {
    // 새 커서 캐릭터(64x64 캔버스)의 로프 손/발 픽셀. 로프는 스프라이트 가운데로 잡는다.
    internal static class CursorGripData {
        public const int FirstRopeFrame = 28;
        public const int RopeFrameCount = 10;

        public static readonly Vector2 HandPixel = new(32f, 8f);
        public static readonly Vector2 FootPixel = new(32f, 58f);

        public static RopeGripProfile.FootEntry[] Feet() {
            var feet = new RopeGripProfile.FootEntry[RopeFrameCount];
            for (var i = 0; i < feet.Length; i++)
                feet[i] = new RopeGripProfile.FootEntry { frame = FirstRopeFrame + i, pixel = FootPixel };

            return feet;
        }
    }
}
