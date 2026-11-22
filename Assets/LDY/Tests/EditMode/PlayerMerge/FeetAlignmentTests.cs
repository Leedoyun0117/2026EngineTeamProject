using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class FeetAlignmentTests {
        [Test]
        public void FindOpaqueRows_ReturnsLowestAndHighestRowsWithAlpha() {
            var alpha = new byte[4 * 5];
            alpha[2 * 4 + 1] = 255;
            alpha[4 * 4 + 3] = 10;

            var rows = FeetAlignment.FindOpaqueRows(alpha, 4, 5);

            Assert.AreEqual(2, rows.Lowest);
            Assert.AreEqual(4, rows.Highest);
            Assert.AreEqual(3, rows.Height);
        }

        [Test]
        public void FindOpaqueRows_FullyTransparent_IsEmpty() {
            var rows = FeetAlignment.FindOpaqueRows(new byte[9], 3, 3);

            Assert.IsTrue(rows.IsEmpty);
            Assert.AreEqual(0, rows.Height);
        }

        [Test]
        public void ColliderBottom_IsOffsetMinusHalfSize() {
            Assert.AreEqual(-0.5f, FeetAlignment.ColliderBottom(0.2567531f, 1.513506f), 0.001f);
        }

        [Test]
        public void VisualLocalY_BottomPivotWithPaddingRows_LowersVisualBelowColliderBottom() {
            var y = FeetAlignment.VisualLocalY(-0.5f, 0f, 6, 40f);

            Assert.AreEqual(-0.65f, y, 0.0001f);
        }

        [Test]
        public void VisualLocalY_FeetOnPivot_PutsPivotOnColliderBottom() {
            Assert.AreEqual(-0.5f, FeetAlignment.VisualLocalY(-0.5f, 3f, 3, 40f), 0.0001f);
        }

        [Test]
        public void VisualLocalY_ScalesWithPixelsPerUnit() {
            var at40 = FeetAlignment.VisualLocalY(0f, 0f, 8, 40f);
            var at100 = FeetAlignment.VisualLocalY(0f, 0f, 8, 100f);

            Assert.AreEqual(-0.2f, at40, 0.0001f);
            Assert.AreEqual(-0.08f, at100, 0.0001f);
        }

        [Test]
        public void PixelsToUnits_UsesPixelsPerUnit() {
            Assert.AreEqual(1.275f, FeetAlignment.PixelsToUnits(51f, 40f), 0.0001f);
        }
    }
}
