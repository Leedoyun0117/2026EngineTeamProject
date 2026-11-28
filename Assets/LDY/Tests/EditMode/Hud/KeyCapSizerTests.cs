using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    // 폭은 Galmuri9 10px(HudSkin 글꼴)의 글자 진행폭: Y 6, Q 6, E 5, Tab 6+6+6, Space 5+6+6+6+6, 한글 10.
    // 한 줄 높이는 ascent 10 - descent(-1) = 11.
    public class KeyCapSizerTests {
        private const float LineHeight = 11f;
        private const float TwoHangul = 20f; // "실행", "해제", "흡수", "전환"

        // 툴팁 말풍선: 키 상자 규칙은 같고, 여백 2/3, 간격 2, 높이 12+2+2.
        private static readonly KeyCapSpec Bubble = new KeyCapSpec(3, 12, 2, 2, 3, 16);

        [TestCase("Y", 6f, 12, 20, 45)]
        [TestCase("Q", 6f, 12, 20, 45)]
        [TestCase("E", 5f, 11, 19, 44)]
        [TestCase("Tab", 18f, 24, 32, 57)]
        [TestCase("Space", 29f, 35, 43, 68)]
        public void ButtonFitsGlyph(string glyph, float glyphWidth, int capWidth, int labelX, int width) {
            var m = KeyCapSizer.Measure(glyphWidth, LineHeight, TwoHangul, LineHeight, KeyCapSpec.Button);

            Assert.AreEqual(capWidth, m.CapWidth, glyph + " 키 상자 폭 = 글자 + 3*2");
            Assert.AreEqual(12, m.CapHeight, glyph + " 키 상자 높이 고정");
            Assert.AreEqual(5, m.CapX, glyph + " 왼쪽 여백");
            Assert.AreEqual(3, m.CapY, glyph + " 18 안에서 12 세로 가운데");
            Assert.AreEqual(labelX, m.LabelX, glyph + " 라벨 = 키 상자 + 3");
            Assert.AreEqual(width, m.Width, glyph + " 버튼 폭 = 5 + 상자 + 3 + 라벨 + 5");
            Assert.AreEqual(18, m.Height, glyph + " 버튼 높이 고정");
            Assert.IsFalse(m.GlyphOverflow);
            Assert.IsFalse(m.LabelOverflow);
        }

        [TestCase("Y", 6f)]
        [TestCase("Q", 6f)]
        [TestCase("E", 5f)]
        [TestCase("Tab", 18f)]
        [TestCase("Space", 29f)]
        public void GlyphIsCenteredInCap(string glyph, float glyphWidth) {
            var m = KeyCapSizer.Measure(glyphWidth, LineHeight, TwoHangul, LineHeight, KeyCapSpec.Button);

            Assert.AreEqual(3, m.GlyphX, glyph);
            Assert.AreEqual(m.CapWidth - m.GlyphWidth - m.GlyphX, m.GlyphX, glyph + " 좌우 여백 같음");
            Assert.AreEqual(1, m.GlyphY, glyph + " 12 안에서 11, 반 픽셀은 위로");
        }

        [TestCase("E", 5f, 11, 15, 38)]
        [TestCase("Y", 6f, 12, 16, 39)]
        public void TooltipUsesSameCapRule(string glyph, float glyphWidth, int capWidth, int labelX, int width) {
            var m = KeyCapSizer.Measure(glyphWidth, LineHeight, TwoHangul, LineHeight, Bubble);

            Assert.AreEqual(capWidth, m.CapWidth, glyph);
            Assert.AreEqual(12, m.CapHeight, glyph);
            Assert.AreEqual(2, m.CapY, glyph);
            Assert.AreEqual(labelX, m.LabelX, glyph);
            Assert.AreEqual(width, m.Width, glyph);
            Assert.AreEqual(16, m.Height, glyph);
        }

        [Test]
        public void FractionalWidthRoundsUpToWholePixel() {
            Assert.AreEqual(6, KeyCapSizer.Pixels(5.2f));
            Assert.AreEqual(5, KeyCapSizer.Pixels(5.0000005f));
            Assert.AreEqual(0, KeyCapSizer.Pixels(-1f));
            Assert.AreEqual(12, KeyCapSizer.Measure(5.2f, LineHeight, 0f, 0f, KeyCapSpec.Button).CapWidth);
        }

        [Test]
        public void EmptyLabelDropsGap() {
            var m = KeyCapSizer.Measure(6f, LineHeight, 0f, 0f, KeyCapSpec.Button);
            Assert.AreEqual(5 + 12 + 5, m.Width);
        }

        [Test]
        public void TallTextIsOverflow() {
            var m = KeyCapSizer.Measure(6f, 13f, TwoHangul, 14f, KeyCapSpec.Button);
            Assert.IsTrue(m.GlyphOverflow);
            Assert.IsTrue(m.LabelOverflow);
        }

        [Test]
        public void RowStartsAfterSlotWithGap() {
            // 슬롯 x=4 폭 128 → 오른쪽 132. 실행(Y) 45, 해제(Q) 45.
            CollectionAssert.AreEqual(new[] { 136, 185 }, KeyCapSizer.Row(132, 4, new[] { 45, 45 }));
            CollectionAssert.AreEqual(new[] { 136, 197 }, KeyCapSizer.Row(132, 4, new[] { 57, 68 }));
            CollectionAssert.IsEmpty(KeyCapSizer.Row(132, 4, null));
        }

        [Test]
        public void ButtonIsVerticallyCenteredInTaskbar() {
            Assert.AreEqual(2, KeyCapSizer.Center(22, 18));
        }
    }
}
