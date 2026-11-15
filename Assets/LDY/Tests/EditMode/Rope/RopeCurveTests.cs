using System.Collections.Generic;
using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class RopeCurveTests {
        private const float Dt = 0.02f;
        private const float HoldDistance = 5f;
        private static readonly Vector2 Anchor = new(0f, 10f);

        private static RopeCurveTuning Curve(float total = 8f) => new() {
            segmentCount = 20,
            totalLength = total,
            gravity = 100f,
            iterations = 100,
            holdBlendTime = 0.15f,
            sleepSpeed = 0.002f,
            tailStiffness = 0.5f
        };

        private static RopeDampingTuning Damping(float settle = 3f) => new() {
            heldDamping = 1.5f,
            heldTailDamping = 8f,
            releaseDamping = 0.3f,
            settleTime = settle,
            releaseBlendTime = 0.25f
        };

        private static RopeSimulation Create(float settle = 3f) {
            var rope = new RopeSimulation(Curve(), Damping(settle));
            rope.Initialize(Anchor);
            return rope;
        }

        private static Vector2 OnArc(float distance, float angle) {
            return Anchor + distance * new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle));
        }

        private static void Swing(RopeSimulation rope, float seconds, float amplitude) {
            var steps = Mathf.RoundToInt(seconds / Dt);
            for (var i = 0; i < steps; i++)
                rope.StepHeld(Anchor, HoldDistance, OnArc(HoldDistance, amplitude * Mathf.Sin(i * Dt * 3f)), Dt);
        }

        private static void SweepRight(RopeSimulation rope, float angularSpeed, int steps) {
            for (var i = 0; i < steps; i++)
                rope.StepHeld(Anchor, HoldDistance, OnArc(HoldDistance, angularSpeed * i * Dt), Dt);
        }

        private static float MaxSegmentError(RopeSimulation rope) {
            var worst = 0f;
            for (var i = 0; i < rope.Points.Count - 1; i++)
                worst = Mathf.Max(worst, Mathf.Abs(Vector2.Distance(rope.Points[i], rope.Points[i + 1]) - rope.SegmentLength));
            return worst;
        }

        private static float TipSwing(RopeSimulation rope) {
            return Mathf.Abs(rope.Points[rope.Points.Count - 1].x - Anchor.x);
        }

        private static Vector2[] Snapshot(RopeSimulation rope) {
            var copy = new Vector2[rope.Points.Count];
            for (var i = 0; i < copy.Length; i++)
                copy[i] = rope.Points[i];
            return copy;
        }

        private static float Envelope(List<float> series, float from, float to) {
            var worst = 0f;
            for (var i = Mathf.RoundToInt(from / Dt); i < Mathf.Min(series.Count, Mathf.RoundToInt(to / Dt)); i++)
                worst = Mathf.Max(worst, series[i]);
            return worst;
        }

        [Test]
        public void FreeRope_KeepsSegmentLengthAfterIterations() {
            var rope = Create();
            Swing(rope, 1.5f, 0.6f);
            rope.Release();

            for (var i = 0; i < 100; i++)
                rope.Step(Anchor, Dt);

            Assert.Less(MaxSegmentError(rope), rope.SegmentLength * 0.03f);
        }

        [Test]
        public void AnchorPoint_NeverMoves() {
            var rope = Create();
            Swing(rope, 1.5f, 0.6f);
            Assert.AreEqual(Anchor, rope.Points[0]);
            rope.Release();

            for (var i = 0; i < 100; i++) {
                rope.Step(Anchor, Dt);
                Assert.AreEqual(Anchor, rope.Points[0]);
            }
        }

        [Test]
        public void HeldPoint_FollowsPlayerPosition() {
            var rope = Create();
            var target = OnArc(HoldDistance, 0.4f);

            rope.StepHeld(Anchor, HoldDistance, target, Dt);

            Assert.GreaterOrEqual(rope.PinIndex, 1);
            Assert.AreEqual(target.x, rope.Points[rope.PinIndex].x, 1e-4f);
            Assert.AreEqual(target.y, rope.Points[rope.PinIndex].y, 1e-4f);
        }


        [Test]
        public void HeldStill_RopeDoesNotJitter() {
            var rope = Create();
            var position = OnArc(3.5f, 0.3f);
            rope.TryGetGrabPoint(position, 5f, out _);
            var previous = Snapshot(rope);
            var worst = 0f;

            for (var i = 0; i < 300; i++) {
                rope.StepHeld(Anchor, 3.5f, position, Dt);
                if (i > 200)
                    for (var k = 0; k < previous.Length; k++)
                        worst = Mathf.Max(worst, Vector2.Distance(previous[k], rope.Points[k]));
                previous = Snapshot(rope);
            }

            Assert.Less(worst, 0.003f);
        }

        [Test]
        public void HeldRope_UpperSectionIsStraightBetweenAnchorAndPlayer() {
            var rope = Create();
            var position = OnArc(3.5f, 0.4f);
            rope.TryGetGrabPoint(position, 5f, out _);

            for (var i = 0; i < 120; i++)
                rope.StepHeld(Anchor, 3.5f, position, Dt);

            var chord = (position - Anchor).normalized;
            for (var k = 1; k < rope.PinIndex; k++) {
                var offset = rope.Points[k] - Anchor;
                Assert.Less(Mathf.Abs(offset.x * chord.y - offset.y * chord.x), 0.01f);
            }
        }

        [Test]
        public void ClimbingUpAndDown_KeepsTailNearlyStraight() {
            var rope = Create();
            var pendulum = new Pendulum(RopeLengthLimits.Apply(new PendulumTuning {
                minLength = 1f, maxLength = 6f, gravity = 25f, damping = 0.15f, climbSpeed = 2.5f
            }, rope.TotalLength));
            var state = new PendulumState(0.35f, 0f, 3f);
            rope.TryGetGrabPoint(Pendulum.PositionOf(Anchor, state), 5f, out _);
            var worstBend = 0f;

            for (var i = 0; i < 200; i++) {
                var climb = i < 30 ? 0f : i < 100 ? 1f : -1f;
                state = pendulum.Step(state, new PendulumInput(0f, climb), Dt);
                rope.StepHeld(Anchor, state.Length, Pendulum.PositionOf(Anchor, state), Dt);
                if (i < 20)
                    continue;

                var bend = 0f;
                for (var k = rope.PinIndex + 1; k < rope.Points.Count - 1; k++)
                    bend += Mathf.Abs(Vector2.SignedAngle(rope.Points[k] - rope.Points[k - 1], rope.Points[k + 1] - rope.Points[k]));
                worstBend = Mathf.Max(worstBend, bend);
            }

            Assert.Less(worstBend, 12f);
        }

        [Test]
        public void HeldTail_StaysWithinReasonableAngleWhileSwinging() {
            var rope = Create();
            var pendulum = new Pendulum(new PendulumTuning {
                minLength = 1f, maxLength = 4.8f, gravity = 25f, damping = 0.15f, swingForce = 12f,
                softLimitStartDegrees = 40f, softLimitStrength = 4f, maxAngleDegrees = 90f
            });
            var state = new PendulumState(0.2f, 0f, 3f);
            rope.TryGetGrabPoint(Pendulum.PositionOf(Anchor, state), 5f, out _);
            var worst = 0f;

            for (var i = 0; i < 500; i++) {
                state = pendulum.Step(state, new PendulumInput(Mathf.Sign(state.AngularVelocity), 0f), Dt);
                rope.StepHeld(Anchor, state.Length, Pendulum.PositionOf(Anchor, state), Dt);
                if (i < 50)
                    continue;

                var tail = rope.Points[rope.Points.Count - 1] - rope.Points[rope.PinIndex];
                worst = Mathf.Max(worst, Mathf.Abs(Mathf.Atan2(tail.x, -tail.y) * Mathf.Rad2Deg));
            }

            Assert.Less(worst, 75f);
        }
        [Test]
        public void HeldPinIndex_StaysFixedWhileClimbing() {
            var rope = Create();
            rope.StepHeld(Anchor, 2f, Anchor + Vector2.down * 2f, Dt);
            var pin = rope.PinIndex;

            for (var i = 0; i < 80; i++)
                rope.StepHeld(Anchor, 2f + i * 0.04f, Anchor + Vector2.down * (2f + i * 0.04f), Dt);

            Assert.AreEqual(pin, rope.PinIndex);
        }

        [Test]
        public void LongIdle_TailHangsStraightDownAndStops() {
            var rope = Create();
            Swing(rope, 2f, 0.6f);
            rope.Release();

            for (var i = 0; i < 2500; i++)
                rope.Step(Anchor, Dt);

            var tip = rope.Points[rope.Points.Count - 1];
            Assert.AreEqual(Anchor.x, tip.x, 0.05f);
            Assert.AreEqual(Anchor.y - rope.TotalLength, tip.y, 0.15f);
            Assert.Less(rope.PointVelocity(rope.Points.Count - 1).magnitude, 1e-3f);
        }

        [Test]
        public void ClimbingKeepsTotalLengthAndNeverJumpsPoints() {
            var rope = Create();
            var total = rope.TotalLength;
            var pendulum = new Pendulum(RopeLengthLimits.Apply(new PendulumTuning {
                minLength = 1f, maxLength = 6f, gravity = 25f, climbSpeed = 2f
            }, total));
            var state = new PendulumState(0f, 0f, 4f);
            var worstJump = 0f;
            var before = Snapshot(rope);

            for (var i = 0; i < 120; i++) {
                var climb = i < 60 ? 1f : -1f;
                state = pendulum.Step(state, new PendulumInput(0f, climb), Dt);
                rope.StepHeld(Anchor, state.Length, Pendulum.PositionOf(Anchor, state), Dt);

                Assert.AreEqual(total, rope.CurrentLength, 1e-3f);
                for (var k = 0; k < before.Length; k++)
                    worstJump = Mathf.Max(worstJump, Vector2.Distance(before[k], rope.Points[k]));
                before = Snapshot(rope);
            }

            Assert.AreEqual(total, rope.TotalLength, 1e-5f);
            Assert.Less(worstJump, 0.3f);
        }

        [Test]
        public void LengthLimits_ClampMinAndMaxToTotalLength() {
            var tuning = RopeLengthLimits.Apply(new PendulumTuning { minLength = 9f, maxLength = 12f }, 10f);

            Assert.AreEqual(8f, tuning.maxLength, 1e-5f);
            Assert.AreEqual(8f, tuning.minLength, 1e-5f);

            var pendulum = new Pendulum(tuning);
            Assert.AreEqual(8f, pendulum.ClampLength(100f), 1e-5f);
            Assert.AreEqual(8f, pendulum.ClampLength(0f), 1e-5f);
        }

        [Test]
        public void Release_DoesNotSnapPointsInOneTick() {
            var rope = Create();
            SweepRight(rope, 0.6f, 40);

            var before = Snapshot(rope);
            rope.Release();
            rope.Step(Anchor, Dt);

            var worst = 0f;
            for (var i = 0; i < before.Length; i++)
                worst = Mathf.Max(worst, Vector2.Distance(before[i], rope.Points[i]));

            Assert.Less(worst, 0.2f);
        }

        [Test]
        public void Release_KeepsHeldPointVelocity() {
            var rope = Create();
            SweepRight(rope, 0.6f, 40);

            var pin = rope.PinIndex;
            var velocity = rope.PointVelocity(pin);
            var before = rope.Points[pin];
            rope.Release();
            Assert.AreEqual(velocity.x, rope.PointVelocity(pin).x, 1e-6f);
            Assert.AreEqual(velocity.y, rope.PointVelocity(pin).y, 1e-6f);
            rope.Step(Anchor, Dt);
            var moved = rope.Points[pin] - before;

            Assert.Greater(velocity.magnitude, 0.03f);
            Assert.Less(moved.magnitude, velocity.magnitude * 3f);
        }

        [Test]
        public void AfterRelease_AmplitudeShrinksAroundSettleTime() {
            const float settle = 3f;
            var rope = Create(settle);
            Swing(rope, 2f, 0.6f);
            rope.Release();

            var series = new List<float>();
            for (var i = 0; i < Mathf.RoundToInt(settle * 3f / Dt); i++) {
                rope.Step(Anchor, Dt);
                series.Add(TipSwing(rope));
            }

            var initial = Envelope(series, 0f, 1.5f);
            var atSettle = Envelope(series, settle, settle + 1.5f);
            var later = Envelope(series, settle * 2f, settle * 2f + 1.5f);

            Assert.Greater(initial, 0.5f);
            Assert.Less(atSettle, initial * 0.3f);
            Assert.Less(later, initial * 0.1f);
        }

        [Test]
        public void ReGrabAfterRelease_ContinuesFromCurrentRopeState() {
            var rope = Create();
            Swing(rope, 1.5f, 0.6f);
            rope.Release();
            for (var i = 0; i < 19; i++)
                rope.Step(Anchor, Dt);

            var earlier = Snapshot(rope);
            rope.Step(Anchor, Dt);
            var before = Snapshot(rope);
            rope.StepHeld(Anchor, 10f * rope.SegmentLength, rope.Points[10], Dt);

            for (var i = 0; i < before.Length; i++) {
                var carried = Vector2.Distance(earlier[i], before[i]);
                Assert.Less(Vector2.Distance(before[i], rope.Points[i]), carried * 1.5f + 0.1f);
            }
        }
    }
}
