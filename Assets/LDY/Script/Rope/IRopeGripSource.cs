namespace LDY.Script {
    // 플레이어 스프라이트에서 손/발 픽셀 정보(GripSample)를 읽어오는 쪽. 줄 코드는 스프라이트 구현을 몰라도 된다.
    public interface IRopeGripSource {
        bool TryGetSample(out GripSample sample);
    }

    // 줄이 계산한 매달림 회전각(라디안)을 플레이어 렌더러에 알려주는 쪽.
    public interface IRopeAngleSink {
        void ReportRopeAngle(float radians);
    }

    // 현재 줄 회전각을 읽기 전용으로 노출.
    public interface IRopeAngleSource {
        float RopeAngle { get; }
    }
}
