using UnityEngine;

namespace LDY.Script {
    // 프레임 번호 -> 발 픽셀 위치 조회표.
    public sealed class FootTable {
        private readonly int[] _frames;
        private readonly Vector2[] _pixels;

        public FootTable(int[] frames, Vector2[] pixels) {
            _frames = frames;
            _pixels = pixels;
        }

        // 선형 탐색으로 프레임에 맞는 발 위치를 찾는다(항목이 적어 충분).
        public bool TryGet(int frame, out Vector2 pixel) {
            var count = Mathf.Min(_frames.Length, _pixels.Length);
            for (var i = 0; i < count; i++) {
                if (_frames[i] != frame)
                    continue;

                pixel = _pixels[i];
                return true;
            }

            pixel = default;
            return false;
        }
    }
}
