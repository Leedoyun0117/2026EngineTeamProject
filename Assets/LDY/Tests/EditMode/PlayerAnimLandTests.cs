using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class PlayerAnimLandTests {
        private const float Dt = 0.016f;

        private PlayerAnimStateMachine _machine;

        [SetUp]
        public void SetUp() {
            _machine = new PlayerAnimStateMachine(new PlayerAnimRules());
        }

        private PlayerAnimId Tick(float vx = 0f, float vy = 0f, bool grounded = true, bool finished = false,
            bool grabbing = false, RowMoveDirection row = RowMoveDirection.None, bool rope = false,
            bool landAvailable = true) {
            return _machine.Tick(new PlayerAnimInput(new Vector2(vx, vy), grounded, false, Dt, finished, 0,
                grabbing, row, rope, landAvailable));
        }

        private void Fall(int ticks = 10) {
            for (var i = 0; i < ticks; i++)
                Tick(vy: -3f, grounded: false);
        }

        [Test]
        public void LandingWhileStandingStillEntersLand() {
            Fall();
            Assert.AreEqual(PlayerAnimId.Land, Tick());
        }

        [Test]
        public void LandingAfterJumpEntersLand() {
            Tick(vy: 8f, grounded: false);
            Assert.AreEqual(PlayerAnimId.Land, Tick());
        }

        [Test]
        public void LandStaysUntilClipFinished() {
            Fall();
            Tick();
            Assert.AreEqual(PlayerAnimId.Land, Tick());
            Assert.AreEqual(PlayerAnimId.Idle, Tick(finished: true));
        }

        [Test]
        public void MovingWhileLandedSkipsLandAndWalks() {
            Fall();
            Assert.AreEqual(PlayerAnimId.Walk, Tick(vx: 2f));
        }

        [Test]
        public void MovingWhileLandedSkipsLandAndRuns() {
            Fall();
            Assert.AreEqual(PlayerAnimId.Run, Tick(vx: 5f));
        }

        [Test]
        public void MovingInputDuringLandSwitchesToWalk() {
            Fall();
            Tick();
            Assert.AreEqual(PlayerAnimId.Walk, Tick(vx: 2f));
        }

        [Test]
        public void MovingInputDuringLandSwitchesToRun() {
            Fall();
            Tick();
            Assert.AreEqual(PlayerAnimId.Run, Tick(vx: 5f));
        }

        [Test]
        public void LeavingGroundDuringLandReturnsToAir() {
            Fall();
            Tick();
            Assert.AreEqual(PlayerAnimId.Jump, Tick(vy: 8f, grounded: false));
        }

        [Test]
        public void LandIsNotUsedWhenSetHasNoLandClip() {
            Fall();
            Assert.AreEqual(PlayerAnimId.Idle, Tick(landAvailable: false));
        }

        [Test]
        public void ShortDropDoesNotEnterLand() {
            for (var i = 0; i < 3; i++)
                Tick(vy: -1f, grounded: false);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }

        [Test]
        public void RowMoveEndDoesNotEnterLand() {
            Tick(row: RowMoveDirection.Down);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }

        [Test]
        public void GrabBeatsLand() {
            Fall();
            Assert.AreEqual(PlayerAnimId.Grab, Tick(grabbing: true));
        }

        [Test]
        public void RopeBeatsLand() {
            Fall();
            Assert.AreEqual(PlayerAnimId.RopeHang, Tick(rope: true));
        }

        [Test]
        public void ReleasingGrabOnGroundAfterAirDoesNotLand() {
            Fall();
            Tick(grabbing: true);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }

        [Test]
        public void LandOnlyFiresOncePerLanding() {
            Fall();
            Tick();
            Tick(finished: true);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }
    }
}
