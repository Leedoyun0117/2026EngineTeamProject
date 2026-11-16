using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class GripFacingTests {
        [Test]
        public void Root_facing_right_is_not_mirrored() {
            Assert.IsFalse(GripFacing.IsRootMirrored(1f));
        }

        [Test]
        public void Root_rotated_180_on_y_is_mirrored() {
            Assert.IsTrue(GripFacing.IsRootMirrored(-1f));
        }

        [Test]
        public void Local_angle_is_unchanged_when_not_mirrored() {
            Assert.AreEqual(0.4f, GripFacing.LocalAngle(0.4f, false));
        }

        [Test]
        public void Local_angle_is_negated_when_mirrored() {
            Assert.AreEqual(-0.4f, GripFacing.LocalAngle(0.4f, true));
        }

        [Test]
        public void Mirrored_root_without_sprite_flip_is_world_flipped() {
            Assert.IsTrue(GripFacing.IsWorldFlipped(false, true));
        }

        [Test]
        public void Sprite_flip_on_mirrored_root_cancels_out() {
            Assert.IsFalse(GripFacing.IsWorldFlipped(true, true));
        }

        [Test]
        public void Unmirrored_root_keeps_sprite_flip() {
            Assert.IsTrue(GripFacing.IsWorldFlipped(true, false));
        }
    }
}
