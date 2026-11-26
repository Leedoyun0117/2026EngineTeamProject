using System.Linq;
using LDY.Background.Core;
using NUnit.Framework;

namespace LDY.Tests.Background {
    public class PixelSpaceTests {
        private const float Eps = 1e-5f;
        private static readonly PixelSpace Space = new PixelSpace(640f, 360f, 40f);

        [TestCase(0f, 0f, -8f, 4.5f)]
        [TestCase(640f, 0f, 8f, 4.5f)]
        [TestCase(0f, 360f, -8f, -4.5f)]
        [TestCase(640f, 360f, 8f, -4.5f)]
        [TestCase(320f, 180f, 0f, 0f)]
        public void ToLocal_코너와_중앙(float px, float py, float x, float y) {
            var p = Space.ToLocal(px, py);
            Assert.That(p.X, Is.EqualTo(x).Within(Eps));
            Assert.That(p.Y, Is.EqualTo(y).Within(Eps));
        }

        [Test]
        public void CenterOf_rect_중심() {
            var rect = new PixelRect(22f, 76f, 136f, 10f);
            var c = Space.CenterOf(rect);
            Assert.That(c.X, Is.EqualTo((90f - 320f) / 40f).Within(Eps));
            Assert.That(c.Y, Is.EqualTo((180f - 81f) / 40f).Within(Eps));
            var size = Space.SizeOf(rect);
            Assert.That(size.X, Is.EqualTo(3.4f).Within(Eps));
            Assert.That(size.Y, Is.EqualTo(0.25f).Within(Eps));
        }
    }

    public class FrameClockTests {
        [Test]
        public void 경계_정확히_한_프레임이면_다음_인덱스() {
            var clock = new FrameClock(4, 0.125);
            Assert.That(clock.Advance(0.0625), Is.False);
            Assert.That(clock.Index, Is.EqualTo(0));
            Assert.That(clock.Advance(0.0625), Is.True);
            Assert.That(clock.Index, Is.EqualTo(1));
        }

        [Test]
        public void 루프_마지막_다음은_0() {
            var clock = new FrameClock(3, 0.125);
            clock.Advance(0.375);
            Assert.That(clock.Index, Is.EqualTo(0));
            Assert.That(clock.Ticks, Is.EqualTo(3));
            clock.Advance(0.125);
            Assert.That(clock.Index, Is.EqualTo(1));
        }

        [Test]
        public void 큰_dt는_여러_프레임을_건너뜀() {
            var clock = new FrameClock(12, 0.125);
            Assert.That(clock.Advance(1.0), Is.True);
            Assert.That(clock.Index, Is.EqualTo(8));
        }

        [TestCase(0d)]
        [TestCase(-1d)]
        public void dt_0이하면_멈춤(double dt) {
            var clock = new FrameClock(4, 0.125);
            clock.Advance(0.125);
            Assert.That(clock.Advance(dt), Is.False);
            Assert.That(clock.Index, Is.EqualTo(1));
        }
    }

    public class VisualizerModelTests {
        [Test]
        public void 같은_시드면_같은_결과() {
            var a = new VisualizerModel(42, 14, 16);
            var b = new VisualizerModel(42, 14, 16);
            for (var step = 0; step < 200; step++)
            for (var bar = 0; bar < 14; bar++)
                Assert.That(a.Height(step, bar), Is.EqualTo(b.Height(step, bar)));
        }

        [Test]
        public void 다른_시드면_다른_결과() {
            var a = new VisualizerModel(1, 14, 16);
            var b = new VisualizerModel(2, 14, 16);
            var differs = Enumerable.Range(0, 50).Any(step => a.Height(step, 3) != b.Height(step, 3));
            Assert.That(differs, Is.True);
        }

        [Test]
        public void 높이는_0과_segments_사이() {
            var model = new VisualizerModel(7, 14, 16);
            for (var step = 0; step < 500; step++)
            for (var bar = 0; bar < 14; bar++)
                Assert.That(model.Height(step, bar), Is.InRange(0, 16));
        }
    }

    public class BackgroundFitTests {
        [Test]
        public void ortho9_16대9는_정확히_2() {
            Assert.That(BackgroundFit.CoverScale(9d, 16d / 9d), Is.EqualTo(2d));
        }

        [Test]
        public void ortho10_16대9는_약_2_22() {
            Assert.That(BackgroundFit.CoverScale(10d, 16d / 9d), Is.EqualTo(20d / 9d).Within(1e-9));
        }

        [Test]
        public void ortho4_5_16대9는_1() {
            Assert.That(BackgroundFit.CoverScale(4.5d, 16d / 9d), Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void 가로가_넓으면_가로_기준() {
            Assert.That(BackgroundFit.CoverScale(9d, 21d / 9d), Is.EqualTo(18d * 21d / 9d / 16d).Within(1e-9));
        }

        [Test]
        public void 세로가_긴_화면은_세로_기준() {
            Assert.That(BackgroundFit.CoverScale(9d, 4d / 3d), Is.EqualTo(2d).Within(1e-12));
        }
    }

    public class PixelFontSizeTests {
        [TestCase(12f, 12)]
        [TestCase(14f, 12)]
        [TestCase(24f, 24)]
        [TestCase(30f, 24)]
        [TestCase(7f, 12)]
        public void 크기는_12의_정수배이고_줄이지_않음(float rectHeight, int expected) {
            Assert.That(PixelFontSize.PixelSize(rectHeight, 12), Is.EqualTo(expected));
        }

        [TestCase(9f, true)]
        [TestCase(12f, false)]
        [TestCase(14f, false)]
        public void rect보다_크면_넘침(float rectHeight, bool overflows) {
            Assert.That(PixelFontSize.OverflowsHeight(rectHeight, 12), Is.EqualTo(overflows));
        }
    }

    public class NoteMotionTests {
        [Test]
        public void 수명_밖이면_보이지_않음() {
            Assert.That(NoteMotion.Evaluate(2f, 2f, 1f).Alpha, Is.EqualTo(0f));
            Assert.That(NoteMotion.Evaluate(-0.1f, 2f, 1f).Alpha, Is.EqualTo(0f));
        }

        [Test]
        public void 중간에는_위로_올라가고_불투명() {
            var pose = NoteMotion.Evaluate(1f, 2f, 2f);
            Assert.That(pose.Rise, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(pose.Alpha, Is.EqualTo(1f).Within(1e-5f));
        }
    }
}
