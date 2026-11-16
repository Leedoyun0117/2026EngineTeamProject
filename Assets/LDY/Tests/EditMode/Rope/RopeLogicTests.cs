using System.Collections.Generic;
using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class PendulumTests {
        private static PendulumTuning Tuning(float damping = 0f, float swing = 10f) => new() {
            minLength = 1f,
            maxLength = 5f,
            gravity = 10f,
            damping = damping,
            swingForce = swing,
            climbSpeed = 2f
        };

        [Test]
        public void Step_AdvancesOneTick() {
            var pendulum = new Pendulum(Tuning());
            var next = pendulum.Step(new PendulumState(0.5f, 0f, 2f), new PendulumInput(0f, 0f), 0.1f);

            var expectedOmega = -(10f / 2f) * Mathf.Sin(0.5f) * 0.1f;
            Assert.AreEqual(expectedOmega, next.AngularVelocity, 1e-5f);
            Assert.AreEqual(0.5f + expectedOmega * 0.1f, next.Angle, 1e-5f);
            Assert.AreEqual(2f, next.Length, 1e-5f);
        }

        [Test]
        public void Step_DampingBringsPendulumToRest() {
            var pendulum = new Pendulum(Tuning(1f));
            var state = new PendulumState(0.8f, 0f, 2f);

            for (var i = 0; i < 1500; i++)
                state = pendulum.Step(state, new PendulumInput(0f, 0f), 0.02f);

            Assert.Less(Mathf.Abs(state.Angle), 1e-3f);
            Assert.Less(Mathf.Abs(state.AngularVelocity), 1e-3f);
        }

        [Test]
        public void Step_SwingInputPushesInInputDirection() {
            var pendulum = new Pendulum(Tuning());
            var right = pendulum.Step(new PendulumState(0f, 0f, 2f), new PendulumInput(1f, 0f), 0.1f);
            var left = pendulum.Step(new PendulumState(0f, 0f, 2f), new PendulumInput(-1f, 0f), 0.1f);

            Assert.Greater(right.AngularVelocity, 0f);
            Assert.Less(left.AngularVelocity, 0f);
        }

        [Test]
        public void Step_ClimbClampsToMinLength() {
            var pendulum = new Pendulum(Tuning());
            var state = new PendulumState(0f, 0f, 2f);

            for (var i = 0; i < 200; i++)
                state = pendulum.Step(state, new PendulumInput(0f, 1f), 0.05f);

            Assert.AreEqual(1f, state.Length, 1e-5f);
        }

        [Test]
        public void Step_DescendClampsToMaxLength() {
            var pendulum = new Pendulum(Tuning());
            var state = new PendulumState(0f, 0f, 2f);

            for (var i = 0; i < 200; i++)
                state = pendulum.Step(state, new PendulumInput(0f, -1f), 0.05f);

            Assert.AreEqual(5f, state.Length, 1e-5f);
        }

        [Test]
        public void CreateState_ClampsLengthAndKeepsTangentSpeed() {
            var pendulum = new Pendulum(Tuning());
            var state = pendulum.CreateState(Vector2.zero, new Vector2(0f, -9f), new Vector2(3f, 0f));

            Assert.AreEqual(5f, state.Length, 1e-5f);
            Assert.AreEqual(0f, state.Angle, 1e-5f);
            Assert.AreEqual(3f, Pendulum.TangentVelocity(state).x, 1e-4f);
        }

        [Test]
        public void PositionOf_HangsBelowAnchorAtZeroAngle() {
            var position = Pendulum.PositionOf(new Vector2(1f, 5f), new PendulumState(0f, 0f, 2f));

            Assert.AreEqual(1f, position.x, 1e-5f);
            Assert.AreEqual(3f, position.y, 1e-5f);
        }

        [Test]
        public void Step_MaxAngleLimitsSwingAndStopsOutwardSpeed() {
            var tuning = new PendulumTuning {
                minLength = 1f, maxLength = 5f, gravity = 10f, swingForce = 30f, maxAngleDegrees = 90f
            };
            var pendulum = new Pendulum(tuning);
            var state = new PendulumState(0f, 0f, 3f);

            for (var i = 0; i < 400; i++) {
                state = pendulum.Step(state, new PendulumInput(1f, 0f), 0.02f);
                Assert.LessOrEqual(Mathf.Abs(state.Angle), Mathf.PI / 2f + 1e-4f);
            }

            Assert.Greater(state.Angle, 1f);
        }


        [Test]
        public void Step_SoftLimitKeepsSwingBelowHardLimitAndSlowsGradually() {
            PendulumTuning Tuning(float strength) => new() {
                minLength = 1f, maxLength = 5f, gravity = 25f, swingForce = 12f,
                softLimitStartDegrees = 45f, softLimitStrength = strength, maxAngleDegrees = 140f
            };

            float MaxAngle(float strength) {
                var pendulum = new Pendulum(Tuning(strength));
                var state = new PendulumState(0f, 0f, 4.5f);
                var worst = 0f;
                for (var i = 0; i < 1500; i++) {
                    state = pendulum.Step(state, new PendulumInput(1f, 0f), 0.02f);
                    worst = Mathf.Max(worst, state.Angle);
                }

                return worst;
            }

            var soft = MaxAngle(6f);
            var none = MaxAngle(0f);

            Assert.Less(soft, 140f * Mathf.Deg2Rad - 0.05f);
            Assert.Less(soft, none);
        }

        [Test]
        public void SoftLimitPull_GrowsWithAngleAndPointsBack() {
            var pendulum = new Pendulum(new PendulumTuning {
                minLength = 1f, maxLength = 5f, gravity = 25f,
                softLimitStartDegrees = 45f, softLimitStrength = 6f, maxAngleDegrees = 140f
            });
            var low = pendulum.Step(new PendulumState(50f * Mathf.Deg2Rad, 0f, 3f), default, 0.02f).AngularVelocity;
            var high = pendulum.Step(new PendulumState(120f * Mathf.Deg2Rad, 0f, 3f), default, 0.02f).AngularVelocity;

            Assert.Less(low, 0f);
            Assert.Less(high, low);
        }

        [Test]
        public void Step_ResonantPushNeverExceedsNinetyDegrees() {
            var pendulum = new Pendulum(new PendulumTuning {
                minLength = 1f, maxLength = 4.8f, gravity = 25f, damping = 0.15f, swingForce = 12f,
                softLimitStartDegrees = 40f, softLimitStrength = 4f, maxAngleDegrees = 90f
            });
            var state = new PendulumState(0.2f, 0f, 3.5f);
            var worst = 0f;

            for (var i = 0; i < 3000; i++) {
                state = pendulum.Step(state, new PendulumInput(Mathf.Sign(state.AngularVelocity), 0f), 0.02f);
                worst = Mathf.Max(worst, Mathf.Abs(state.Angle));
            }

            Assert.Less(worst, 90f * Mathf.Deg2Rad - 1e-3f);
        }

        [Test]
        public void Step_QuadraticDampingCapsResonantSpeed() {
            float Peak(float quadratic) {
                var pendulum = new Pendulum(new PendulumTuning {
                    minLength = 1f, maxLength = 4.8f, gravity = 25f, damping = 0.15f, quadraticDamping = quadratic,
                    swingForce = 9f, softLimitStartDegrees = 40f, softLimitStrength = 4f, maxAngleDegrees = 90f
                });
                var state = new PendulumState(0f, 0f, 3.5f);
                var peak = 0f;
                for (var i = 0; i < 1500; i++) {
                    state = pendulum.Step(state, new PendulumInput(i < 3 ? 1f : Mathf.Sign(state.AngularVelocity), 0f), 0.02f);
                    peak = Mathf.Max(peak, Mathf.Abs(state.AngularVelocity) * state.Length);
                }

                return peak;
            }

            Assert.Less(Peak(0.25f), 11f);
            Assert.Less(Peak(0.25f), Peak(0f) * 0.7f);
        }
    }

    public class ClimbSwayTests {
        private static ClimbSway Create() => new(new ClimbSwayTuning { strength = 0.3f, frequency = 1.2f, rampTime = 0.25f });

        [Test]
        public void NotClimbing_ProducesNoSway() {
            var sway = Create();

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(0f, sway.Step(0f, 0.02f), 1e-6f);
        }

        [Test]
        public void Climbing_SwaysWithinStrengthAndChangesSign() {
            var sway = Create();
            float min = 0f, max = 0f;

            for (var i = 0; i < 200; i++) {
                var value = sway.Step(1f, 0.02f);
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            Assert.LessOrEqual(max, 0.3f + 1e-4f);
            Assert.GreaterOrEqual(min, -0.3f - 1e-4f);
            Assert.Greater(max, 0.2f);
            Assert.Less(min, -0.2f);
        }

        [Test]
        public void Climbing_RampsUpSmoothlyInsteadOfStartingAtFullStrength() {
            var sway = Create();
            var first = 0f;

            for (var i = 0; i < 5; i++)
                first = Mathf.Max(first, Mathf.Abs(sway.Step(1f, 0.02f)));

            Assert.Less(first, 0.1f);
        }

        [Test]
        public void ClimbingRopeSwingStaysSmall() {
            var pendulum = new Pendulum(new PendulumTuning {
                minLength = 1f, maxLength = 4.8f, gravity = 25f, damping = 0.15f, quadraticDamping = 0.25f,
                swingForce = 9f, climbSpeed = 2.5f, softLimitStartDegrees = 40f, softLimitStrength = 4f, maxAngleDegrees = 90f
            });
            var sway = Create();
            var state = new PendulumState(0f, 0f, 4.5f);
            var worst = 0f;

            for (var i = 0; i < 125; i++) {
                state = pendulum.Step(state, new PendulumInput(sway.Step(1f, 0.02f), 1f), 0.02f);
                worst = Mathf.Max(worst, Mathf.Abs(state.Angle));
            }

            Assert.Greater(worst, 2f * Mathf.Deg2Rad);
            Assert.Less(worst, 12f * Mathf.Deg2Rad);
        }
        [Test]
        public void Step_ZeroMaxAngleMeansUnlimited() {
            var pendulum = new Pendulum(new PendulumTuning {
                minLength = 1f, maxLength = 5f, gravity = 10f, swingForce = 0f, maxAngleDegrees = 0f
            });
            var state = pendulum.Step(new PendulumState(3f, 2f, 3f), new PendulumInput(0f, 0f), 0.1f);

            Assert.Greater(Mathf.Abs(state.Angle), 3f);
        }
    }

    public class ReleaseVelocityTests {
        [Test]
        public void AtBottom_MovingRight_ReleasesRight() {
            var velocity = ReleaseVelocity.Compute(new PendulumState(0f, 2f, 3f), false, 6f);

            Assert.AreEqual(6f, velocity.x, 1e-4f);
            Assert.AreEqual(0f, velocity.y, 1e-4f);
        }

        [Test]
        public void AtRightSide_SwingingUp_ReleasesUpward() {
            var velocity = ReleaseVelocity.Compute(new PendulumState(Mathf.PI / 2f, 2f, 3f), false, 6f);

            Assert.AreEqual(0f, velocity.x, 1e-4f);
            Assert.AreEqual(6f, velocity.y, 1e-4f);
        }

        [Test]
        public void JumpAddsUpwardBonusAndKeepsTangent() {
            var velocity = ReleaseVelocity.Compute(new PendulumState(0f, -2f, 3f), true, 6f);

            Assert.AreEqual(-6f, velocity.x, 1e-4f);
            Assert.AreEqual(6f, velocity.y, 1e-4f);
        }
    }

    public class RopeReachTests {
        [Test]
        public void ClosestPoint_ProjectsOntoSegment() {
            var point = RopeReach.ClosestPoint(Vector2.zero, new Vector2(0f, -6f), new Vector2(1f, -2f));

            Assert.AreEqual(0f, point.x, 1e-5f);
            Assert.AreEqual(-2f, point.y, 1e-5f);
        }

        [Test]
        public void ClosestPoint_ClampsToEnds() {
            var point = RopeReach.ClosestPoint(Vector2.zero, new Vector2(0f, -6f), new Vector2(0f, -10f));

            Assert.AreEqual(-6f, point.y, 1e-5f);
        }
    }

    public class HoldSlotTests {
        [Test]
        public void OnlyOneOwnerAtATime() {
            var slot = new HoldSlot();
            var a = new object();
            var b = new object();

            Assert.IsTrue(slot.TryAcquire(a, 0f));
            Assert.IsFalse(slot.TryAcquire(b, 0f));
        }

        [Test]
        public void CooldownBlocksRegrabUntilElapsed() {
            var slot = new HoldSlot();
            var a = new object();
            slot.TryAcquire(a, 0f);
            slot.Release(a, 1f, 0.2f);

            Assert.IsFalse(slot.TryAcquire(a, 1.1f));
            Assert.IsTrue(slot.TryAcquire(a, 1.2f));
        }

        [Test]
        public void ReleaseByNonOwnerIsIgnored() {
            var slot = new HoldSlot();
            var a = new object();
            slot.TryAcquire(a, 0f);
            slot.Release(new object(), 1f, 0.2f);

            Assert.IsFalse(slot.TryAcquire(new object(), 5f));
        }
    }

    public class SuspendRequestHubTests {
        private sealed class RecordingSink : ISuspendSink {
            public readonly List<bool> Calls = new();

            public void SetSuspended(bool suspended) {
                Calls.Add(suspended);
            }
        }

        [Test]
        public void SingleOwner_SuspendsAndResumes() {
            var sink = new RecordingSink();
            var hub = new SuspendRequestHub(sink);
            var owner = new object();

            hub.Request(owner);
            hub.Release(owner);

            CollectionAssert.AreEqual(new[] { true, false }, sink.Calls);
            Assert.IsFalse(hub.IsSuspended);
        }

        [Test]
        public void AltAndRope_OverlapKeepsSuspendedUntilBothRelease() {
            var sink = new RecordingSink();
            var hub = new SuspendRequestHub(sink);
            var rope = new object();
            var alt = new object();

            hub.Request(rope);
            hub.Request(alt);
            hub.Release(alt);

            Assert.IsTrue(hub.IsSuspended);
            CollectionAssert.AreEqual(new[] { true }, sink.Calls);

            hub.Release(rope);

            CollectionAssert.AreEqual(new[] { true, false }, sink.Calls);
        }

        [Test]
        public void DuplicateRequestAndDoubleRelease_DoNotBreakState() {
            var sink = new RecordingSink();
            var hub = new SuspendRequestHub(sink);
            var owner = new object();
            var other = new object();

            hub.Request(owner);
            hub.Request(owner);
            hub.Release(owner);
            hub.Release(owner);
            hub.Release(other);

            CollectionAssert.AreEqual(new[] { true, false }, sink.Calls);
            Assert.IsFalse(hub.IsSuspended);
        }

        [Test]
        public void ReleaseWithoutRequest_DoesNothing() {
            var sink = new RecordingSink();
            var hub = new SuspendRequestHub(sink);

            hub.Release(new object());

            Assert.IsEmpty(sink.Calls);
        }
    }
}
