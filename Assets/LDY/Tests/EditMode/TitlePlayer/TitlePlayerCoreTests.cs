using LDY.TitlePlayer.Core;
using NUnit.Framework;

namespace LDY.Tests.TitlePlayer {
    public class MotorVelocityTests {
        [TestCase(0f, 0f)]
        [TestCase(1f, 5f)]
        [TestCase(-1f, -5f)]
        [TestCase(-3f, -5f)]
        public void TargetFollowsInput(float input, float expected) {
            Assert.AreEqual(expected, MotorRules.TargetVelocityX(input, 5f, false, false));
        }

        [Test]
        public void LockedOrSuspendedDoesNotWriteVelocity() {
            Assert.IsNull(MotorRules.TargetVelocityX(1f, 5f, true, false));
            Assert.IsNull(MotorRules.TargetVelocityX(1f, 5f, false, true));
        }
    }

    public class MotorJumpTests {
        private const float Dt = 0.02f;
        private static readonly MotorTuning Tuning = new MotorTuning(5f, 11f, 0.5f, 30f, 0.15f);

        private static MotorResult Step(MotorState state, bool requested, bool held, bool grounded, float vy,
            bool locked = false, bool suspended = false, bool blocked = false) =>
            MotorRules.Step(Tuning, state,
                new MotorFrame(0f, requested, held, grounded, vy, locked, suspended, blocked, Dt));

        [Test]
        public void JumpsOnlyWhenGrounded() {
            Assert.AreEqual(11f, Step(default, true, true, true, 0f).VelocityY);
            Assert.IsNull(Step(default, true, true, false, -1f).VelocityY);
        }

        [Test]
        public void NoCoyoteTimeLikeJJB() {
            var leftLedge = new MotorState(false, 0f, 0.01f);

            Assert.IsNull(Step(leftLedge, true, true, false, -0.1f).VelocityY);
        }

        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, true)]
        public void LockSuspendOrBlockIgnoresJump(bool locked, bool suspended, bool blocked) {
            var result = Step(default, true, true, true, 0f, locked, suspended, blocked);

            Assert.IsNull(result.VelocityY);
            Assert.IsFalse(result.State.Rising);
        }

        [Test]
        public void LockedFrameChangesNothing() {
            var state = new MotorState(true, 0.1f, 0.3f);
            var result = Step(state, true, true, false, 3f, locked: true);

            Assert.IsNull(result.VelocityX);
            Assert.AreEqual(0f, result.ForceY);
            Assert.AreEqual(state, result.State);
        }

        [Test]
        public void ReleasingJumpCutsRise() {
            var rising = Step(default, true, true, true, 0f).State;
            var released = Step(rising, false, false, false, 9f);

            Assert.AreEqual(0f, released.VelocityY);
            Assert.IsFalse(released.State.Rising);
        }

        [Test]
        public void HoldingStopsAtMaxHoldTime() {
            var state = Step(default, true, true, true, 0f).State;
            float? last = null;
            for (var i = 0; i < 30 && state.Rising; i++) {
                var result = Step(state, false, true, false, 5f);
                state = result.State;
                last = result.VelocityY;
            }

            Assert.IsFalse(state.Rising);
            Assert.AreEqual(0f, last);
            Assert.GreaterOrEqual(state.HoldTime, Tuning.MaxJumpHoldTime);
        }

        [Test]
        public void ExtraGravityAfterDelayInAir() {
            Assert.AreEqual(0f, Step(new MotorState(false, 0f, 0.1f), false, false, false, -1f).ForceY);
            Assert.AreEqual(-30f, Step(new MotorState(false, 0f, 0.15f), false, false, false, -1f).ForceY);
            Assert.AreEqual(0f, Step(new MotorState(false, 0f, 1f), false, false, true, 0f).ForceY);
        }
    }

    public class FacingRuleTests {
        [TestCase(1f, -1, 1)]
        [TestCase(-1f, 1, -1)]
        [TestCase(0.01f, -1, -1)]
        [TestCase(0f, 1, 1)]
        public void KeepsPreviousInsideDeadZone(float velocityX, int previous, int expected) {
            Assert.AreEqual(expected, FacingRule.Next(velocityX, 0.05f, previous));
        }
    }

    public class TuningMappingTests {
        [Test]
        public void CopiesJJBValues() {
            var values = TuningMapping.FromJJB(new JJBMovementValues(5f, 11f, 0.5f, 30f, 0.15f, 0.5f, 0.25f, -0.68f, 64));

            Assert.AreEqual(5f, values.Motor.MoveSpeed);
            Assert.AreEqual(11f, values.Motor.JumpSpeed);
            Assert.AreEqual(0.5f, values.Motor.MaxJumpHoldTime);
            Assert.AreEqual(30f, values.Motor.ExtraGravity);
            Assert.AreEqual(0.15f, values.Motor.GravityDelay);
            Assert.AreEqual(0.5f, values.GroundCheckWidth);
            Assert.AreEqual(0.25f, values.GroundCheckHeight);
            Assert.AreEqual(-0.68f, values.GroundCheckOffsetY);
            Assert.AreEqual(64, values.GroundLayerMask);
        }

        [Test]
        public void KeepsJJBMinimums() {
            var values = TuningMapping.FromJJB(new JJBMovementValues(0f, -2f, -1f, -5f, -0.1f, -1f, -1f, 0f, 0));

            Assert.AreEqual(TuningMapping.MinMoveSpeed, values.Motor.MoveSpeed);
            Assert.AreEqual(0f, values.Motor.JumpSpeed);
            Assert.AreEqual(0f, values.Motor.MaxJumpHoldTime);
            Assert.AreEqual(0f, values.Motor.ExtraGravity);
            Assert.AreEqual(0f, values.Motor.GravityDelay);
            Assert.AreEqual(0f, values.GroundCheckWidth);
        }
    }
}
