using System.Linq;
using LDY.Flow.Core;
using LDY.StageSelect.Core;
using NUnit.Framework;

namespace LDY.Tests.StageSelect {
    public class StartSelectionRuleTests {
        private static readonly string[] Ids = { "1-T", "1-1", "1-2", "1-3" };

        [Test]
        public void FirstIncompleteWhenNothingWasCleared() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Open, StageStatus.Locked, StageStatus.Locked };
            Assert.AreEqual(1, StartSelectionRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void JustClearedWins() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Cleared, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(1, StartSelectionRule.Pick(Ids, statuses, "1-1"));
        }

        [Test]
        public void UnknownClearedIdFallsBackToFirstIncomplete() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Cleared, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(2, StartSelectionRule.Pick(Ids, statuses, "9-9"));
        }

        [Test]
        public void LockedIsNotIncomplete() {
            var statuses = new[] { StageStatus.Locked, StageStatus.NotReady, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(2, StartSelectionRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void AllClearedFallsBackToFirst() {
            var statuses = Enumerable.Repeat(StageStatus.Cleared, 4).ToArray();
            Assert.AreEqual(0, StartSelectionRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void EmptyListHasNoSelection() {
            Assert.AreEqual(-1, StartSelectionRule.Pick(new string[0], new StageStatus[0], "1-1"));
        }
    }
}
