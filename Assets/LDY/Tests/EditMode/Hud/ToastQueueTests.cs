using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class ToastQueueTests {
        [Test]
        public void FirstPushShowsImmediately() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "a", 2f);

            Assert.AreEqual("a", queue.Current.Message);
            Assert.AreEqual(ToastKind.Info, queue.Current.Kind);
            Assert.AreEqual(0, queue.Waiting.Count);
        }

        [Test]
        public void SameMessageAsCurrentOnlyRefreshesTime() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Error, "a", 2f);
            queue.Tick(1.5f);
            var version = queue.Version;

            queue.Push(ToastKind.Error, "a", 2f);

            Assert.AreEqual(2f, queue.Current.Remaining, 1e-4f);
            Assert.AreEqual(0, queue.Waiting.Count);
            Assert.AreNotEqual(version, queue.Version);
        }

        [Test]
        public void SameMessageWaitingIsNotDuplicated() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "a", 2f);
            queue.Push(ToastKind.Info, "b", 1f);
            queue.Push(ToastKind.Warning, "b", 3f);

            Assert.AreEqual(1, queue.Waiting.Count);
            Assert.AreEqual(3f, queue.Waiting[0].Remaining, 1e-4f);
            Assert.AreEqual(ToastKind.Warning, queue.Waiting[0].Kind);
        }

        [Test]
        public void WaitingIsCappedByDroppingOldest() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "shown", 2f);
            foreach (var message in new[] { "1", "2", "3", "4" })
                queue.Push(ToastKind.Info, message, 2f);

            Assert.AreEqual(3, queue.Waiting.Count);
            Assert.AreEqual("2", queue.Waiting[0].Message);
            Assert.AreEqual("4", queue.Waiting[2].Message);
            Assert.AreEqual("shown", queue.Current.Message);
        }

        [Test]
        public void ExpiryAdvancesToNextThenEmpties() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "a", 1f);
            queue.Push(ToastKind.Error, "b", 1f);

            queue.Tick(0.5f);
            Assert.AreEqual("a", queue.Current.Message);

            queue.Tick(0.6f);
            Assert.AreEqual("b", queue.Current.Message);
            Assert.AreEqual(1f, queue.Current.Remaining, 1e-4f);

            queue.Tick(1f);
            Assert.IsNull(queue.Current);
        }

        [Test]
        public void ZeroWaitingDropsWhileShowing() {
            var queue = new ToastQueue(0);
            queue.Push(ToastKind.Info, "a", 1f);
            queue.Push(ToastKind.Info, "b", 1f);

            Assert.AreEqual("a", queue.Current.Message);
            Assert.AreEqual(0, queue.Waiting.Count);
        }

        [Test]
        public void EmptyMessageIsIgnored() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "", 1f);
            queue.Push(ToastKind.Info, null, 1f);

            Assert.IsNull(queue.Current);
            Assert.AreEqual(0, queue.Version);
        }

        [Test]
        public void ClearHidesEverything() {
            var queue = new ToastQueue(3);
            queue.Push(ToastKind.Info, "a", 1f);
            queue.Push(ToastKind.Info, "b", 1f);
            queue.Clear();

            Assert.IsNull(queue.Current);
            Assert.AreEqual(0, queue.Waiting.Count);
        }
    }
}
