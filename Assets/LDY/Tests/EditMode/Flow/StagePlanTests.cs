using System.Linq;
using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests.Flow {
    public class StagePlanTests {
        [Test]
        public void PlanPassesCatalogRules() {
            CollectionAssert.IsEmpty(StageCatalogRules.Validate(FlowTestSlots.PlannedReadiness()));
        }

        [Test]
        public void PlanHasTwentyOneUniqueIdsSevenPerWorld() {
            var ids = StagePlan.Entries.Select(e => e.Id).ToList();

            Assert.AreEqual(21, ids.Count);
            CollectionAssert.AllItemsAreUnique(ids);
            for (var world = 1; world <= 3; world++)
                Assert.AreEqual(7, StagePlan.Entries.Count(e => e.World == world), "월드 " + world);
        }

        [Test]
        public void OnlyThreeFiveAndThreeSixAreEmpty() {
            var empty = StagePlan.Entries.Where(e => e.Abilities.Count == 0).Select(e => e.Id);

            CollectionAssert.AreEqual(new[] { "3-5", "3-6" }, empty);
        }

        [Test]
        public void AbilitiesUseKnownPickupIds() {
            var known = new[] { "pause", "cut", "drag", "alpha", "light", "tab", "copy", "zoom" };

            foreach (var entry in StagePlan.Entries)
                CollectionAssert.IsSubsetOf(entry.Abilities, known, entry.Id);
        }

        [Test]
        public void RulesCatchDuplicatesCountAndOrder() {
            var slots = FlowTestSlots.PlannedReadiness().ToList();
            slots[2] = slots[1];
            slots.RemoveAt(20);

            var problems = StageCatalogRules.Validate(slots);

            Assert.IsTrue(problems.Any(p => p.Contains("항목 수 20개")));
            Assert.IsTrue(problems.Any(p => p.Contains("id 중복 1-1")));
            Assert.IsTrue(problems.Any(p => p.Contains("월드 3 항목 6개")));
            Assert.IsTrue(problems.Any(p => p.Contains("뒤 순서")));
        }
    }

    public class StageNamingTests {
        [TestCase(1, 0, "1-T", "튜토리얼.exe")]
        [TestCase(1, 1, "1-1", "1-1.exe")]
        [TestCase(2, 6, "2-6", "2-6.exe")]
        [TestCase(3, 0, "3-T", "튜토리얼.exe")]
        public void IdAndFileName(int world, int number, string id, string fileName) {
            Assert.AreEqual(id, StageNaming.Id(world, number));
            Assert.AreEqual(fileName, StageNaming.FileName(world, number));
        }

        [TestCase("1-T", true, 1, 0)]
        [TestCase("3-6", true, 3, 6)]
        [TestCase("3-7", false, 0, 0)]
        [TestCase("1-0", false, 0, 0)]
        [TestCase("x-1", false, 0, 0)]
        [TestCase("", false, 0, 0)]
        public void TryParse(string id, bool ok, int world, int number) {
            Assert.AreEqual(ok, StageNaming.TryParse(id, out var w, out var n));
            if (!ok) return;
            Assert.AreEqual(world, w);
            Assert.AreEqual(number, n);
        }

        [Test]
        public void WorldPathUsesBackslashes() {
            Assert.AreEqual(@"C:\GAME\STAGES\STAGE_1_뮤직플레이어", StageNaming.WorldPath(StagePlan.Worlds[0].FolderName));
        }

        [TestCase(0d, "00:00")]
        [TestCase(59.99d, "00:59")]
        [TestCase(61d, "01:01")]
        [TestCase(-3d, "00:00")]
        public void FormatTime(double seconds, string text) {
            Assert.AreEqual(text, StageNaming.FormatTime(seconds));
        }
    }
}
