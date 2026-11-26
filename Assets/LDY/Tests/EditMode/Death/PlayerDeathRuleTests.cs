using System.Linq;
using LDY.Death;
using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class DeathGateTests {
        [Test]
        public void SecondDieIsIgnored() {
            var gate = new DeathGate();

            Assert.IsTrue(gate.TryBegin());
            Assert.IsFalse(gate.TryBegin());
            Assert.AreEqual(DeathPhase.Dying, gate.Phase);
        }

        [Test]
        public void RestartIsRequestedOnlyOnceAndOnlyWhileDying() {
            var gate = new DeathGate();
            Assert.IsFalse(gate.TryRequestRestart());

            gate.TryBegin();
            Assert.IsTrue(gate.TryRequestRestart());
            Assert.IsFalse(gate.TryRequestRestart());
            Assert.IsFalse(gate.TryBegin());
        }
    }

    public class DeathClipOrderTests {
        [Test]
        public void TrapPlaysTrappedThenDeath() {
            CollectionAssert.AreEqual(new[] { PlayerAnimId.Trapped, PlayerAnimId.Death },
                DeathClipOrder.For(DeathCause.Trap).ToArray());
        }

        [TestCase(DeathCause.Hazard)]
        [TestCase(DeathCause.Fall)]
        [TestCase(DeathCause.Enemy)]
        [TestCase(DeathCause.Unknown)]
        public void OtherCausesPlayDeathOnly(DeathCause cause) {
            CollectionAssert.AreEqual(new[] { PlayerAnimId.Death }, DeathClipOrder.For(cause).ToArray());
        }

        [Test]
        public void PuppetPlaysDeathOnly() {
            CollectionAssert.AreEqual(new[] { PlayerAnimId.Death }, DeathClipOrder.Puppet.ToArray());
        }
    }

    public class DestroyJudgeTests {
        [Test]
        public void IntendedDeathAfterRestartRequestIsIgnored() {
            Assert.AreEqual(DestroyVerdict.Ignore, DestroyJudge.Judge(DeathPhase.RestartRequested, false, false));
        }

        [Test]
        public void UnintendedDestroyOfLivingPlayerSpawnsPuppet() {
            Assert.AreEqual(DestroyVerdict.SpawnPuppet, DestroyJudge.Judge(DeathPhase.Alive, false, false));
        }

        [Test]
        public void DestroyDuringDeathBeforeRestartSpawnsPuppetSoRestartIsNotLost() {
            Assert.AreEqual(DestroyVerdict.SpawnPuppet, DestroyJudge.Judge(DeathPhase.Dying, false, false));
        }

        [TestCase(DeathPhase.Alive)]
        [TestCase(DeathPhase.Dying)]
        [TestCase(DeathPhase.RestartRequested)]
        public void SceneUnloadIsIgnored(DeathPhase phase) {
            Assert.AreEqual(DestroyVerdict.Ignore, DestroyJudge.Judge(phase, false, true));
        }

        [TestCase(DeathPhase.Alive)]
        [TestCase(DeathPhase.Dying)]
        public void QuittingIsIgnored(DeathPhase phase) {
            Assert.AreEqual(DestroyVerdict.Ignore, DestroyJudge.Judge(phase, true, false));
        }
    }

    public class DeathTimelineTests {
        private const float Eps = 1e-3f;

        [Test]
        public void RestartOnlyAfterAllClipsPlusDelay() {
            var timeline = new DeathTimeline(new[] { 0.5f, 0.6f }, 0.3f);

            timeline.Tick(1.1f - Eps);
            Assert.IsFalse(timeline.RestartDue);
            timeline.Tick(0.3f);
            Assert.IsFalse(timeline.RestartDue);
            timeline.Tick(2f * Eps);
            Assert.IsTrue(timeline.RestartDue);
        }

        [Test]
        public void RestartIsSignalledOnce() {
            var timeline = new DeathTimeline(new[] { 0.2f }, 0.3f);

            timeline.Tick(1f);
            Assert.IsTrue(timeline.RestartDue);
            timeline.Tick(1f);
            Assert.IsFalse(timeline.RestartDue);
        }

        [Test]
        public void ClipsAdvanceInOrderAndHoldTheLast() {
            var timeline = new DeathTimeline(new[] { 0.5f, 0.6f }, 0.3f);
            Assert.AreEqual(0, timeline.ClipIndex);

            timeline.Tick(0.4f);
            Assert.AreEqual(0, timeline.ClipIndex);
            Assert.IsFalse(timeline.ClipChanged);

            timeline.Tick(0.2f);
            Assert.AreEqual(1, timeline.ClipIndex);
            Assert.IsTrue(timeline.ClipChanged);

            timeline.Tick(0.6f);
            Assert.AreEqual(1, timeline.ClipIndex);
            Assert.IsFalse(timeline.ClipChanged);
        }

        [Test]
        public void NoClipsStillRestartsAfterDelay() {
            var timeline = new DeathTimeline(new float[0], 0.3f);
            Assert.AreEqual(-1, timeline.ClipIndex);

            timeline.Tick(0.3f - Eps);
            Assert.IsFalse(timeline.RestartDue);
            timeline.Tick(2f * Eps);
            Assert.IsTrue(timeline.RestartDue);
        }
    }

    public class DeathPullTests {
        [Test]
        public void OnlyTrapWithAnchorPulls() {
            Assert.IsTrue(DeathPull.Applies(DeathCause.Trap, 3f));
            Assert.IsFalse(DeathPull.Applies(DeathCause.Trap, null));
            Assert.IsFalse(DeathPull.Applies(DeathCause.Enemy, 3f));
            Assert.IsFalse(DeathPull.Applies(DeathCause.Hazard, 3f));
        }

        [Test]
        public void FollowsSmoothStepAndEndsAtTarget() {
            var pull = new DeathPull(0f, 8f, 0.25f);

            Assert.AreEqual(8f * 0.15625f, pull.Tick(0.0625f), 1e-4f);
            Assert.IsFalse(pull.Finished);
            Assert.AreEqual(4f, pull.Tick(0.0625f), 1e-4f);
            Assert.AreEqual(8f, pull.Tick(0.125f), 1e-4f);
            Assert.IsTrue(pull.Finished);
            Assert.AreEqual(8f, pull.Tick(1f), 1e-4f);
        }

        [Test]
        public void ZeroDurationSnapsToTarget() {
            var pull = new DeathPull(2f, 9f, 0f);

            Assert.AreEqual(9f, pull.Tick(0f), 1e-4f);
            Assert.IsTrue(pull.Finished);
        }
    }

    public class PixelScaleTests {
        [Test]
        public void LowPpuClipIsScaledDownToShownPixelSize() {
            Assert.AreEqual(0.4f, PixelScale.Ratio(100f, 40f), 1e-5f);
        }

        [Test]
        public void SamePpuKeepsScale() {
            Assert.AreEqual(1f, PixelScale.Ratio(40f, 40f), 1e-5f);
        }

        [TestCase(0f, 40f)]
        [TestCase(100f, 0f)]
        public void UnknownPpuKeepsScale(float shown, float clip) {
            Assert.AreEqual(1f, PixelScale.Ratio(shown, clip), 1e-5f);
        }
    }
}
