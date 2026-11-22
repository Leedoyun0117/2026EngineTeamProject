using NUnit.Framework;

namespace LDY.Tests {
    using LDY.Spider;

    public class AttackRangeRuleTests {
        private static readonly LegReach[] Reach = {
            new LegReach(3f, 8f), new LegReach(1.5f, 7f), new LegReach(1.5f, 7f), new LegReach(3f, 8f)
        };

        private static readonly bool[] AllAlive = { true, true, true, true };

        private static float[] Legs(float bodyX) => new[] { bodyX - 0.55f, bodyX - 0.2f, bodyX + 0.2f, bodyX + 0.55f };

        private static int Pick(float target, bool[] alive = null, float bodyX = 0f) {
            return AttackRangeRule.Pick(target, bodyX, Legs(bodyX), alive ?? AllAlive, Reach);
        }

        [Test]
        public void RightTargetUsesOnlyRightLegsAndLeftTargetOnlyLeftLegs() {
            Assert.AreEqual(3, Pick(5f));
            Assert.AreEqual(0, Pick(-5f));
            Assert.AreEqual(2, Pick(2f));
            Assert.AreEqual(1, Pick(-2f));
        }

        [Test]
        public void InsideMinimumRadiusNoLegReaches() {
            Assert.AreEqual(-1, Pick(0.5f));
            Assert.AreEqual(-1, Pick(-1.4f));
            Assert.AreEqual(1, Pick(-1.6f));
        }

        [Test]
        public void BeyondMaximumRadiusNoLegReaches() {
            Assert.AreEqual(-1, Pick(8.1f));
            Assert.AreEqual(3, Pick(7.9f));
        }

        [Test]
        public void OuterLegMinimumLeavesInnerLegForNearTargets() {
            Assert.AreEqual(2, Pick(2.9f));
        }

        [Test]
        public void DeadLegsAreSkipped() {
            Assert.AreEqual(2, Pick(5f, new[] { true, true, true, false }));
            Assert.AreEqual(-1, Pick(5f, new[] { true, true, false, false }));
        }

        [Test]
        public void StandoffPutsTargetBesideBodyOnTheTargetsSide() {
            var goal = AttackRangeRule.StandoffBodyX(0.3f, 0f, Legs(0f), AllAlive, 4.5f, default);
            Assert.AreEqual(-4.2f, goal, 1e-4f);
        }

        [Test]
        public void StandoffApproachesFromOppositeSideWhenThatSideHasNoLiveLeg() {
            var goal = AttackRangeRule.StandoffBodyX(0.3f, 0f, Legs(0f), new[] { true, true, false, false }, 4.5f,
                default);
            Assert.AreEqual(4.8f, goal, 1e-4f);
        }

        [Test]
        public void StandoffIsClampedToCombatZone() {
            var goal = AttackRangeRule.StandoffBodyX(0.3f, 0f, Legs(0f), AllAlive, 4.5f, new CombatZone(-3f, 3f));
            Assert.AreEqual(-3f, goal, 1e-4f);
        }
    }

    public class CombatZoneTests {
        [Test]
        public void ClampsInsideBoundsAndPassesThroughWithoutBounds() {
            var zone = new CombatZone(-5f, 5f);
            Assert.AreEqual(5f, zone.Clamp(9f));
            Assert.AreEqual(-5f, zone.Clamp(-9f));
            Assert.AreEqual(1f, zone.Clamp(1f));
            Assert.AreEqual(9f, default(CombatZone).Clamp(9f));
        }

        [Test]
        public void SwappedBoundsStillWork() {
            Assert.AreEqual(5f, new CombatZone(5f, -5f).Clamp(9f));
        }
    }

    public class SpiderHurtBrainTests {
        private static readonly LegReach[] Reach = {
            new LegReach(3f, 8f), new LegReach(1.5f, 7f), new LegReach(1.5f, 7f), new LegReach(3f, 8f)
        };

        private static float[] Legs(float bodyX) => new[] { bodyX - 0.55f, bodyX - 0.2f, bodyX + 0.2f, bodyX + 0.55f };

