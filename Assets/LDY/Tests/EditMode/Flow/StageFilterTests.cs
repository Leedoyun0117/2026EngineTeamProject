using System.Linq;
using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests.Flow {
    public class StageFilterTests {
        private static readonly StageStatus[] Statuses = {
            StageStatus.Cleared, StageStatus.Open, StageStatus.Locked, StageStatus.NotReady, StageStatus.Cleared
        };

        private static readonly int[] Everything = Enumerable.Range(0, Statuses.Length).ToArray();

        [Test]
        public void AllKeepsEverything() {
            CollectionAssert.AreEqual(Everything, StageFilter.Apply(StageFilterMode.All, Statuses, Everything));
        }

        [Test]
        public void IncompleteKeepsEverythingNotCleared() {
            CollectionAssert.AreEqual(new[] { 1, 2, 3 },
                StageFilter.Apply(StageFilterMode.Incomplete, Statuses, Everything));
        }

        [Test]
        public void CompleteKeepsOnlyCleared() {
            CollectionAssert.AreEqual(new[] { 0, 4 },
                StageFilter.Apply(StageFilterMode.Complete, Statuses, Everything));
        }

        [Test]
        public void SummaryCountsShownItems() {
            var summary = StageFilter.Summarize(Statuses, new[] { 0, 1, 2 });

            Assert.AreEqual(3, summary.Total);
            Assert.AreEqual(1, summary.Cleared);
            Assert.AreEqual(1, summary.Locked);
        }
    }
}
