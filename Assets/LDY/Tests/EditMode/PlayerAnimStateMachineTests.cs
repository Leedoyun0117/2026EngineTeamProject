using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class PlayerAnimStateMachineTests {
        private const float Dt = 0.016f;

        private PlayerAnimRules _rules;
        private PlayerAnimStateMachine _machine;

        [SetUp]
        public void SetUp() {
            _rules = new PlayerAnimRules { airVelocityMargin = 0.5f, groundLeaveGrace = 0.08f, sitDelay = 5f };
            _machine = new PlayerAnimStateMachine(_rules);
        }

        private PlayerAnimId Tick(float vx = 0f, float vy = 0f, bool grounded = true, float dt = Dt,
            bool grabbing = false, RowMoveDirection row = RowMoveDirection.None, bool finished = false, int frame = 0) {
            return _machine.Tick(new PlayerAnimInput(new Vector2(vx, vy), grounded, false, dt, finished, frame,
                grabbing, row));
        }

        private void Air(float vy, int ticks = 1) {
            for (var i = 0; i < ticks; i++)
                Tick(vy: vy, grounded: false);
        }

        [Test]
        public void RisingFastEntersJumpImmediately() {
            Assert.AreEqual(PlayerAnimId.Jump, Tick(vy: 8f, grounded: false));
        }

        [Test]
        public void FallingEntersFallAfterGrace() {
            Air(-3f, 3);
            Assert.AreEqual(PlayerAnimId.Idle, _machine.Current);
            Air(-3f, 3);
            Assert.AreEqual(PlayerAnimId.Fall, _machine.Current);
        }

        [Test]
        public void NearApexKeepsJumpWithoutFlicker() {
            Tick(vy: 8f, grounded: false);
            var changes = 0;
            foreach (var vy in new[] { 2f, 0.4f, -0.3f, 0.2f, -0.45f, 0.1f, 0f }) {
                Tick(vy: vy, grounded: false);
                if (_machine.Changed)
                    changes++;
            }

            Assert.AreEqual(0, changes);
            Assert.AreEqual(PlayerAnimId.Jump, _machine.Current);
        }

        [Test]
        public void PastApexSwitchesToFallAndStaysThroughMargin() {
            Tick(vy: 8f, grounded: false);
            Air(-0.6f);
            Assert.AreEqual(PlayerAnimId.Fall, _machine.Current);
            Air(0.3f);
            Air(-0.2f);
            Assert.AreEqual(PlayerAnimId.Fall, _machine.Current);
        }

        [Test]
        public void ShortDropIsIgnored() {
            Air(-1f, 3);
            Assert.AreEqual(PlayerAnimId.Idle, _machine.Current);
            Tick(grounded: true);
            Assert.AreEqual(PlayerAnimId.Idle, _machine.Current);
            Assert.IsFalse(_machine.Changed);
        }

        [Test]
        public void ShortDropWhileWalkingKeepsWalk() {
            Tick(vx: 2f);
            Assert.AreEqual(PlayerAnimId.Walk, _machine.Current);
            Tick(vx: 2f, vy: -1f, grounded: false);
            Tick(vx: 2f, vy: -1f, grounded: false);
            Assert.AreEqual(PlayerAnimId.Walk, _machine.Current);
        }

        [Test]
        public void LandingReturnsToGroundStateImmediately() {
            Air(-3f, 10);
            Assert.AreEqual(PlayerAnimId.Fall, _machine.Current);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
            Tick(vx: 2f, vy: -3f, grounded: false);
            Air(-3f, 10);
            Assert.AreEqual(PlayerAnimId.Walk, Tick(vx: 2f));
        }

        [Test]
        public void LandingWhileRunningReturnsToRun() {
            Air(-3f, 10);
            Assert.AreEqual(PlayerAnimId.Run, Tick(vx: 5f));
        }

        [Test]
        public void GrabBeatsAirborne() {
            Assert.AreEqual(PlayerAnimId.Grab, Tick(vy: 8f, grounded: false, grabbing: true));
        }

        [Test]
        public void GrabBeatsRowMove() {
            Assert.AreEqual(PlayerAnimId.Grab, Tick(grabbing: true, row: RowMoveDirection.Up));
        }

        [Test]
        public void GrabBeatsSit() {
            for (var i = 0; i < 400; i++)
                Tick(dt: 0.02f);
            Assert.AreEqual(PlayerAnimId.SitDown, _machine.Current);
            Assert.AreEqual(PlayerAnimId.Grab, Tick(grabbing: true));
        }

        [Test]
        public void ReleasingGrabReturnsToGround() {
            Tick(grabbing: true);
            Assert.AreEqual(PlayerAnimId.Idle, Tick());
        }

        [Test]
        public void ReleasingGrabInAirGoesToFall() {
            Tick(grabbing: true, grounded: false);
            Air(-1f, 6);
            Assert.AreEqual(PlayerAnimId.Fall, _machine.Current);
        }

        [Test]
        public void JumpReleasedEarlyDoesNotDropToGroundMidAir() {
            Tick(vy: 8f, grounded: false);
            Air(0f, 2);
            Assert.AreEqual(PlayerAnimId.Jump, _machine.Current);
        }

        [Test]
        public void RowMoveUpIsJump() {
            Assert.AreEqual(PlayerAnimId.Jump, Tick(row: RowMoveDirection.Up));
        }

        [Test]
        public void RowMoveDownIsFall() {
            Assert.AreEqual(PlayerAnimId.Fall, Tick(row: RowMoveDirection.Down));
        }

        [Test]
        public void RowMoveEndReturnsToGroundAfterGrace() {
            Tick(row: RowMoveDirection.Up);
            Assert.AreEqual(PlayerAnimId.Idle, Tick(grounded: false));
            Assert.AreEqual(PlayerAnimId.Idle, Tick(grounded: true));
        }

        [Test]
        public void SitInterruptedByAirborne() {
            for (var i = 0; i < 400; i++)
                Tick(dt: 0.02f);
            Assert.AreEqual(PlayerAnimId.SitDown, _machine.Current);
            Assert.AreEqual(PlayerAnimId.Jump, Tick(vy: 8f, grounded: false));
        }
    }
}