        private static SpiderBrain Create(CombatZone zone = default, bool useReach = true, int severs = 2) {
            return new SpiderBrain(new SpiderBrainConfig {
                LegCount = 4,
                AimDuration = 1f,
                TelegraphTime = 1f,
                StuckTime = 2f,
                RetractTime = 1f,
                Cooldown = 1f,
                TargetError = 0f,
                SeversToDefeat = severs,
                FleeTime = 3f,
                HurtTime = 1f,
                HurtCooldown = 3f,
                Reach = useReach ? Reach : null,
                Standoff = 4.5f,
                ArriveTolerance = 0.3f,
                Zone = zone
            }, () => 0f);
        }

        private static void Step(SpiderBrain brain, float dt, float playerX, float bodyX = 0f) {
            brain.Tick(dt, playerX, bodyX, Legs(bodyX));
        }

        private static void ReachStuck(SpiderBrain brain, float playerX) {
            for (var i = 0; i < 4; i++)
                Step(brain, 1f, playerX);

            brain.TipArrived();
        }

        [Test]
        public void PlayerUnderBodyRepositionsInsteadOfStabbing() {
            var brain = Create();
            Step(brain, 1f, 0.5f);
            Assert.AreEqual(SpiderState.Aim, brain.State);
            Step(brain, 0.1f, 0.5f);
            Assert.AreEqual(SpiderState.Reposition, brain.State);
            Step(brain, 0.1f, 0.5f);
            Assert.AreEqual(-4f, brain.BodyGoalX, 1e-4f);
            Assert.AreEqual(SpiderState.Reposition, brain.State);
        }

        [Test]
        public void RepositionEndsInAimOnceBodyArrivesInRange() {
            var brain = Create();
            Step(brain, 1f, 0.5f);
            Step(brain, 0.1f, 0.5f);
            Step(brain, 0.1f, 0.5f);

            Step(brain, 0.1f, 0.5f, -4f);

            Assert.AreEqual(SpiderState.Aim, brain.State);
            Step(brain, 1f, 0.5f, -4f);
            Assert.AreEqual(SpiderState.Telegraph, brain.State);
            Assert.AreEqual(3, brain.ActiveLeg);
        }

        [Test]
        public void PlayerOutsideZoneLeavesBodyWaitingAtZoneEnd() {
            var brain = Create(new CombatZone(-1f, 1f));
            Step(brain, 1f, 0.2f);
            Step(brain, 0.1f, 0.2f);
            Step(brain, 0.1f, 0.2f);
            Assert.AreEqual(-1f, brain.BodyGoalX, 1e-4f);

            Step(brain, 0.1f, 0.2f, -1f);

            Assert.AreEqual(SpiderState.Idle, brain.State);
        }

        [Test]
        public void SeverEntersHurtThenLongCooldownThenAim() {
            var brain = Create(useReach: false);
            ReachStuck(brain, -2f);

            Assert.IsTrue(brain.Sever(brain.ActiveLeg));
            Assert.AreEqual(SpiderState.Hurt, brain.State);

            Step(brain, 1f, -2f);
            Assert.AreEqual(SpiderState.Idle, brain.State);
            Step(brain, 2.5f, -2f);
            Assert.AreEqual(SpiderState.Idle, brain.State);
            Step(brain, 0.5f, -2f);
            Assert.AreEqual(SpiderState.Aim, brain.State);
        }

        [Test]
        public void NormalCooldownReturnsAfterHurtCycle() {
            var brain = Create(useReach: false, severs: 3);
            ReachStuck(brain, -2f);
            brain.Sever(brain.ActiveLeg);
            Step(brain, 1f, -2f);
            Step(brain, 3f, -2f);
            Step(brain, 1f, -2f);
            Step(brain, 1f, -2f);
            Step(brain, 1f, -2f);
            brain.TipArrived();
            Step(brain, 2f, -2f);
            Step(brain, 1f, -2f);
            Assert.AreEqual(SpiderState.Idle, brain.State);

            Step(brain, 1f, -2f);

            Assert.AreEqual(SpiderState.Aim, brain.State);
        }

        [Test]
        public void ReachingSeverCountFleesAndReportsDefeatedExactlyOnce() {
            var brain = Create(useReach: false);
            var defeated = 0;
            brain.Defeated += () => defeated++;
            ReachStuck(brain, -2f);
            brain.Sever(brain.ActiveLeg);
            Step(brain, 1f, -2f);
            Step(brain, 3f, -2f);
            Step(brain, 1f, -2f);
            Step(brain, 1f, -2f);
            Step(brain, 1f, -2f);
            brain.TipArrived();

            brain.Sever(brain.ActiveLeg);
            Assert.AreEqual(SpiderState.Flee, brain.State);
            Assert.AreEqual(0, defeated);
            Step(brain, 3f, 0f);
            Step(brain, 5f, 0f);
            Step(brain, 5f, 0f);

            Assert.AreEqual(SpiderState.Gone, brain.State);
            Assert.AreEqual(1, defeated);
        }
    }

