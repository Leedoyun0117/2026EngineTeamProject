using LDY.Title.Core;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests.Title {
    // focus_frame.png: PPU 60, border 8px, 점 8px + 간격 8px = 주기 16px
    public class DottedFrameTests {
        private const float Ppu = 60f;
        private const float Period = 16f / Ppu;
        private const float Border = 8f / Ppu;
        private const float Eps = 1e-4f;

        [TestCase(9.6f, 9.6f)] // 슬라이더 행 폭(576px, 이미 36주기)
        [TestCase(1.0f, 64f / Ppu)] // 행 높이 60px → 64px
        [TestCase(4.6f, 272f / Ppu)] // 드롭다운 폭 276px → 272px
        [TestCase(0.86f, 48f / Ppu)] // 닫기 버튼 높이 51.6px → 48px
        [TestCase(0.01f, 32f / Ppu)] // 최소 2주기
        public void SnapsToNearestPeriodMultiple(float length, float expected) {
            Assert.AreEqual(expected, DottedFrame.SnapLength(length, Period), Eps);
        }

        [TestCase(0.3f)]
        [TestCase(1.0f)]
        [TestCase(2.6f)]
        [TestCase(4.6f)]
        [TestCase(9.6f)]
        [TestCase(14.4f)]
        public void TiledMiddleIsWholePeriodsSoNoDotIsCut(float length) {
            var snapped = DottedFrame.SnapLength(length, Period);
            var periods = (snapped - Border * 2f) / Period;
            Assert.AreEqual(Mathf.Round(periods), periods, Eps);
        }

        [TestCase(1.0f)]
        [TestCase(9.6f)]
        [TestCase(0.86f)]
        public void SnapIsIdempotent(float length) {
            var once = DottedFrame.SnapLength(length, Period);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(once, DottedFrame.SnapLength(once, Period), Eps);
        }

        [Test]
        public void SnapRectKeepsCenter() {
            var rect = new Rect(-4.8f, 0.3f, 9.6f, 1f);
            var snapped = DottedFrame.SnapRect(rect, Period);
            Assert.AreEqual(rect.center.x, snapped.center.x, Eps);
            Assert.AreEqual(rect.center.y, snapped.center.y, Eps);
            Assert.AreEqual(64f / Ppu, snapped.height, Eps);
        }

        [Test]
        public void ZeroPeriodLeavesSizeAlone() {
            Assert.AreEqual(1.234f, DottedFrame.SnapLength(1.234f, 0f));
        }
    }
}
