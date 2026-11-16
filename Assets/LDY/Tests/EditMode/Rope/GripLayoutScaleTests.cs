using LDY.Script;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests {
    public class GripLayoutScaleTests {
        private static readonly GripLayout Layout = new(100f, new Vector2(32f, 64f), new Vector2(51f, 21f));

        [Test]
        public void Doubled_scale_doubles_hand_distance_from_pivot() {
            Assert.AreEqual(Layout.HandLocal(false).x * 2f, Layout.Scaled(2f).HandLocal(false).x, 1e-4f);
        }

        [Test]
        public void Unit_scale_keeps_hand_height() {
            Assert.AreEqual(Layout.HandLocal(false).y, Layout.Scaled(1f).HandLocal(false).y, 1e-4f);
        }
    }
}