    public class HurtMotionTests {
        [Test]
        public void PulseStartsAndEndsAtZeroAndPeaksAtOne() {
            Assert.AreEqual(0f, Easing.Pulse(0f), 1e-6f);
            Assert.AreEqual(1f, Easing.Pulse(0.3f), 1e-6f);
            Assert.AreEqual(0f, Easing.Pulse(1f), 1e-6f);
        }

        [Test]
        public void FlailIsZeroAtBothEndsOfHurt() {
            Assert.AreEqual(0f, LegFlail.Degrees(0f, 0.3f, 14f, 7f, 0f), 1e-4f);
            Assert.AreEqual(0f, LegFlail.Degrees(1f, 0.3f, 14f, 7f, 0f), 1e-3f);
        }

        [Test]
        public void FlailRotationKeepsDistanceToSocket() {
            var socket = new Vec2(1f, 5f);
            var tip = new Vec2(4f, 0f);
            var rotated = LegFlail.Rotate(tip, socket, 20f);

            Assert.AreEqual((tip - socket).Length, (rotated - socket).Length, 1e-4f);
            Assert.Greater(System.Math.Abs(tip.X - rotated.X), 1e-3f);
        }

        [Test]
        public void TuckStartsAtPlantedAndEndsAtTuckPoint() {
            var planted = new Vec2(6f, 0f);
            var hang = new Vec2(0f, 4f);

            Assert.AreEqual(6f, LegTuck.Tip(planted, hang, 0f).X, 1e-5f);
            Assert.AreEqual(4f, LegTuck.Tip(planted, hang, 1f).Y, 1e-5f);
            Assert.AreEqual(1f, LegTuck.Progress(0.5f, 0.35f));
            Assert.AreEqual(0.5f, LegTuck.Progress(0.175f, 0.35f), 1e-4f);
        }

        [Test]
        public void FaceFollowsStateAndGlitch() {
            Assert.AreEqual(BodyFace.Idle, BodyFaceRule.Pick(SpiderState.Idle, 0f, false, true, 0.1f));
            Assert.AreEqual(BodyFace.Glitch, BodyFaceRule.Pick(SpiderState.Idle, 0f, true, false, 0.1f));
            Assert.AreEqual(BodyFace.HurtA, BodyFaceRule.Pick(SpiderState.Hurt, 0.05f, false, false, 0.1f));
            Assert.AreEqual(BodyFace.HurtB, BodyFaceRule.Pick(SpiderState.Hurt, 0.15f, false, false, 0.1f));
            Assert.AreEqual(BodyFace.Glitch, BodyFaceRule.Pick(SpiderState.Flee, 0.5f, false, true, 0.1f));
            Assert.AreEqual(BodyFace.Idle, BodyFaceRule.Pick(SpiderState.Flee, 0.5f, true, false, 0.1f));
        }

        [Test]
        public void StanceKeepsOriginalWhenAllAliveAndSpreadsLiveLegsOverSameSpanWhenOneIsCut() {
            var original = new[] { -6f, -2f, 2f, 6f };
            var result = new float[4];

            StanceLayout.Spread(original, new[] { true, true, true, true }, result);
            CollectionAssert.AreEqual(original, result);

            StanceLayout.Spread(original, new[] { true, false, true, true }, result);
            Assert.AreEqual(-6f, result[0], 1e-4f);
            Assert.AreEqual(0f, result[2], 1e-4f);
            Assert.AreEqual(6f, result[3], 1e-4f);
            Assert.IsTrue(result[0] < result[2] && result[2] < result[3]);
        }

        [Test]
        public void StanceWithSingleLegStandsInTheMiddle() {
            var result = new float[4];
            StanceLayout.Spread(new[] { -6f, -2f, 2f, 6f }, new[] { false, false, true, false }, result);

            Assert.AreEqual(0f, result[2], 1e-4f);
        }
    }
}
