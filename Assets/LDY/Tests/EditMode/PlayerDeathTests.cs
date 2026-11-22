using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class PosePriorityTests {
        [Test]
        public void DeathBeatsTrappedBeatsAlt() {
            Assert.AreEqual(PoseLayer.Death, PosePriority.Resolve(true, true, true));
            Assert.AreEqual(PoseLayer.Death, PosePriority.Resolve(true, false, true));
            Assert.AreEqual(PoseLayer.Trapped, PosePriority.Resolve(false, true, true));
            Assert.AreEqual(PoseLayer.Alt, PosePriority.Resolve(false, false, true));
            Assert.AreEqual(PoseLayer.Normal, PosePriority.Resolve(false, false, false));
        }
    }

    public class PlayerDeathAnimStateTests {
        private PlayerAnimStateMachine _machine;

        [SetUp]
        public void SetUp() {
            _machine = new PlayerAnimStateMachine(new PlayerAnimRules());
        }

        private PlayerAnimId Tick(bool trapped = false, bool dead = false, bool grabbing = false, bool rope = false) {
            return _machine.Tick(new PlayerAnimInput(Vector2.zero, true, false, 0.016f, false, 0, grabbing,
                RowMoveDirection.None, rope, false, trapped, dead));
        }

        [Test]
        public void DeathBeatsTrapped() {
            Assert.AreEqual(PlayerAnimId.Death, Tick(trapped: true, dead: true));
        }

        [Test]
        public void TrappedBeatsGrabAndRope() {
            Assert.AreEqual(PlayerAnimId.Trapped, Tick(trapped: true, grabbing: true, rope: true));
        }

        [Test]
        public void TrappedThenDeathChangesState() {
            Tick(trapped: true);
            Assert.AreEqual(PlayerAnimId.Trapped, _machine.Current);
            Tick(dead: true);
            Assert.AreEqual(PlayerAnimId.Death, _machine.Current);
            Assert.IsTrue(_machine.Changed);
        }

        [Test]
        public void MissingDeathClipFallsBackToTrappedThenIdle() {
            Assert.IsTrue(PlayerAnimFallback.TryNext(PlayerAnimId.Death, out var next));
            Assert.AreEqual(PlayerAnimId.Trapped, next);
            Assert.IsTrue(PlayerAnimFallback.TryNext(PlayerAnimId.Trapped, out next));
            Assert.AreEqual(PlayerAnimId.Idle, next);
        }
    }

    public class DeathSequenceTests {
        private static readonly DeathTimings Timings = new DeathTimings {
            PullTime = 0.2f,
            TrappedTime = 0.5f,
            TailTime = 0.2f,
            DeathTime = 0.6f,
            EndHold = 0.1f
        };

        [Test]
        public void TrapFlowsPullTrappedTailDeathDone() {
            var sequence = new DeathSequence();
            sequence.BeginTrapped(Timings);
            Assert.AreEqual(DeathStage.Pull, sequence.Stage);

            sequence.Tick(0.1f);
            Assert.AreEqual(0.5f, sequence.PullProgress, 1e-4f);
            sequence.Tick(0.15f);
            Assert.AreEqual(DeathStage.Trapped, sequence.Stage);
            Assert.IsTrue(sequence.IsTrapped);
            sequence.Tick(0.5f);
            Assert.AreEqual(DeathStage.TrappedTail, sequence.Stage);
            sequence.Tick(0.2f);
            Assert.AreEqual(DeathStage.Death, sequence.Stage);
            Assert.IsTrue(sequence.IsDead);
            Assert.IsFalse(sequence.Finished);
            sequence.Tick(0.7f);
            Assert.AreEqual(DeathStage.Done, sequence.Stage);
            Assert.IsTrue(sequence.Finished);

            sequence.Tick(1f);
            Assert.IsFalse(sequence.Finished);
        }

        [Test]
        public void NeedleSkipsPullAndTrapped() {
            var sequence = new DeathSequence();
            sequence.BeginNeedle(Timings);

            Assert.AreEqual(DeathStage.Death, sequence.Stage);
            Assert.IsFalse(sequence.IsTrapped);
            sequence.Tick(0.3f);
            Assert.AreEqual(DeathStage.Death, sequence.Stage);
            sequence.Tick(0.5f);
            Assert.IsTrue(sequence.Finished);
        }

        [Test]
        public void ZeroLengthClipsStillFinishOnce() {
            var sequence = new DeathSequence();
            sequence.BeginTrapped(default);
            sequence.Tick(0f);

            Assert.AreEqual(DeathStage.Done, sequence.Stage);
            Assert.IsTrue(sequence.Finished);
        }
    }
}
