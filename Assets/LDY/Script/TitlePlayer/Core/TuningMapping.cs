namespace LDY.TitlePlayer.Core {
    // 지빈 Player.prefab의 JJBPlayerMovement·JJBPlayerJump 직렬화 값.
    public readonly struct JJBMovementValues {
        public JJBMovementValues(float moveSpeed, float jumpSpeed, float maxJumpHoldTime, float extraGravity,
            float gravityDelay, float groundCheckWidth, float groundCheckHeight, float groundCheckOffsetY,
            int groundLayerMask) {
            MoveSpeed = moveSpeed;
            JumpSpeed = jumpSpeed;
            MaxJumpHoldTime = maxJumpHoldTime;
            ExtraGravity = extraGravity;
            GravityDelay = gravityDelay;
            GroundCheckWidth = groundCheckWidth;
            GroundCheckHeight = groundCheckHeight;
            GroundCheckOffsetY = groundCheckOffsetY;
            GroundLayerMask = groundLayerMask;
        }

        public float MoveSpeed { get; }
        public float JumpSpeed { get; }
        public float MaxJumpHoldTime { get; }
        public float ExtraGravity { get; }
        public float GravityDelay { get; }
        public float GroundCheckWidth { get; }
        public float GroundCheckHeight { get; }
        public float GroundCheckOffsetY { get; }
        public int GroundLayerMask { get; }
    }

    public readonly struct TitleTuningValues {
        public TitleTuningValues(MotorTuning motor, float groundCheckWidth, float groundCheckHeight,
            float groundCheckOffsetY, int groundLayerMask) {
            Motor = motor;
            GroundCheckWidth = groundCheckWidth;
            GroundCheckHeight = groundCheckHeight;
            GroundCheckOffsetY = groundCheckOffsetY;
            GroundLayerMask = groundLayerMask;
        }

        public MotorTuning Motor { get; }
        public float GroundCheckWidth { get; }
        public float GroundCheckHeight { get; }
        public float GroundCheckOffsetY { get; }
        public int GroundLayerMask { get; }
    }

    // JJB 쪽 [Min] 제약(moveSpeed ≥ 1, 나머지 ≥ 0)을 그대로 지켜 옮긴다.
    public static class TuningMapping {
        public const float MinMoveSpeed = 1f;

        public static TitleTuningValues FromJJB(JJBMovementValues jjb) =>
            new TitleTuningValues(
                new MotorTuning(Max(jjb.MoveSpeed, MinMoveSpeed), Max(jjb.JumpSpeed, 0f),
                    Max(jjb.MaxJumpHoldTime, 0f), Max(jjb.ExtraGravity, 0f), Max(jjb.GravityDelay, 0f)),
                Max(jjb.GroundCheckWidth, 0f), Max(jjb.GroundCheckHeight, 0f), jjb.GroundCheckOffsetY,
                jjb.GroundLayerMask);

        private static float Max(float value, float min) => value < min ? min : value;
    }
}
