using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests.Flow {
    public class ProgressModelTests {
        [Test]
        public void FirstClearSetsBest() {
            var progress = new ProgressModel();

            Assert.IsTrue(progress.RecordClear("1-T", 42.5d));
            Assert.IsTrue(progress.IsCleared("1-T"));
            Assert.IsTrue(progress.TryGetBest("1-T", out var best));
            Assert.AreEqual(42.5d, best);
        }

        [Test]
        public void BestChangesOnlyWhenFaster() {
            var progress = new ProgressModel();
            progress.RecordClear("1-1", 30d);

            Assert.IsFalse(progress.RecordClear("1-1", 31d));
            Assert.IsFalse(progress.RecordClear("1-1", 30d));
            progress.TryGetBest("1-1", out var unchanged);
            Assert.AreEqual(30d, unchanged);

            Assert.IsTrue(progress.RecordClear("1-1", 12d));
            progress.TryGetBest("1-1", out var faster);
            Assert.AreEqual(12d, faster);
        }

        [Test]
        public void UnknownStageHasNoRecord() {
            var progress = new ProgressModel();

            Assert.IsFalse(progress.IsCleared("2-T"));
            Assert.IsFalse(progress.TryGetBest("2-T", out _));
        }

        [Test]
        public void ResetForgetsRecordsAndUnlocks() {
            var progress = new ProgressModel();
            progress.RecordClear("1-T", 1d);
            progress.ForceUnlock("3-T");

            progress.Reset();

            Assert.IsFalse(progress.IsCleared("1-T"));
            Assert.IsFalse(progress.IsForcedUnlocked("3-T"));
        }
    }
}
