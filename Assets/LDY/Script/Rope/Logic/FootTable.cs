using UnityEngine;

namespace LDY.Script {
    public sealed class FootTable {
        private readonly int[] _frames;
        private readonly Vector2[] _pixels;

        public FootTable(int[] frames, Vector2[] pixels) {
            _frames = frames;
            _pixels = pixels;
        }

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
