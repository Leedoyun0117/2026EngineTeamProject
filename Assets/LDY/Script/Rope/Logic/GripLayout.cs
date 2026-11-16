using UnityEngine;

namespace LDY.Script {
    public readonly struct GripLayout {
        private const float PixelCenter = 0.5f;

        private readonly float _pixelsPerUnit;
        private readonly Vector2 _pivotPixel;
        private readonly Vector2 _handPixel;

        public GripLayout(float pixelsPerUnit, Vector2 pivotPixel, Vector2 handPixel) {
            _pixelsPerUnit = Mathf.Max(pixelsPerUnit, Mathf.Epsilon);
            _pivotPixel = pivotPixel;
            _handPixel = handPixel;
        }

        // 스프라이트를 가진 오브젝트가 scale 배로 커져 있으면 같은 픽셀이 월드에서 그만큼 멀어진다.
        public GripLayout Scaled(float scale) {
            return new GripLayout(_pixelsPerUnit / Mathf.Max(Mathf.Abs(scale), Mathf.Epsilon), _pivotPixel, _handPixel);
        }

        public Vector2 HandLocal(bool flipped) {
            return Local(_handPixel, flipped);
        }

        public Vector2 Local(Vector2 pixel, bool flipped) {
            var x = (pixel.x + PixelCenter - _pivotPixel.x) / _pixelsPerUnit;
            var y = (_pivotPixel.y - pixel.y - PixelCenter) / _pixelsPerUnit;
            return new Vector2(flipped ? -x : x, y);
        }

        public Vector2 OffsetFromHand(Vector2 pixel, bool flipped, float radians) {
            return Rotate(Local(pixel, flipped) - HandLocal(flipped), radians);
        }

        public Vector2 PointWorld(Vector2 handWorld, Vector2 pixel, bool flipped, float radians) {
            return handWorld + OffsetFromHand(pixel, flipped, radians);
        }

        public static Vector2 Rotate(Vector2 value, float radians) {
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }
    }
}
