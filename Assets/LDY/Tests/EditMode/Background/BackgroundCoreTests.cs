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

    public class MapFitTests {
        private const float HalfW = 5f * 16f / 9f;
        private const float HalfH = 5f;

        [TestCase(40f, 20f)]
        [TestCase(80f, 20f)]
        [TestCase(60f, 70f)]
        [TestCase(122f, 40f)]
        public void 고정_배율_3은_맵_왼쪽_아래에_48x27_창(float w, float h) {
            var map = new WorldRect(-3f, 5f, -3f + w, 5f + h);
            var plan = MapFit.Plan(map, BackgroundFitMode.FixedScale, 3, BackgroundAnchor.BottomLeft, HalfW, HalfH);
            Assert.That(plan.Scale, Is.EqualTo(3));
            Assert.That(plan.Window.MinX, Is.EqualTo(-3f));
            Assert.That(plan.Window.MinY, Is.EqualTo(5f));
            Assert.That(plan.Window.Width, Is.EqualTo(48f));
            Assert.That(plan.Window.Height, Is.EqualTo(27f));
        }

        [TestCase(40f, 20f)]
        [TestCase(80f, 20f)]
        [TestCase(60f, 70f)]
        [TestCase(122f, 40f)]
        public void 바탕화면은_맵에_반화면_여유를_더해_덮는다(float w, float h) {
            var map = new WorldRect(0f, 0f, w, h);
            var plan = MapFit.Plan(map, BackgroundFitMode.FixedScale, 3, BackgroundAnchor.BottomLeft, HalfW, HalfH);
            Assert.That(plan.Desktop.MinX, Is.EqualTo(-HalfW).Within(1e-4f));
            Assert.That(plan.Desktop.MinY, Is.EqualTo(-HalfH).Within(1e-4f));
            Assert.That(plan.Desktop.Width, Is.EqualTo(w + 2f * HalfW).Within(1e-4f));
            Assert.That(plan.Desktop.Height, Is.EqualTo(h + 2f * HalfH).Within(1e-4f));
            Assert.That(plan.Desktop.CenterX, Is.EqualTo(w / 2f).Within(1e-4f));
            Assert.That(plan.Desktop.CenterY, Is.EqualTo(h / 2f).Within(1e-4f));
        }

        [Test]
        public void 중앙_정렬_옵션은_맵_중심() {
            var plan = MapFit.Plan(new WorldRect(0f, 0f, 122f, 40f), BackgroundFitMode.FixedScale, 3,
                BackgroundAnchor.Center, HalfW, HalfH);
            Assert.That(plan.Window.CenterX, Is.EqualTo(61f));
            Assert.That(plan.Window.CenterY, Is.EqualTo(20f));
        }

        [TestCase(2)]
        [TestCase(5)]
        public void 고정_배율은_인스펙터_값(int scale) {
            var plan = MapFit.Plan(new WorldRect(0f, 0f, 40f, 20f), BackgroundFitMode.FixedScale, scale,
                BackgroundAnchor.BottomLeft, HalfW, HalfH);
            Assert.That(plan.Scale, Is.EqualTo(scale));
            Assert.That(plan.Window.Width, Is.EqualTo(16f * scale));
        }

        [Test]
        public void 고정_배율_0이하는_1() {
            Assert.That(MapFit.Plan(new WorldRect(0f, 0f, 40f, 20f), BackgroundFitMode.FixedScale, 0,
                BackgroundAnchor.BottomLeft, 0f, 0f).Scale, Is.EqualTo(1));
        }

        [TestCase(40f, 20f, 3)]
        [TestCase(80f, 20f, 5)]
        [TestCase(60f, 70f, 8)]
        [TestCase(122f, 40f, 8)]
        [TestCase(16f, 9f, 1)]
        [TestCase(32f, 18f, 2)]
        public void 맵_덮기_옵션은_정수로_올린_배율(float w, float h, int scale) {
            var plan = MapFit.Plan(new WorldRect(0f, 0f, w, h), BackgroundFitMode.CoverMap, 3,
                BackgroundAnchor.Center, HalfW, HalfH);
            Assert.That(plan.Scale, Is.EqualTo(scale));
            Assert.That(plan.Window.Width, Is.GreaterThanOrEqualTo(w));
            Assert.That(plan.Window.Height, Is.GreaterThanOrEqualTo(h));
        }

        [Test]
        public void 크기_0은_예외() {
            Assert.Throws<System.ArgumentException>(() => MapFit.Plan(new WorldRect(0f, 0f, 0f, 5f),
                BackgroundFitMode.FixedScale, 3, BackgroundAnchor.BottomLeft, 0f, 0f));
        }
    }

    public class BackgroundDimTests {
        [Test]
        public void 기본_dim은_RGB에_0_65를_곱하고_알파는_유지() {
            var c = BackgroundDim.Apply(new Rgba(1f, 0.5f, 0.2f, 0.8f), BackgroundDim.Default);
            Assert.That(c.R, Is.EqualTo(0.65f).Within(1e-5f));
            Assert.That(c.G, Is.EqualTo(0.325f).Within(1e-5f));
            Assert.That(c.B, Is.EqualTo(0.13f).Within(1e-5f));
            Assert.That(c.A, Is.EqualTo(0.8f));
        }

        [TestCase(0f, 1f)]
        [TestCase(1f, 0f)]
        [TestCase(-1f, 1f)]
        [TestCase(2f, 0f)]
        public void dim은_0과_1_사이로_자름(float dim, float factor) {
            Assert.That(BackgroundDim.Factor(dim), Is.EqualTo(factor));
        }
    }

    public class BackgroundTextDefaultsTests {
        [TestCase("window_title", true)]
        [TestCase("tab_0", true)]
        [TestCase("tab_3", true)]
        [TestCase("address_path", true)]
        [TestCase("side_title", true)]
        [TestCase("menu", false)]
        [TestCase("tree_4", false)]
        [TestCase("address_label", false)]
        [TestCase("status_marquee", false)]
        public void 기본_show(string id, bool shown) {
            Assert.That(BackgroundTextDefaults.ShownByDefault(id), Is.EqualTo(shown));
        }
    }

    public class BackgroundWorldTests {
        [TestCase("1-1", 1)]
        [TestCase("2-T", 2)]
        [TestCase("3-6", 3)]
        public void 스테이지_id의_월드(string id, int world) {
            Assert.That(BackgroundWorld.TryFromStageId(id, out var w), Is.True);
            Assert.That(w, Is.EqualTo(world));
        }

        [TestCase("Stage1_MusicPlayer_Source", 1)]
        [TestCase("Stage3_RecycleBin_Source", 3)]
        public void 에셋_이름의_월드(string name, int world) {
            Assert.That(BackgroundWorld.TryFromAssetName(name, out var w), Is.True);
            Assert.That(w, Is.EqualTo(world));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("T-1")]
        [TestCase("0-1")]
        public void 숫자로_시작하지_않으면_실패(string id) {
            Assert.That(BackgroundWorld.TryFromStageId(id, out _), Is.False);
        }

        [Test]
        public void Stage_접두사가_없으면_실패() {
            Assert.That(BackgroundWorld.TryFromAssetName("MusicPlayer1", out _), Is.False);
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
