using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class PoseSourceSetTests {
        private class FakePose : IGrabStateSource, IRowMoveSource {
            public bool IsGrabbing { get; set; }
            public RowMoveDirection RowMove { get; set; }
        }

        [Test]
        public void MissingSourcesFallBackToNoPose() {
            var set = new PoseSourceSet(null, null);
            Assert.IsFalse(set.IsGrabbing);
            Assert.AreEqual(RowMoveDirection.None, set.RowMove);
        }

        [Test]
        public void DefaultStructBehavesLikeNoPose() {
            var set = default(PoseSourceSet);
            Assert.IsFalse(set.IsGrabbing);
            Assert.AreEqual(RowMoveDirection.None, set.RowMove);
        }

        [Test]
        public void ProvidedSourcesAreRead() {
            var fake = new FakePose { IsGrabbing = true, RowMove = RowMoveDirection.Up };
            var set = new PoseSourceSet(fake, fake);
            Assert.IsTrue(set.IsGrabbing);
            Assert.AreEqual(RowMoveDirection.Up, set.RowMove);
        }

        [Test]
        public void MachineRunsWithoutPoseSources() {
            var machine = new PlayerAnimStateMachine(new PlayerAnimRules());
            var set = new PoseSourceSet(null, null);
            var input = new PlayerAnimInput(new Vector2(3f, 0f), true, false, 0.016f, false, 0,
                set.IsGrabbing, set.RowMove);

            Assert.DoesNotThrow(() => machine.Tick(input));
            Assert.AreNotEqual(PlayerAnimId.Grab, machine.Current);
        }

        [Test]
        public void GrabSourceDrivesGrabState() {
            var machine = new PlayerAnimStateMachine(new PlayerAnimRules());
            var set = new PoseSourceSet(new FakePose { IsGrabbing = true }, null);
            var input = new PlayerAnimInput(Vector2.zero, true, false, 0.016f, false, 0, set.IsGrabbing, set.RowMove);

            Assert.AreEqual(PlayerAnimId.Grab, machine.Tick(input));
        }
    }
}
