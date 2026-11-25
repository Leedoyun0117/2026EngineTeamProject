using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
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
}
