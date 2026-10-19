using UnityEngine;

namespace LDY.Script
{
    // 행 간격에 따라 달라지는 값은 창마다 WindowLayout에 따로 둠
    public static class WindowStyle
    {
        public const float Width = 10f;
        public const float FrameBorder = 12f * WindowKit.Pixel;
        public const float TitleBarHeight = 48f * WindowKit.Pixel;
        public const float TitleGap = 0.35f;
        public const float FooterMargin = 0.45f;
        // 닫기 줄과 "ESC 닫기" 줄 사이 간격 수
        public const int RowGaps = 5;
        public const float RowCenterAbovePlatform = 0.55f;
        // 근처 판정 구역이 발판 윗면보다 이만큼 아래부터 시작
        public const float ZoneBelow = 0.3f;

        public const float InnerLeft = -Width * 0.5f + FrameBorder;
        public const float InnerRight = Width * 0.5f - FrameBorder;

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

    // 설정창은 행 이동이라 간격이 좁아도 됨(1.2)
    // 크레딧창은 물리 점프라 키(1.5) + 발판 두께(0.1) + 접촉 여유(0.05) 이상 필요(1.65)
    public readonly struct WindowLayout
    {
        public static readonly WindowLayout Settings = new WindowLayout(1.2f, 1.4f);
        public static readonly WindowLayout Credit = new WindowLayout(1.65f, 1.8f);

        // zoneHeight: 구역은 발판 윗면 -ZoneBelow ~ +zoneHeight-ZoneBelow
        // 틈이 없으려면 행 간격 - ZoneBelow 이상, 위 행 발바닥(간격 + 0.02)이 아래 행 구역에 안 들어가려면
        // zoneHeight - ZoneBelow < 행 간격. 위 행 버튼 구역이 좁으면 아래 행 위젯이 대신 선택됨
        private WindowLayout(float rowSpacing, float zoneHeight)
        {
            RowSpacing = rowSpacing;
            ZoneHeight = zoneHeight;
        }

        public float RowSpacing { get; }
        public float ZoneHeight { get; }

        public float Height => WindowStyle.FrameBorder * 2f + WindowStyle.TitleBarHeight + WindowStyle.TitleGap
            + RowSpacing * WindowStyle.RowGaps + WindowStyle.FooterMargin;
        public float InnerTop => Height * 0.5f - WindowStyle.FrameBorder;
        public float InnerBottom => -Height * 0.5f + WindowStyle.FrameBorder;
        public float TitleBarCenterY => InnerTop - WindowStyle.TitleBarHeight * 0.5f;
        // 발판 윗면 높이(맨 위 닫기 줄, 맨 아래 ESC 줄)
        public float CloseTop => InnerTop - WindowStyle.TitleBarHeight - WindowStyle.TitleGap;
        public float SliderTop0 => CloseTop - RowSpacing;
        public float FooterTop => CloseTop - RowSpacing * WindowStyle.RowGaps;
        public float HintY => InnerBottom + 0.17f;
    }
}
