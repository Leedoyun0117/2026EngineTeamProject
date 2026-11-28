using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class TypewriterTests {
        [Test]
        public void RevealsByCharactersPerSecond() {
            var typer = new Typewriter(10f);
            typer.Start("abcdef");
            Assert.AreEqual("", typer.Visible);

            Assert.IsTrue(typer.Tick(0.25f));
            Assert.AreEqual("ab", typer.Visible);
            Assert.IsFalse(typer.IsDone);
        }

        [Test]
        public void SmallStepsAccumulate() {
            var typer = new Typewriter(10f);
            typer.Start("abc");
            Assert.IsFalse(typer.Tick(0.06f));
            Assert.IsTrue(typer.Tick(0.06f));
            Assert.AreEqual(1, typer.VisibleCount);
        }

        [Test]
        public void LongFrameClampsToLength() {
            var typer = new Typewriter(10f);
            typer.Start("abc");
            typer.Tick(5f);

            Assert.AreEqual("abc", typer.Visible);
            Assert.IsTrue(typer.IsDone);
            Assert.IsFalse(typer.Tick(1f));
        }

        [Test]
        public void CompleteAndRestart() {
            var typer = new Typewriter(10f);
            typer.Start("abc");
            typer.Complete();
            Assert.IsTrue(typer.IsDone);

            typer.Start("xy");
            Assert.AreEqual(0, typer.VisibleCount);
            Assert.AreEqual("xy", typer.Text);
        }

        [Test]
        public void NonPositiveSpeedShowsAllAndNullIsEmpty() {
            var instant = new Typewriter(0f);
            instant.Start("abc");
            Assert.AreEqual("abc", instant.Visible);

            var typer = new Typewriter(10f);
            typer.Start(null);
            Assert.IsTrue(typer.IsDone);
            Assert.AreEqual("", typer.Visible);
        }

        [Test]
        public void NegativeDeltaDoesNotRewind() {
            var typer = new Typewriter(10f);
            typer.Start("abcd");
            typer.Tick(0.2f);
            typer.Tick(-1f);
            Assert.AreEqual(2, typer.VisibleCount);
        }
    }

    public class FrameClockTests {
        [Test]
        public void AdvancesAfterFrameTime() {
            var clock = new FrameClock(4, 0.1f);
            Assert.IsFalse(clock.Tick(0.05f));
            Assert.IsTrue(clock.Tick(0.05f));
            Assert.AreEqual(1, clock.Frame);
        }

        [Test]
        public void LongFrameSkipsAndWraps() {
            var clock = new FrameClock(4, 0.1f);
            clock.Tick(0.55f);
            Assert.AreEqual(1, clock.Frame);
        }

        [Test]
        public void SingleFrameOrZeroDurationNeverChanges() {
            var single = new FrameClock(1, 0.1f);
            Assert.IsFalse(single.Tick(1f));

            var zero = new FrameClock(4, 0f);
            Assert.IsFalse(zero.Tick(1f));
            Assert.AreEqual(0, zero.Frame);
        }

        [Test]
        public void ResetGoesBackToFirstFrame() {
            var clock = new FrameClock(2, 0.1f);
            clock.Tick(0.15f);
            clock.Reset();
            Assert.AreEqual(0, clock.Frame);
            Assert.IsFalse(clock.Tick(0.05f));
        }
    }

    public class PixelSnapTests {
        [Test]
        public void RoundsToWholeCanvasPixels() {
            Assert.AreEqual(3f, PixelSnap.Snap(3.4f));
            Assert.AreEqual(4f, PixelSnap.Snap(3.6f));
            Assert.AreEqual(-3f, PixelSnap.Snap(-3.4f));
        }

        [Test]
        public void HalfRoundsAwayFromZero() {
            Assert.AreEqual(3f, PixelSnap.Snap(2.5f));
            Assert.AreEqual(-3f, PixelSnap.Snap(-2.5f));
        }

        [Test]
        public void SnapsToWorldPixelGrid() {
            Assert.AreEqual(0.025f, PixelSnap.Snap(0.03f, 1f / 40f), 1e-6f);
            Assert.AreEqual(1.05f, PixelSnap.Snap(1.04f, 1f / 40f), 1e-6f);
        }

        [Test]
        public void NonPositiveStepPassesThrough() {
            Assert.AreEqual(1.234f, PixelSnap.Snap(1.234f, 0f));
            Assert.AreEqual(1.234f, PixelSnap.Snap(1.234f, -1f));
        }
    }
}
