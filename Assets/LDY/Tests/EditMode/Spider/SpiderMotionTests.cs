using System.Linq;
using NUnit.Framework;

namespace LDY.Tests {
    using LDY.Spider;

    public class EasingTests {
        [Test]
        public void AllCurvesHitBoundaries() {
            Assert.AreEqual(0f, Easing.Smooth(0f), 1e-6f);
            Assert.AreEqual(1f, Easing.Smooth(1f), 1e-6f);
            Assert.AreEqual(0f, Easing.EaseOut(0f), 1e-6f);
            Assert.AreEqual(1f, Easing.EaseOut(1f), 1e-6f);
            Assert.AreEqual(0f, Easing.EaseIn(0f), 1e-6f);
            Assert.AreEqual(1f, Easing.EaseIn(1f), 1e-6f);
        }

        [Test]
        public void ClampsOutsideRange() {
            Assert.AreEqual(0f, Easing.EaseIn(-1f), 1e-6f);
            Assert.AreEqual(1f, Easing.EaseOut(2f), 1e-6f);
        }

        [Test]
        public void EaseOutIsAheadAndEaseInIsBehindLinear() {
            Assert.Greater(Easing.EaseOut(0.5f), 0.5f);
            Assert.Less(Easing.EaseIn(0.5f), 0.5f);
        }
    }

    public class LegChoreographerTests {
        private const float TargetX = 3f;
        private const float Raise = 6f;

        private sealed class FlatGround : IGroundProbe {
            public bool TryGetGroundY(float x, float fromY, out float y) {
                y = 0f;
                return true;
            }
        }

        private static SpiderBrain NewBrain() {
            return new SpiderBrain(new SpiderBrainConfig {
                LegCount = 4,
                AimDuration = 0f,
                TelegraphTime = 1f,
                StuckTime = 1f,
                RetractTime = 1f,
                Cooldown = 0f,
                TargetError = 0f,
                SeversToDefeat = 2,
                FleeTime = 1f
            }, () => 0f);
        }

        private static LegChoreographer NewChoreographer(SpiderBrain brain) {
            return new LegChoreographer(brain, new FlatGround(), new AttackMotionConfig {
                RaiseHeight = Raise,
                RaiseTime = 0.8f,
                StabTime = 0.1f,
                RetractLiftShare = 0.4f,
                RetractRise = 3f,
                RetractArcHeight = 1f
            });
        }

        private static readonly float[] LegX = { -6f, -2f, 2f, 6f };
        private static readonly Vec2 Home = new Vec2(2f, 0f);
        private const float Dt = 0.05f;

        private static Vec2 Step(SpiderBrain brain, LegChoreographer choreographer) {
            brain.Tick(Dt, TargetX, LegX);
            choreographer.TryStep(Dt, Home, Home, out var tip);
            return tip;
        }

        [Test]
        public void TelegraphEndsDirectlyAboveTargetX() {
            var brain = NewBrain();
            var choreographer = NewChoreographer(brain);
            var last = Home;

            for (var i = 0; i < 200 && brain.State != SpiderState.Stab; i++) {
                var tip = Step(brain, choreographer);
                if (brain.State == SpiderState.Telegraph)
                    last = tip;
            }

            Assert.AreEqual(SpiderState.Stab, brain.State);
            Assert.AreEqual(TargetX, last.X, 1e-4f);
            Assert.AreEqual(Raise, last.Y, 1e-4f);
        }

        [Test]
        public void StabKeepsXFixedAndEndsAtGround() {
            var brain = NewBrain();
            var choreographer = NewChoreographer(brain);
            while (brain.State != SpiderState.Stab)
                Step(brain, choreographer);

            var tips = new System.Collections.Generic.List<Vec2>();
            for (var i = 0; i < 100 && brain.State == SpiderState.Stab; i++)
                tips.Add(Step(brain, choreographer));

            Assert.AreEqual(SpiderState.Stuck, brain.State);
            Assert.IsTrue(tips.All(t => System.Math.Abs(t.X - TargetX) < 1e-5f));
            Assert.AreEqual(0f, tips.Last().Y, 1e-5f);
            Assert.AreEqual(0f, choreographer.TargetPoint.Y, 1e-5f);
        }

        [Test]
        public void StabFallsMonotonicallyWithoutJumpingBackUp() {
            var brain = NewBrain();
            var choreographer = NewChoreographer(brain);
            while (brain.State != SpiderState.Stab)
                Step(brain, choreographer);

            var previous = Raise;
            while (brain.State == SpiderState.Stab) {
                var tip = Step(brain, choreographer);
                Assert.LessOrEqual(tip.Y, previous + 1e-5f);
                previous = tip.Y;
            }
        }
    }

    public class FootPlanterTests {
        private static FootPlanter Create(float threshold = 1.5f) {
            var planter = new FootPlanter(4, threshold, 0.3f, 0.5f);
            planter.Initialize(new[] { new Vec2(0f, 0f), new Vec2(2f, 0f), new Vec2(4f, 0f), new Vec2(6f, 0f) });
            return planter;
        }

