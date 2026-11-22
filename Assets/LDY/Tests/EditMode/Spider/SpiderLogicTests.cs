using System;
using NUnit.Framework;

namespace LDY.Tests {
    using LDY.Spider;

    public class TwoBoneIKTests {
        private const float Tolerance = 1e-3f;

        [Test]
        public void ReachableTarget_PlacesTipOnTargetWithCorrectSegmentLengths() {
            var solution = TwoBoneIK.Solve(Vec2.Zero, 3f, 4f, new Vec2(5f, 0f), 1f);

            Assert.IsFalse(solution.Clamped);
            Assert.AreEqual(5f, solution.Tip.X, Tolerance);
            Assert.AreEqual(0f, solution.Tip.Y, Tolerance);
            Assert.AreEqual(3f, solution.Knee.Length, Tolerance);
            Assert.AreEqual(4f, (solution.Tip - solution.Knee).Length, Tolerance);
        }

        [Test]
        public void TooFarTarget_ClampsToFullyExtendedAlongTargetDirection() {
            var solution = TwoBoneIK.Solve(Vec2.Zero, 3f, 4f, new Vec2(0f, 20f), 1f);

            Assert.IsTrue(solution.Clamped);
            Assert.AreEqual(0f, solution.Tip.X, Tolerance);
            Assert.AreEqual(7f, solution.Tip.Y, Tolerance);
        }

        [Test]
        public void TooCloseTarget_ClampsToMinimumReach() {
            var solution = TwoBoneIK.Solve(Vec2.Zero, 5f, 2f, new Vec2(1f, 0f), 1f);

            Assert.IsTrue(solution.Clamped);
            Assert.AreEqual(3f, solution.Tip.Length, Tolerance);
            Assert.AreEqual(5f, solution.Knee.Length, Tolerance);
            Assert.AreEqual(2f, (solution.Tip - solution.Knee).Length, Tolerance);
        }

        [Test]
        public void BendSign_FlipsKneeToOppositeSideOfTargetLine() {
            var counterClockwise = TwoBoneIK.Solve(Vec2.Zero, 3f, 4f, new Vec2(5f, 0f), 1f);
            var clockwise = TwoBoneIK.Solve(Vec2.Zero, 3f, 4f, new Vec2(5f, 0f), -1f);

            Assert.Greater(counterClockwise.Knee.Y, 0f);
            Assert.Less(clockwise.Knee.Y, 0f);
            Assert.AreEqual(counterClockwise.Knee.X, clockwise.Knee.X, Tolerance);
        }

        [Test]
        public void Forward_ReproducesSolvedTip() {
            var solution = TwoBoneIK.Solve(new Vec2(1f, 2f), 3f, 4f, new Vec2(5f, -1f), -1f);
            var tip = TwoBoneIK.Forward(new Vec2(1f, 2f), 3f, 4f, solution.UpperAngle, solution.LowerAngle);

            Assert.AreEqual(solution.Tip.X, tip.X, Tolerance);
            Assert.AreEqual(solution.Tip.Y, tip.Y, Tolerance);
        }
    }

    public class LegPickerTests {
        private static readonly float[] LegX = { -6f, -2f, 2f, 6f };

        [Test]
        public void PicksAliveLegNearestToTarget() {
            var alive = new[] { true, true, true, true };

            Assert.AreEqual(1, LegPicker.Pick(-1f, LegX, alive));
            Assert.AreEqual(3, LegPicker.Pick(10f, LegX, alive));
        }

        [Test]
        public void SkipsCutLegs() {
            var alive = new[] { true, false, true, true };

            Assert.AreEqual(2, LegPicker.Pick(-2f, LegX, alive));
        }

        [Test]
        public void ReturnsMinusOneWhenNoLegAlive() {
            Assert.AreEqual(-1, LegPicker.Pick(0f, LegX, new[] { false, false, false, false }));
        }
    }

    public class SpiderBrainTests {
        private static readonly float[] LegX = { -6f, -2f, 2f, 6f };

        private static SpiderBrain Create(int severs = 2) {
            return new SpiderBrain(new SpiderBrainConfig {
                LegCount = 4,
                AimDuration = 1f,
                TelegraphTime = 1f,
                StuckTime = 2f,
                RetractTime = 1f,
                Cooldown = 1f,
                TargetError = 0f,
                SeversToDefeat = severs,
                FleeTime = 3f
            }, () => 0f);
        }

        private static void ReachStuck(SpiderBrain brain, float playerX) {
            brain.Tick(1f, playerX, LegX);
            brain.Tick(1f, playerX, LegX);
            brain.Tick(1f, playerX, LegX);
            brain.Tick(1f, playerX, LegX);
            brain.TipArrived();
        }

