using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class GripTests {
        private const float Ppu = 100f;
        private const float Tolerance = 1e-4f;
        private static readonly Vector2 Hand = new(3f, 7f);
        private static readonly Vector2 HandPixel = new(51f, 21f);
        private static readonly Vector2 FootPixel = new(48f, 54f);
        private static readonly GripLayout Layout = new(Ppu, new Vector2(32f, 64f), HandPixel);

        private static GripTuning Tuning() => new() {
            angleStepDegrees = 7.5f,
            maxAngleDegrees = 60f,
            blendTime = 0.1f,
            footSmoothTime = 0f
        };

        private static GripSample WithFoot(bool flipped, Vector2 pixel) {
            return new GripSample(Layout, flipped, true, pixel);
        }

        [TestCase(-1.2f, false)]
        [TestCase(0f, false)]
        [TestCase(0.37f, true)]
        [TestCase(1.4f, true)]
        public void Hand_stays_on_rope_point_at_any_angle(float angle, bool flipped) {
            var hand = Layout.PointWorld(Hand, HandPixel, flipped, angle);

            Assert.AreEqual(Hand.x, hand.x, Tolerance);
            Assert.AreEqual(Hand.y, hand.y, Tolerance);
        }

        [Test]
        public void Sprite_pivot_orbits_hand_at_fixed_distance() {
            var radius = Layout.HandLocal(false).magnitude;

            foreach (var angle in new[] { -1f, 0f, 0.6f, 1.2f }) {
                var pivot = Layout.PointWorld(Hand, new Vector2(31.5f, 63.5f), false, angle);
                Assert.AreEqual(radius, Vector2.Distance(pivot, Hand), Tolerance);
            }
        }

        [TestCase(0f, 0f)]
        [TestCase(3f, 0f)]
        [TestCase(4f, 7.5f)]
        [TestCase(11f, 7.5f)]
        [TestCase(11.3f, 15f)]
        [TestCase(-11f, -7.5f)]
        [TestCase(-34f, -37.5f)]
        public void Angle_is_rounded_to_step(float degrees, float expected) {
            var snapped = GripAngle.Snap(degrees * Mathf.Deg2Rad, 7.5f, 60f);

            Assert.AreEqual(expected, snapped * Mathf.Rad2Deg, Tolerance);
        }

        [TestCase(80f, 60f)]
        [TestCase(-170f, -60f)]
        [TestCase(61f, 60f)]
        public void Angle_is_limited_to_max(float degrees, float expected) {
            var snapped = GripAngle.Snap(degrees * Mathf.Deg2Rad, 7.5f, 60f);

            Assert.AreEqual(expected, snapped * Mathf.Rad2Deg, Tolerance);
        }

        [Test]
        public void Max_not_multiple_of_step_is_never_exceeded() {
            var snapped = GripAngle.Snap(2f, 7.5f, 50f);

            Assert.LessOrEqual(snapped * Mathf.Rad2Deg, 50f + Tolerance);
        }

        [Test]
        public void Flip_mirrors_hand_offset() {
            var normal = Layout.HandLocal(false);
            var flipped = Layout.HandLocal(true);

            Assert.AreEqual(-normal.x, flipped.x, Tolerance);
            Assert.AreEqual(normal.y, flipped.y, Tolerance);
            Assert.Greater(normal.x, 0f);
        }

        [Test]
        public void Flip_mirrors_foot_position_at_zero_angle() {
            var normal = Layout.PointWorld(Hand, FootPixel, false, 0f) - Hand;
            var flipped = Layout.PointWorld(Hand, FootPixel, true, 0f) - Hand;

            Assert.AreEqual(-normal.x, flipped.x, Tolerance);
            Assert.AreEqual(normal.y, flipped.y, Tolerance);
        }

        [Test]
        public void Foot_at_zero_angle_is_below_hand_by_pixel_difference() {
            var foot = Layout.PointWorld(Hand, FootPixel, false, 0f);

            Assert.AreEqual(Hand.x + (FootPixel.x - HandPixel.x) / Ppu, foot.x, Tolerance);
            Assert.AreEqual(Hand.y - (FootPixel.y - HandPixel.y) / Ppu, foot.y, Tolerance);
        }

        [Test]
        public void Foot_rotates_around_hand_by_angle() {
            var before = Layout.PointWorld(Hand, FootPixel, false, 0f) - Hand;
            var after = Layout.PointWorld(Hand, FootPixel, false, Mathf.PI / 2f) - Hand;

            Assert.AreEqual(-before.y, after.x, Tolerance);
            Assert.AreEqual(before.x, after.y, Tolerance);
            Assert.AreEqual(before.magnitude, after.magnitude, Tolerance);
        }

        [Test]
        public void Positive_angle_swings_feet_toward_positive_x_like_the_pendulum() {
            var tilted = Layout.PointWorld(Hand, new Vector2(51f, 54f), false, 0.5f) - Hand;

            Assert.Greater(tilted.x, 0f);
        }

        [Test]
        public void Solver_reports_foot_from_frame_pixel_and_rotation() {
            var solver = new GripSolver(Tuning());
            var angle = 15f * Mathf.Deg2Rad;

            var result = solver.Solve(angle, Hand, WithFoot(false, FootPixel), 0.02f);

            var expected = Layout.PointWorld(Hand, FootPixel, false, angle);
            Assert.IsTrue(result.HasFoot);
            Assert.AreEqual(expected.x, result.Foot.x, Tolerance);
            Assert.AreEqual(expected.y, result.Foot.y, Tolerance);
            Assert.AreEqual(Vector2.Distance(Hand, expected), result.FootDistance, Tolerance);
        }

        [Test]
        public void Solver_uses_flip_for_foot() {
            var solver = new GripSolver(Tuning());

            var result = solver.Solve(0f, Hand, WithFoot(true, FootPixel), 0.02f);

            var expected = Layout.PointWorld(Hand, FootPixel, true, 0f);
            Assert.AreEqual(expected.x, result.Foot.x, Tolerance);
            Assert.Greater(result.Foot.x, Hand.x);
        }

        [Test]
        public void Solver_reports_snapped_angle() {
            var solver = new GripSolver(Tuning());

            var result = solver.Solve(11f * Mathf.Deg2Rad, Hand, default, 0.02f);

            Assert.AreEqual(7.5f, result.Angle * Mathf.Rad2Deg, Tolerance);
        }

        [Test]
        public void Solver_without_foot_frame_does_not_pin() {
            var solver = new GripSolver(Tuning());

            var result = solver.Solve(0.3f, Hand, default, 0.02f);

            Assert.IsFalse(result.HasFoot);
        }

        [Test]
        public void Solver_foot_follows_hand_without_lag() {
            var tuning = Tuning();
            tuning.footSmoothTime = 0.2f;
            var solver = new GripSolver(tuning);
            solver.Solve(0f, Hand, WithFoot(false, FootPixel), 0.02f);

            var moved = Hand + new Vector2(5f, 0f);
            var result = solver.Solve(0f, moved, WithFoot(false, FootPixel), 0.02f);

            Assert.AreEqual(moved.x + (FootPixel.x - HandPixel.x) / Ppu, result.Foot.x, Tolerance);
        }

        [Test]
        public void Solver_smooths_foot_when_frame_pixel_changes() {
            var tuning = Tuning();
            tuning.footSmoothTime = 0.2f;
            var solver = new GripSolver(tuning);
            var first = solver.Solve(0f, Hand, WithFoot(false, FootPixel), 0.02f);

            var second = solver.Solve(0f, Hand, WithFoot(false, new Vector2(48f, 40f)), 0.02f);
            var target = Layout.PointWorld(Hand, new Vector2(48f, 40f), false, 0f);

            Assert.Greater(second.Foot.y, first.Foot.y);
            Assert.Less(second.Foot.y, target.y);
        }

        [Test]
        public void Blend_returns_to_zero_after_release() {
            var blend = new GripBlend(0.1f);
            for (var i = 0; i < 10; i++)
                blend.Step(true, 0.02f);
            Assert.AreEqual(1f, blend.Weight, Tolerance);
            Assert.AreNotEqual(0f, 40f * Mathf.Deg2Rad * blend.Weight);

            for (var i = 0; i < 5; i++)
                blend.Step(false, 0.02f);

            Assert.AreEqual(0f, 40f * Mathf.Deg2Rad * blend.Weight, Tolerance);
        }

        [Test]
        public void Blend_is_partial_before_duration_ends() {
            var blend = new GripBlend(0.1f);
            blend.Step(true, 0.1f);

            blend.Step(false, 0.05f);

            Assert.AreEqual(0.5f, blend.Weight, Tolerance);
        }

        [Test]
        public void Blend_does_not_move_when_time_is_stopped() {
            var blend = new GripBlend(0.1f);
            blend.Step(true, 0.05f);

            blend.Step(false, 0f);

            Assert.AreEqual(0.5f, blend.Weight, Tolerance);
        }

        [TestCase("Frame_33", true, 33)]
        [TestCase("Frame_0", true, 0)]
        [TestCase("Frame_x", false, -1)]
        [TestCase("Idle", false, -1)]
        [TestCase(null, false, -1)]
        public void Frame_name_is_parsed(string name, bool ok, int expected) {
            Assert.AreEqual(ok, AsepriteFrameName.TryParse(name, out var frame));
            Assert.AreEqual(expected, frame);
        }

        [Test]
        public void Foot_table_returns_pixel_for_known_frames_only() {
            var table = new FootTable(new[] { 33, 34 }, new[] { new Vector2(48f, 54f), new Vector2(47f, 54f) });

            Assert.IsTrue(table.TryGet(34, out var pixel));
            Assert.AreEqual(new Vector2(47f, 54f), pixel);
            Assert.IsFalse(table.TryGet(32, out _));
        }
    }
}