        private static Vec2[] Desired(params float[] x) => x.Select(v => new Vec2(v, 0f)).ToArray();

        private static int Lifted(FootPlanter planter) {
            return planter.Phases.Count(p => p == LegPhase.Stepping || p == LegPhase.Busy);
        }

        [Test]
        public void BelowThreshold_NoLegSteps() {
            var planter = Create();
            planter.Step(0.02f, Desired(1f, 3f, 5f, 7f));

            Assert.AreEqual(0, Lifted(planter));
        }

        [Test]
        public void AboveThreshold_OnlyThatLegSteps() {
            var planter = Create();
            planter.Step(0.02f, Desired(0f, 4.5f, 4f, 6f));

            Assert.AreEqual(LegPhase.Stepping, planter.PhaseOf(1));
            Assert.AreEqual(LegPhase.Planted, planter.PhaseOf(0));
            Assert.AreEqual(LegPhase.Planted, planter.PhaseOf(2));
            Assert.AreEqual(LegPhase.Planted, planter.PhaseOf(3));
        }

        [Test]
        public void SameGroupLegsNeverLiftTogether() {
            var planter = Create();
            var desired = Desired(3f, 2f, 7f, 6f);

            planter.Step(0.02f, desired);
            Assert.AreEqual(1, new[] { 0, 2 }.Count(i => planter.PhaseOf(i) == LegPhase.Stepping));

            planter.Step(0.02f, desired);
            Assert.AreEqual(1, new[] { 0, 2 }.Count(i => planter.PhaseOf(i) == LegPhase.Stepping));
        }

        [Test]
        public void SteppingLegLandsOnDesiredAndSecondLegThenFollows() {
            var planter = Create();
            var desired = Desired(3f, 2f, 7f, 6f);
            for (var i = 0; i < 40; i++)
                planter.Step(0.05f, desired);

            Assert.AreEqual(3f, planter.Planted(0).X, 1e-4f);
            Assert.AreEqual(7f, planter.Planted(2).X, 1e-4f);
            Assert.AreEqual(0, Lifted(planter));
        }

        [Test]
        public void StepArcRisesAndHasNoJump() {
            var planter = Create();
            var desired = Desired(0f, 4.5f, 4f, 6f);
            planter.Step(0.02f, desired);
            var previous = planter.Current(1);
            var peak = 0f;
            for (var i = 0; i < 30; i++) {
                planter.Step(0.01f, desired);
                var now = planter.Current(1);
                Assert.Less((now - previous).Length, 0.5f);
                peak = System.Math.Max(peak, now.Y);
                previous = now;
            }

            Assert.Greater(peak, 0.3f);
        }

        [Test]
        public void BusyLegBlocksItsGroupOnly() {
            var planter = Create();
            planter.SetBusy(0);
            planter.Step(0.02f, Desired(0f, 2f, 7f, 9f));

            Assert.AreEqual(LegPhase.Busy, planter.PhaseOf(0));
            Assert.AreEqual(LegPhase.Planted, planter.PhaseOf(2));
            Assert.AreEqual(LegPhase.Stepping, planter.PhaseOf(3));
        }

        [Test]
        public void PickStepper_PrefersLargestOvershootAndSkipsLiftedGroups() {
            var planted = new[] { LegPhase.Planted, LegPhase.Planted, LegPhase.Planted, LegPhase.Planted };
            Assert.AreEqual(0, FootPlanter.PickStepper(new[] { 3f, 0f, 2f, 0f }, planted, 1.5f));

            var lifted = new[] { LegPhase.Stepping, LegPhase.Planted, LegPhase.Planted, LegPhase.Planted };
            Assert.AreEqual(-1, FootPlanter.PickStepper(new[] { 0f, 0f, 5f, 0f }, lifted, 1.5f));
        }

        [Test]
        public void CutLegIsIgnored() {
            var planter = Create();
            planter.Cut(1);
            planter.Step(0.02f, Desired(0f, 9f, 4f, 6f));

            Assert.AreEqual(LegPhase.Cut, planter.PhaseOf(1));
        }
    }

    public class BodyMotionTests {
        [Test]
        public void TrackerReachesTargetWithoutExceedingMaxSpeed() {
            var tracker = new BodyTracker(0f, 2f, 4f);
            var peak = 0f;
            for (var i = 0; i < 600; i++) {
                tracker.Step(0.02f, 10f, false);
                peak = System.Math.Max(peak, System.Math.Abs(tracker.Velocity));
            }

            Assert.LessOrEqual(peak, 2f + 1e-4f);
            Assert.AreEqual(10f, tracker.X, 0.05f);
        }

        [Test]
        public void ImpactBounceDipsThenReturnsToZero() {
            var bounce = new ImpactBounce(0.4f, 0.2f);
            bounce.Trigger();
            var dip = 0f;
            var last = 1f;
            for (var i = 0; i < 20; i++) {
                last = bounce.Step(0.02f);
                dip = System.Math.Min(dip, last);
            }

            Assert.Less(dip, -0.3f);
            Assert.AreEqual(0f, last, 1e-3f);
        }
    }
}
