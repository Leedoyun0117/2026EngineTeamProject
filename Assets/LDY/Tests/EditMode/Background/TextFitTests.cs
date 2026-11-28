using LDY.Background.Core;
using NUnit.Framework;

namespace LDY.Tests.Background {
    public class TextFitTests {
        [Test]
        public void 높이_9는_10으로_세로_가운데_유지() {
            var r = TextFit.Expand(new PixelRect(8f, 26f, 300f, 9f), TextAlign.Left, 0f);
            Assert.That(r.Height, Is.EqualTo(10f));
            Assert.That(r.CenterY, Is.EqualTo(30.5f));
            Assert.That(r.Y, Is.EqualTo(25.5f));
            Assert.That(r.X, Is.EqualTo(8f));
            Assert.That(r.Width, Is.EqualTo(300f));
        }

        [Test]
        public void 높이_7은_위아래로_1_5씩() {
            var r = TextFit.Expand(new PixelRect(22f, 63f, 60f, 7f), TextAlign.Left, 0f);
            Assert.That(r.Y, Is.EqualTo(61.5f));
            Assert.That(r.Height, Is.EqualTo(10f));
        }

        [Test]
        public void 높이가_충분하면_그대로() {
            var rect = new PixelRect(1f, 2f, 30f, 14f);
            Assert.That(TextFit.Same(TextFit.Expand(rect, TextAlign.Center, 20f), rect), Is.True);
        }

        [TestCase(TextAlign.Left, 49f, 48f)]
        [TestCase(TextAlign.Center, 49f, 47f)]
        [TestCase(TextAlign.Right, 49f, 46f)]
        public void 너비는_정렬_기준점을_고정하고_올림(TextAlign align, float x, float needed) {
            var r = TextFit.Expand(new PixelRect(49f, 42f, 47f, 14f), align, needed + 0.3f);
            Assert.That(r.Width, Is.EqualTo(needed + 1f));
            var grow = r.Width - 47f;
            var expected = align == TextAlign.Left ? 49f : align == TextAlign.Right ? 49f - grow : 49f - grow / 2f;
            Assert.That(r.X, Is.EqualTo(expected));
        }

        [TestCase(19f, false)]
        [TestCase(20f, true)]
        [TestCase(60f, true)]
        public void 두_줄_높이부터_줄바꿈(float height, bool wraps) {
            Assert.That(TextFit.Wraps(new PixelRect(0f, 0f, 100f, height)), Is.EqualTo(wraps));
        }
    }
}
