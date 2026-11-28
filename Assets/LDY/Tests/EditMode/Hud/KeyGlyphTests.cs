using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class KeyGlyphTests {
        [TestCase("<Keyboard>/e", "E")]
        [TestCase("<Keyboard>/y", "Y")]
        [TestCase("<Keyboard>/1", "1")]
        [TestCase("<Keyboard>/tab", "Tab")]
        [TestCase("<Keyboard>/space", "Space")]
        [TestCase("<Keyboard>/leftShift", "Shift")]
        [TestCase("<Keyboard>/escape", "Esc")]
        [TestCase("<Keyboard>/numpad5", "Num5")]
        [TestCase("<Keyboard>/f1", "F1")]
        [TestCase("<Mouse>/leftButton", "LMB")]
        [TestCase("<Gamepad>/buttonSouth", "ButtonSouth")]
        public void PathToGlyph(string path, string glyph) {
            Assert.AreEqual(glyph, KeyGlyph.FromPath(path));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("2DVector")]
        [TestCase("<Keyboard>/")]
        [TestCase("<>/e")]
        public void InvalidPathHasNoGlyph(string path) {
            Assert.IsNull(KeyGlyph.FromPath(path));
        }

        [Test]
        public void PickPrefersKeyboard() {
            Assert.AreEqual("E", KeyGlyph.Pick(new[] { "<Gamepad>/buttonWest", "<Keyboard>/e" }));
        }

        [Test]
        public void PickSkipsCompositeHeaders() {
            Assert.AreEqual("A", KeyGlyph.Pick(new[] { "2DVector", "<Keyboard>/a", "<Keyboard>/d" }));
        }

        [Test]
        public void PickFallsBackToFirstReadable() {
            Assert.AreEqual("LMB", KeyGlyph.Pick(new[] { "", "<Mouse>/leftButton", "<Gamepad>/buttonSouth" }));
        }

        [Test]
        public void PickWithNothingReadableIsNull() {
            Assert.IsNull(KeyGlyph.Pick(null));
            Assert.IsNull(KeyGlyph.Pick(new[] { "", "2DVector" }));
        }
    }
}
