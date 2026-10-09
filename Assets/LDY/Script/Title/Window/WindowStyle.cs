using UnityEngine;

namespace LDY.Script
{
    // 창 배치와 글자 스타일 값. 창 로컬 좌표(중심 기준, 월드 유닛).
    // 행 간격 1.2유닛은 점프 높이(약 4유닛)로 한 번에 올라갈 수 있고, 플레이어 키(1.5)보다 촘촘해 근처 판정이 이어진다.
    public static class WindowStyle
    {
        public const float Width = 10f;
        public const float Height = 8f;
        public const float FrameBorder = 12f * WindowKit.Pixel;
        public const float TitleBarHeight = 48f * WindowKit.Pixel;

        public const float RowSpacing = 1.2f;
        // 행 중심은 발판 윗면보다 이만큼 위
        public const float RowCenterAbovePlatform = 0.55f;
        // 플레이어 발바닥이 발판 윗면 기준 이 범위 안에 있으면 그 행 근처
        // 근처 판정은 행 간격보다 넓게 잡아 하이라이트가 잘 뜨게 한다. 겹치는 구역은 가장 가까운 행이 이긴다.
        public const float ZoneBelow = 0.3f;
        public const float ZoneHeight = 1.6f;

        public const float InnerLeft = -Width * 0.5f + FrameBorder;
        public const float InnerRight = Width * 0.5f - FrameBorder;
        public const float InnerTop = Height * 0.5f - FrameBorder;
        public const float InnerBottom = -Height * 0.5f + FrameBorder;
        public const float TitleBarCenterY = InnerTop - TitleBarHeight * 0.5f;

        // 발판 윗면 높이. 맨 위가 닫기 줄, 맨 아래가 "ESC 닫기" 줄
        // 행 간격 1.2는 점프 한 번(정점 약 3.95)으로 위 칸에 오르고도 남는다. 플레이어 키(1.5)보다 좁아 위 발판은 아래에서 통과해야 한다.
        public const float CloseTop = InnerTop - TitleBarHeight - 0.35f;
        public const float SliderTop0 = CloseTop - RowSpacing;
        public const float FooterTop = CloseTop - RowSpacing * 5f;
        public const float HintY = InnerBottom + 0.17f;

        public const float LabelX = -4.5f;
        public const float PercentX = 4.6f;
        public static readonly Vector2 TrackX = new Vector2(-1.4f, 3.0f);

        public const float RowFont = 4.5f;
        public const float TitleFont = 4.5f;
        public const float HintFont = 4.5f;
        public const float ControlHintFont = 3f;

        public static readonly Color Text = new Color(0.16f, 0.16f, 0.2f);
        public static readonly Color TitleText = Color.white;
        public static readonly Color HintText = new Color(0.42f, 0.42f, 0.46f);
    }
}
