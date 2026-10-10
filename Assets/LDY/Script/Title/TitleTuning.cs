namespace LDY.Script
{
    // 시작 화면과 Alt가 함께 쓰는 설정 상수.
    public static class TitleTuning
    {
        // 포커스 복귀 직후 프레임 시간이 튀는 것을 막는 델타타임 상한(초)
        public const float MaxDeltaTime = 0.05f;
        // 발판 윗면에 "서 있다"고 보는 허용 오차
        public const float StandTolerance = 0.1f;
    }
}
