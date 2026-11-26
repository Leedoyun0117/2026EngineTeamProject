using UnityEngine;

namespace LDY.Title.Core {
    // 점선 테두리(Tiled) 크기를 점 주기의 정수배로 맞춘다.
    // 테두리 두 변(8px + 8px)이 주기(16px)와 같으므로 전체가 정수배면 가운데 반복 구간도 정수배가 되어
    // 끝에서 점이 잘리지 않는다. 길이 단위는 호출하는 쪽 좌표(로컬 유닛)와 같으면 된다.
    public static class DottedFrame {
        // 이보다 짧으면 가운데 반복 구간이 없어 점선이 안 보임
        public const int MinPeriods = 2;

        // period가 0 이하면 맞추지 않음
        public static float SnapLength(float length, float period) {
            if (period <= 0f)
                return length;

            var count = Mathf.Max(MinPeriods, Mathf.FloorToInt(length / period + 0.5f));
            return count * period;
        }

        public static Vector2 SnapSize(Vector2 size, float period) {
            return new Vector2(SnapLength(size.x, period), SnapLength(size.y, period));
        }

        // 중심은 그대로 두고 크기만 맞춤
        public static Rect SnapRect(Rect rect, float period) {
            var size = SnapSize(rect.size, period);
            return new Rect(rect.center - size * 0.5f, size);
        }
    }
}
