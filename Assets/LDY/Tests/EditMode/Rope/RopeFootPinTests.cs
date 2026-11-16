using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class RopeFootPinTests {
        private const float Dt = 0.02f;
        private const float HoldDistance = 3f;
        private const float FootDistance = 0.9f;
        private static readonly Vector2 Anchor = new(0f, 10f);

        private static RopeSimulation Create() {
            var curve = new RopeCurveTuning {
                segmentCount = 20,
                totalLength = 8f,
                gravity = 100f,
                iterations = 100,
                holdBlendTime = 0.15f,
                sleepSpeed = 0.002f,
                tailStiffness = 0.5f
            };
            var damping = new RopeDampingTuning {
                heldDamping = 1.5f,
                heldTailDamping = 8f,
                releaseDamping = 0.3f,
                settleTime = 3f,
                releaseBlendTime = 0.25f
            };
            var rope = new RopeSimulation(curve, damping);
            rope.Initialize(Anchor);
            return rope;
        }

        private static Vector2 Hand(float angle) {
            return Anchor + HoldDistance * new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle));
        }

        private static FootPin Foot(Vector2 hand, float angle) {
            var target = hand + FootDistance * new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle));
            return new FootPin(target, FootDistance);
        }

        private static void Hold(RopeSimulation rope, int steps, float angle) {
            var hand = Hand(angle);
            for (var i = 0; i < steps; i++)
                rope.StepHeld(Anchor, HoldDistance, hand, Dt, Foot(hand, angle));
        }

        [Test]
        public void Hand_and_foot_points_sit_on_their_targets() {
            var rope = Create();
            Hold(rope, 120, 0.3f);

            var hand = Hand(0.3f);
            var foot = Foot(hand, 0.3f);
            Assert.Greater(rope.FootIndex, rope.PinIndex);
            Assert.Less((rope.Points[rope.PinIndex] - hand).magnitude, 1e-4f);
            Assert.Less((rope.Points[rope.FootIndex] - foot.Target).magnitude, 1e-4f);
        }

        [Test]
        public void Segments_between_hand_and_foot_keep_their_length() {
            var rope = Create();
            Hold(rope, 150, 0.3f);

            var span = rope.FootIndex - rope.PinIndex;
            var expected = FootDistance / span;
            for (var i = rope.PinIndex; i < rope.FootIndex; i++)
                Assert.AreEqual(expected, Vector2.Distance(rope.Points[i], rope.Points[i + 1]), 0.02f);
        }

        [Test]
        public void Segments_above_hand_keep_their_length() {
            var rope = Create();
            Hold(rope, 150, 0.3f);

            var expected = HoldDistance / rope.PinIndex;
            for (var i = 0; i < rope.PinIndex; i++)
                Assert.AreEqual(expected, Vector2.Distance(rope.Points[i], rope.Points[i + 1]), 0.02f);
        }

        [Test]
        public void Tail_below_foot_stays_free() {
            var rope = Create();
            Hold(rope, 150, 0f);
            var last = rope.Points.Count - 1;
            var settled = rope.Points[last];

            Hold(rope, 5, 0.8f);

            Assert.Less(rope.FootIndex, last);
            Assert.Greater(Vector2.Distance(settled, rope.Points[last]), 0.01f);
        }

        [Test]
        public void Tail_hangs_below_foot() {
            var rope = Create();
            Hold(rope, 200, 0f);

            Assert.Less(rope.Points[rope.Points.Count - 1].y, rope.Points[rope.FootIndex].y);
        }

        [Test]
        public void Release_clears_both_pins() {
            var rope = Create();
            Hold(rope, 60, 0.3f);

            rope.Release();

            Assert.AreEqual(-1, rope.PinIndex);
            Assert.AreEqual(-1, rope.FootIndex);
        }

        [Test]
        public void Rope_settles_freely_after_release() {
            var rope = Create();
            Hold(rope, 60, 0.5f);
            rope.Release();
            var tip = rope.Points[rope.Points.Count - 1];

            for (var i = 0; i < 100; i++)
                rope.Step(Anchor, Dt);

            Assert.Greater(Vector2.Distance(tip, rope.Points[rope.Points.Count - 1]), 0.1f);
        }

        [Test]
        public void No_foot_pin_when_not_requested() {
            var rope = Create();
            var hand = Hand(0.2f);
            for (var i = 0; i < 30; i++)
                rope.StepHeld(Anchor, HoldDistance, hand, Dt);

            Assert.AreEqual(-1, rope.FootIndex);
        }

        [Test]
        public void Foot_target_change_does_not_break_hand_pin() {
            var rope = Create();
            var hand = Hand(0.2f);
            Hold(rope, 60, 0.2f);

            for (var i = 0; i < 20; i++)
                rope.StepHeld(Anchor, HoldDistance, hand, Dt,
                    new FootPin(hand + new Vector2(0.05f * i, -FootDistance), FootDistance));

            Assert.Less((rope.Points[rope.PinIndex] - hand).magnitude, 1e-4f);
        }
    }
}
