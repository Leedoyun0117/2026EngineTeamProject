namespace LDY.Script
{
    // 시작 화면과 Alt가 함께 쓰는 설정 상수.
    public static class TitleTuning
    {
        // 포커스 복귀 직후 프레임 시간이 튀는 것을 막는 델타타임 상한(초)
        public const float MaxDeltaTime = 0.05f;
        // 발판 윗면에 "서 있다"고 보는 허용 오차
        public const float StandTolerance = 0.1f;
        // 정지한 것으로 보는 세로 속도
        public const float RestSpeed = 0.1f;
        // 발판 위에 놓을 때 겹침 없이 시작하려고 띄우는 높이(이후 중력이 내려앉힘)
        public const float StandGap = 0.02f;
        // 0으로 나누지 않으려고 스케일에 거는 하한
        public const float MinScale = 0.0001f;
    }
}
