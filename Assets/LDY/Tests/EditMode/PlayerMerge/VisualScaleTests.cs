using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class VisualScaleTests {
        [Test]
        public void Matching_Ppu100Root2_ToPpu40_Is08() {
            Assert.AreEqual(0.8f, VisualScale.Matching(100f, 2f, 40f), 1e-6f);
        }

        [Test]
        public void WorldHeight_SameCharacterBothWays_Is102() {
            Assert.AreEqual(1.02f, VisualScale.WorldHeight(51, 100f, 2f), 1e-6f);
            Assert.AreEqual(1.02f, VisualScale.WorldHeight(51, 40f, 0.8f), 1e-6f);
        }

        [Test]
        public void Choose_HeightsMatch_KeepsDefault() {
            var choice = VisualScale.Choose(0.8f, 1.02f, 51, 40f);

            Assert.IsTrue(choice.UsedDefault);
            Assert.AreEqual(0.8f, choice.Scale, 1e-6f);
        }

        [Test]
        public void Choose_HeightsDiffer_UsesExactRatio() {
            var choice = VisualScale.Choose(0.8f, 1.02f, 50, 40f);

            Assert.IsFalse(choice.UsedDefault);
            Assert.AreEqual(1.02f, VisualScale.WorldHeight(50, 40f, choice.Scale), 1e-6f);
        }

        [Test]
        public void EffectivePixelsPerUnit_Scale08_Is50() {
            Assert.AreEqual(50f, VisualScale.EffectivePixelsPerUnit(40f, 0.8f), 1e-4f);
        }

        [Test]
        public void FeetPlacement_UsesScaledPixel() {
            var y = FeetAlignment.VisualLocalY(-0.0085f, 0f, 1, VisualScale.EffectivePixelsPerUnit(40f, 0.8f));

            Assert.AreEqual(-0.0285f, y, 1e-5f);
        }
    }
}
