using System.Collections.Generic;
using System.Linq;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportIdTests {
        private static readonly SlotIdRule Tutorial = new SlotIdRule(true, "T");

        // slots[i]에 들어갈 (맵 id, 맵 이름) 목록으로 1x1 맵 파일을 만든다.
        private static CursorStageFile FileOf(params (string id, string name)[][] slots) {
            var stages = new List<ToolStage>();
            var toolSlots = new List<ToolSlot>();
            foreach (var slot in slots) {
                foreach (var (id, name) in slot)
                    stages.Add(new ToolStage(id, name, 1, 1, new[] { "" }, new[] { 0 }, new ToolNote[0], "", false, ""));
                toolSlots.Add(new ToolSlot(slot.Select(m => m.id).ToList(), ""));
            }

            return new CursorStageFile(3, stages, new CustomBlock[0], toolSlots);
        }

        [TestCase("2-3", 2, "2-3")]
        [TestCase("  1-T ", 1, "1-T")]
        [TestCase("10-12", 10, "10-12")]
        [TestCase("02-03", 2, "2-3")]
        public void NameId_Parses(string name, int world, string id) {
            Assert.IsTrue(StageNameId.TryParse(name, out var w, out var s));
            Assert.AreEqual(world, w);
            Assert.AreEqual(id, s);
        }

        [TestCase("Tutorial")]
        [TestCase("1-")]
        [TestCase("-1")]
        [TestCase("1-2-3")]
        [TestCase("1-t")]
        [TestCase("1 - 2")]
        [TestCase("A-1")]
        [TestCase("1-2a")]
        [TestCase("１-2")]
        [TestCase("")]
        [TestCase((string)null)]
        public void NameId_RejectsOtherShapes(string name) {
            Assert.IsFalse(StageNameId.TryParse(name, out _, out _));
        }

        [Test]
        public void NameFormat_UsesNameAsId() {
            var file = FileOf(new[] { ("a", "1-T"), ("b", "1-2"), ("c", "1-1") });
            var result = StageIdAssigner.Assign(file, Tutorial);
            CollectionAssert.AreEqual(new[] { "1-T", "1-2", "1-1" }, result.Assigned.Select(a => a.StageId));
            Assert.IsTrue(result.Assigned.All(a => a.Source == IdSource.Name));
            Assert.AreEqual(0, result.WorldMismatches.Count);
        }

        [Test]
        public void NameOffFormat_FallsBackToSlotOrder() {
            // 순서 번호는 이름으로 id를 얻은 맵도 한 칸으로 센다: 0번째 1-T, 1번째(이름 '보스') → 1-1, 2번째 → 1-2
            var file = FileOf(new[] { ("a", "1-T"), ("b", "보스"), ("c", " 1-2 ") });
            var result = StageIdAssigner.Assign(file, Tutorial);
            CollectionAssert.AreEqual(new[] { "1-T", "1-1", "1-2" }, result.Assigned.Select(a => a.StageId));
            CollectionAssert.AreEqual(new[] { IdSource.Name, IdSource.SlotOrder, IdSource.Name },
                result.Assigned.Select(a => a.Source));
        }

        [Test]
        public void WorldMismatch_WarnsAndKeepsNameWorld() {
            var file = FileOf(new[] { ("a", "1-1") }, new[] { ("b", "3-2") });
            var result = StageIdAssigner.Assign(file, Tutorial);
            var mismatch = result.WorldMismatches.Single();
            Assert.AreEqual("b", mismatch.MapId);
            Assert.AreEqual(2, mismatch.SlotWorld);
            Assert.AreEqual(3, mismatch.NameWorld);

            var b = result.Assigned.Single(a => a.MapId == "b");
            Assert.AreEqual("3-2", b.StageId);
            Assert.AreEqual(3, b.World);
            Assert.AreEqual(2, b.SlotWorld);
        }

        [Test]
        public void DuplicateId_DropsBothAndReports() {
            var file = FileOf(new[] { ("a", "1-1"), ("b", "1-2"), ("c", "1-1") });
            var result = StageIdAssigner.Assign(file, Tutorial);
            CollectionAssert.AreEqual(new[] { "b" }, result.Assigned.Select(a => a.MapId));
            var conflict = result.IdConflicts.Single();
            Assert.AreEqual("1-1", conflict.StageId);
            CollectionAssert.AreEqual(new[] { "a", "c" }, conflict.MapIds);
        }

        [Test]
        public void DuplicateId_BetweenNameAndSlotOrder() {
            // 'x'는 슬롯 순서로 1-1이 되고, 'y'는 이름이 1-1이다.
            var file = FileOf(new[] { ("t", "튜토리얼"), ("x", "첫 방"), ("y", "1-1") });
            var result = StageIdAssigner.Assign(file, Tutorial);
            CollectionAssert.AreEqual(new[] { "t" }, result.Assigned.Select(a => a.MapId));
            Assert.AreEqual("1-1", result.IdConflicts.Single().StageId);
        }
    }
}
