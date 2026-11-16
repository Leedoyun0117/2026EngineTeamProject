namespace LDY.Script {
    public interface IRopeGripSource {
        bool TryGetSample(out GripSample sample);
    }

    public interface IRopeAngleSink {
        void ReportRopeAngle(float radians);
    }

    public interface IRopeAngleSource {
        float RopeAngle { get; }
    }
}
