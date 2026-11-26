using UnityEngine;

namespace LDY.Flow {
    // Win95 제목표시줄 치수. 버튼은 레이아웃 그룹 없이 오른쪽 위 앵커 + 정수 좌표로 고정한다.
    // 오른쪽 끝 배치: [최소화][최대화] 2px [닫기] 2px| (최소화·최대화는 간격 0)
    public static class WindowChrome {
        public const float TitleBarInset = 3f;
        public const float TitleBarHeight = 14f;
        public const float TitleLeft = 4f;
        public const float RightMargin = 2f;
        public const float CloseGap = 2f;
        private const float TitleGap = 4f;
        public static readonly Vector2 ButtonSize = new Vector2(12f, 10f);

        public enum TitleButton {
            Close,
            Maximize,
            Minimize
        }

        // 버튼 오른쪽 끝이 제목표시줄 오른쪽 끝에서 얼마나 안쪽인지(px)
        public static float RightOffset(TitleButton button) {
            switch (button) {
                case TitleButton.Close: return RightMargin;
                case TitleButton.Maximize: return RightMargin + ButtonSize.x + CloseGap;
                default: return RightMargin + ButtonSize.x + CloseGap + ButtonSize.x;
            }
        }

        // 제목 글자가 버튼을 덮지 않도록 오른쪽에 남길 폭
        public static float TitleRightInset(bool hasMinMax) =>
            RightOffset(hasMinMax ? TitleButton.Minimize : TitleButton.Close) + ButtonSize.x + TitleGap;

        // 오른쪽 위 기준. 세로는 가운데지만 내림한 정수 좌표라 반 픽셀이 생기지 않는다.
        public static RectTransform PlaceButton(RectTransform rect, TitleButton button,
            float barHeight = TitleBarHeight) {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.sizeDelta = ButtonSize;
            rect.anchoredPosition =
                new Vector2(-RightOffset(button), -Mathf.Floor((barHeight - ButtonSize.y) * 0.5f));
            return rect;
        }
    }
}