        [Test]
        public void FollowsStateOrderWithElapsedTime() {
            var brain = Create();
            Assert.AreEqual(SpiderState.Idle, brain.State);

            brain.Tick(0.5f, 0f, LegX);
            Assert.AreEqual(SpiderState.Idle, brain.State);
            brain.Tick(0.5f, 0f, LegX);
            Assert.AreEqual(SpiderState.Aim, brain.State);
            brain.Tick(1f, 0f, LegX);
            Assert.AreEqual(SpiderState.Telegraph, brain.State);
            brain.Tick(1f, 0f, LegX);
            Assert.AreEqual(SpiderState.Stab, brain.State);
            brain.Tick(5f, 0f, LegX);
            Assert.AreEqual(SpiderState.Stab, brain.State);

            brain.TipArrived();
            Assert.AreEqual(SpiderState.Stuck, brain.State);
            brain.Tick(2f, 0f, LegX);
            Assert.AreEqual(SpiderState.Retract, brain.State);
            brain.Tick(1f, 0f, LegX);
            Assert.AreEqual(SpiderState.Idle, brain.State);
            brain.Tick(1f, 0f, LegX);
            Assert.AreEqual(SpiderState.Aim, brain.State);
        }

        [Test]
        public void ChoosesNearestLegToPlayer() {
            var brain = Create();
            ReachStuck(brain, 5f);

            Assert.AreEqual(3, brain.ActiveLeg);
            Assert.AreEqual(5f, brain.TargetX, 1e-4f);
        }

        [Test]
        public void SeverDuringStuck_HurtsAndLowersLegCount() {
            var brain = Create();
            ReachStuck(brain, -2f);
            var leg = brain.ActiveLeg;

            Assert.IsTrue(brain.Sever(leg));

            Assert.AreEqual(SpiderState.Hurt, brain.State);
            Assert.AreEqual(1, brain.SeveredCount);
            Assert.IsFalse(brain.IsLegAlive(leg));
        }

        [Test]
        public void SeveredLegIsNotChosenAgain() {
            var brain = Create();
            ReachStuck(brain, -2f);
            brain.Sever(brain.ActiveLeg);
            brain.Tick(1f, -2f, LegX);
            brain.Tick(1f, -2f, LegX);
            brain.Tick(1f, -2f, LegX);

            Assert.AreEqual(SpiderState.Telegraph, brain.State);
            Assert.AreNotEqual(1, brain.ActiveLeg);
        }

        [Test]
        public void ReachingSeverLimit_FleesThenReportsDefeatedOnce() {
            var brain = Create();
            var defeated = 0;
            brain.Defeated += () => defeated++;
            ReachStuck(brain, -2f);
            brain.Sever(brain.ActiveLeg);
            brain.Sever(3);

            Assert.AreEqual(SpiderState.Flee, brain.State);
            brain.Tick(1f, 0f, LegX);
            Assert.AreEqual(0, defeated);
            brain.Tick(2f, 0f, LegX);
            Assert.AreEqual(SpiderState.Gone, brain.State);
            brain.Tick(5f, 0f, LegX);
            Assert.AreEqual(1, defeated);
        }

        [Test]
        public void SeverIgnoredForAlreadyCutLeg() {
            var brain = Create(3);
            Assert.IsTrue(brain.Sever(0));
            Assert.IsFalse(brain.Sever(0));
            Assert.AreEqual(1, brain.SeveredCount);
        }
    }

    public class TrapStateTests {
        [Test]
        public void ArmedPlayerContact_KillsPlayerAndSnaps() {
            var state = new TrapState();

            Assert.AreEqual(TrapOutcome.KillPlayer, state.PlayerTouched());
            Assert.AreEqual(TrapPhase.Snapped, state.Phase);
        }

        [Test]
        public void ArmedNeedleContact_SeversLegAndSnaps() {
            var state = new TrapState();

            Assert.AreEqual(TrapOutcome.SeverLeg, state.NeedleTouched());
            Assert.AreEqual(TrapPhase.Snapped, state.Phase);
        }

        [Test]
        public void SnappedTrapIgnoresEverything() {
            var state = new TrapState();
            state.NeedleTouched();

            Assert.AreEqual(TrapOutcome.None, state.PlayerTouched());
            Assert.AreEqual(TrapOutcome.None, state.NeedleTouched());
            Assert.AreEqual(TrapPhase.Snapped, state.Phase);
        }
    }
}
