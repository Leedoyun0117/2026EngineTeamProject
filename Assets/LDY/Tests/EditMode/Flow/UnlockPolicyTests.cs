using LDY.Flow.Core;
using NUnit.Framework;
using static LDY.Tests.Flow.FlowTestSlots;

namespace LDY.Tests.Flow {
    public class UnlockPolicyTests {
        private StageSlot[] _slots;
        private ProgressModel _progress;

        [SetUp]
        public void SetUp() {
            _slots = PlannedReadiness();
            _progress = new ProgressModel();
        }

        private StageStatus Status(string id) => UnlockPolicy.StatusOf(_slots, IndexOf(_slots, id), _progress);

        [Test]
        public void FirstTutorialIsAlwaysOpen() {
            Assert.AreEqual(StageStatus.Open, Status("1-T"));
            Assert.AreEqual(StageStatus.Locked, Status("1-1"));
        }

        [Test]
        public void ClearingOneOpensOnlyTheNext() {
            _progress.RecordClear("1-T", 5d);

            Assert.AreEqual(StageStatus.Cleared, Status("1-T"));
            Assert.AreEqual(StageStatus.Open, Status("1-1"));
            Assert.AreEqual(StageStatus.Locked, Status("1-2"));
        }

        [Test]
        public void WorldTutorialOpensOnlyAfterPreviousWorldSix() {
            ClearUpTo(_progress, _slots, "1-5");
            Assert.AreEqual(StageStatus.Locked, Status("2-T"));

            _progress.RecordClear("1-6", 5d);
            Assert.AreEqual(StageStatus.Open, Status("2-T"));
            Assert.AreEqual(StageStatus.Locked, Status("2-1"));
        }

        [Test]
        public void NotReadyBlocksInsteadOfBeingSkipped() {
            _slots = FromPlan(e => e.Id != "1-2");
            ClearUpTo(_progress, _slots, "1-1");

            Assert.AreEqual(StageStatus.NotReady, Status("1-2"));
            Assert.AreEqual(StageStatus.Locked, Status("1-3"));
        }

        [Test]
        public void EmptyThreeFiveIsNotReadyAndThreeSixStaysClosed() {
            ClearUpTo(_progress, _slots, "3-4");

            Assert.AreEqual(StageStatus.NotReady, Status("3-5"));
            Assert.AreEqual(StageStatus.NotReady, Status("3-6"));
            Assert.IsFalse(UnlockPolicy.IsUnlocked(_slots, IndexOf(_slots, "3-6"), _progress));
        }

        [Test]
        public void ForcedUnlockOpensWithoutClearingPrevious() {
            _progress.ForceUnlock("2-3");

            Assert.AreEqual(StageStatus.Open, Status("2-3"));
            Assert.AreEqual(StageStatus.Locked, Status("2-4"));
        }

        [Test]
        public void PrerequisiteIsThePreviousSlotAcrossWorlds() {
            Assert.IsNull(UnlockPolicy.PrerequisiteOf(_slots, 0));
            Assert.AreEqual("1-T", UnlockPolicy.PrerequisiteOf(_slots, IndexOf(_slots, "1-1")));
            Assert.AreEqual("1-6", UnlockPolicy.PrerequisiteOf(_slots, IndexOf(_slots, "2-T")));
        }

        [Test]
        public void EvaluateMatchesStatusOf() {
            _progress.RecordClear("1-T", 5d);
            var all = UnlockPolicy.Evaluate(_slots, _progress);

            Assert.AreEqual(_slots.Length, all.Length);
            for (var i = 0; i < _slots.Length; i++)
                Assert.AreEqual(UnlockPolicy.StatusOf(_slots, i, _progress), all[i], _slots[i].Id);
        }
    }
}
