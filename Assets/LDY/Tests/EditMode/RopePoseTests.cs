using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class RopePoseTests {
        private const float Dt = 0.016f;

        private PlayerAnimStateMachine _machine;

        [SetUp]
        public void SetUp() {
            _machine = new PlayerAnimStateMachine(new PlayerAnimRules());
        }

        private PlayerAnimId Tick(bool rope = false, RowMoveDirection row = RowMoveDirection.None,
            bool grabbing = false) {
            return _machine.Tick(new PlayerAnimInput(Vector2.zero, true, false, Dt, false, 0, grabbing, row, rope));
        }

        [Test]
        public void UpInputWhileHangingGivesRopeClimb() {
            Assert.AreEqual(PlayerAnimId.RopeClimb, RopePose.For(RowMoveDirection.Up));
        }

        [Test]
        public void DownInputWhileHangingGivesRopeClimbDown() {
            Assert.AreEqual(PlayerAnimId.RopeClimbDown, RopePose.For(RowMoveDirection.Down));
        }

        [Test]
        public void NoInputWhileHangingGivesRopeHang() {
            Assert.AreEqual(PlayerAnimId.RopeHang, RopePose.For(RowMoveDirection.None));
        }

        [Test]
        public void RopeIdsAreRecognizedAsRope() {
            Assert.IsTrue(RopePose.IsRope(PlayerAnimId.RopeClimbDown));
        }

        [Test]
        public void GrabIsNotRope() {
            Assert.IsFalse(RopePose.IsRope(PlayerAnimId.Grab));
        }

        [Test]
        public void RopeStatesFallBackToGrab() {
            Assert.AreEqual(PlayerAnimId.Grab, RopePose.FallbackFor(PlayerAnimId.RopeClimb));
        }

        [Test]
        public void NonRopeStatesHaveNoFallback() {
            Assert.AreEqual(PlayerAnimId.Walk, RopePose.FallbackFor(PlayerAnimId.Walk));
        }

        [Test]
        public void MachineEntersRopeHangWhenHangingWithoutInput() {
            Assert.AreEqual(PlayerAnimId.RopeHang, Tick(true));
        }

        [Test]
        public void MachineEntersRopeClimbWhenHangingWithUpInput() {
            Assert.AreEqual(PlayerAnimId.RopeClimb, Tick(true, RowMoveDirection.Up));
        }

        [Test]
        public void MachineEntersRopeClimbDownWhenHangingWithDownInput() {
            Assert.AreEqual(PlayerAnimId.RopeClimbDown, Tick(true, RowMoveDirection.Down));
        }

        [Test]
        public void MachineSwitchesFromClimbBackToHangWhenInputStops() {
            Tick(true, RowMoveDirection.Up);
            Assert.AreEqual(PlayerAnimId.RopeHang, Tick(true));
        }

        [Test]
        public void MachineLeavesRopeStateWhenReleased() {
            Tick(true, RowMoveDirection.Up);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }

        [Test]
        public void RopeTakesPriorityOverGrab() {
            Assert.AreEqual(PlayerAnimId.RopeHang, Tick(true, grabbing: true));
        }

        [Test]
        public void SliderGrabStillGivesGrabWithoutRope() {
            Assert.AreEqual(PlayerAnimId.Grab, Tick(grabbing: true, row: RowMoveDirection.Up));
        }

        [Test]
        public void MissingRopeSourceMeansNotHanging() {
            Assert.IsFalse(new PoseSourceSet(null, null).IsRopeHanging);
        }
    }
}
