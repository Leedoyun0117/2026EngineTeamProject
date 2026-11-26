namespace LDY.TitlePlayer.Core {
    public readonly struct MotorTuning {
        public MotorTuning(float moveSpeed, float jumpSpeed, float maxJumpHoldTime, float extraGravity,
            float gravityDelay) {
            MoveSpeed = moveSpeed;
            JumpSpeed = jumpSpeed;
            MaxJumpHoldTime = maxJumpHoldTime;
            ExtraGravity = extraGravity;
            GravityDelay = gravityDelay;
        }

        public float MoveSpeed { get; }
        public float JumpSpeed { get; }
        public float MaxJumpHoldTime { get; }
        public float ExtraGravity { get; }
        public float GravityDelay { get; }
    }

    // 점프 유지 중인지, 이번 점프에서 얼마나 눌렀는지, 공중에 얼마나 있었는지.
    public readonly struct MotorState {
        public MotorState(bool rising, float holdTime, float airTime) {
            Rising = rising;
            HoldTime = holdTime;
            AirTime = airTime;
        }

        public bool Rising { get; }
        public float HoldTime { get; }
        public float AirTime { get; }
    }

    public readonly struct MotorFrame {
        public float MoveX { get; }
        public bool JumpRequested { get; }
        public bool JumpHeld { get; }
        public bool Grounded { get; }
        public float VelocityY { get; }
        public bool Locked { get; }
        public bool Suspended { get; }
        public bool JumpBlocked { get; }
        public float DeltaTime { get; }

        public MotorFrame(float moveX, bool jumpRequested, bool jumpHeld, bool grounded, float velocityY,
            bool locked, bool suspended, bool jumpBlocked, float deltaTime) {
            MoveX = moveX;
            JumpRequested = jumpRequested;
            JumpHeld = jumpHeld;
            Grounded = grounded;
            VelocityY = velocityY;
            Locked = locked;
            Suspended = suspended;
            JumpBlocked = jumpBlocked;
            DeltaTime = deltaTime;
        }
    }

    // null이면 그 축 속도를 건드리지 않는다.
    public readonly struct MotorResult {
        public MotorResult(float? velocityX, float? velocityY, float forceY, MotorState state) {
            VelocityX = velocityX;
            VelocityY = velocityY;
            ForceY = forceY;
            State = state;
        }

        public float? VelocityX { get; }
        public float? VelocityY { get; }
        public float ForceY { get; }
        public MotorState State { get; }
    }

    // JJBPlayerMovement/JJBPlayerJump와 같은 규칙(코요테 타임 없음):
    // 접지 상태에서만 점프, 누르는 동안 maxJumpHoldTime까지 상승, 떼면 상승 속도를 0으로,
    // 공중에 gravityDelay보다 오래 있으면 extraGravity를 아래로 더한다. 잠금·정지 중에는 아무것도 하지 않는다.
    public static class MotorRules {
        public static float? TargetVelocityX(float moveX, float moveSpeed, bool locked, bool suspended) {
            if (locked || suspended) return null;
            return Clamp(moveX) * moveSpeed;
        }

        public static bool CanJump(bool requested, bool grounded, bool locked, bool suspended, bool jumpBlocked) =>
            requested && grounded && !locked && !suspended && !jumpBlocked;

        public static MotorResult Step(MotorTuning tuning, MotorState state, MotorFrame frame) {
            if (frame.Locked || frame.Suspended) return new MotorResult(null, null, 0f, state);

            var velocityX = TargetVelocityX(frame.MoveX, tuning.MoveSpeed, false, false);
            var grounded = frame.Grounded;
            float? velocityY = null;
            var rising = state.Rising;
            var hold = state.HoldTime;
            var air = state.AirTime;
            var currentY = frame.VelocityY;

            if (CanJump(frame.JumpRequested, grounded, false, false, frame.JumpBlocked)) {
                velocityY = currentY = tuning.JumpSpeed;
                hold = 0f;
                air = 0f;
                rising = true;
                grounded = false;
            }

            if (rising) {
                if (currentY <= 0f) {
                    rising = false;
                }
                else if (!frame.JumpHeld || hold >= tuning.MaxJumpHoldTime) {
                    velocityY = 0f;
                    rising = false;
                }
                else {
                    hold += frame.DeltaTime;
                    if (hold >= tuning.MaxJumpHoldTime) {
                        velocityY = 0f;
                        rising = false;
                    }
                }
            }

            air = grounded ? 0f : air + frame.DeltaTime;
            var force = !grounded && air > tuning.GravityDelay ? -tuning.ExtraGravity : 0f;
            return new MotorResult(velocityX, velocityY, force, new MotorState(rising, hold, air));
        }

        private static float Clamp(float value) => value < -1f ? -1f : value > 1f ? 1f : value;
    }
}
