using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class RootScaleConversionTests {
        [Test]
        public void Convert_Root2To1_DoublesLocalValues() {
            var converted = RootScaleConversion.Convert(new Float2(0.43611437f, 0.5359341f), 2f, 1f);

            Assert.AreEqual(0.87222874f, converted.X, 1e-5f);
            Assert.AreEqual(1.0718682f, converted.Y, 1e-5f);
        }

        [Test]
        public void Convert_ColliderOffset_KeepsSignAndWorldPosition() {
            var local = new Float2(-0.004542321f, 0.2637189f);

            var converted = RootScaleConversion.Convert(local, 2f, 1f);

            Assert.AreEqual(-0.009084642f, converted.X, 1e-6f);
            Assert.AreEqual(0.5274378f, converted.Y, 1e-6f);
            Assert.IsTrue(RootScaleConversion.SameWorld(RootScaleConversion.ToWorld(local, 2f),
                RootScaleConversion.ToWorld(converted, 1f)));
        }

        [Test]
        public void Convert_JJBLevelGroundCheck_LandsJustUnderColliderBottom() {
            var converted = RootScaleConversion.Convert(new Float2(0f, PlayerParts.JibinGroundCheckLocalY), 2f, 1f);

            Assert.AreEqual(-0.13f, converted.Y, 1e-6f);
        }

        [Test]
        public void SameWorld_DifferenceOfOneHundredth_IsRejected() {
            Assert.IsTrue(RootScaleConversion.SameWorld(new Float2(1f, 1f), new Float2(1.009f, 1f)));
            Assert.IsFalse(RootScaleConversion.SameWorld(new Float2(1f, 1f), new Float2(1.011f, 1f)));
            Assert.IsFalse(RootScaleConversion.SameWorld(new Float2(0f, 0f), new Float2(0f, -0.02f)));
        }

        [Test]
        public void ToWorld_UnscaledCopyOfRoot2Value_IsHalfSize() {
            var copiedRaw = RootScaleConversion.ToWorld(new Float2(0.436f, 0.536f), 1f);
            var original = RootScaleConversion.ToWorld(new Float2(0.436f, 0.536f), 2f);

            Assert.IsFalse(RootScaleConversion.SameWorld(copiedRaw, original));
        }
    }
}
