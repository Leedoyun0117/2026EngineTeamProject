using System.Collections.Generic;
using System.Linq;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportCustomNameTests {
        private static CustomNameIndex Index() {
            var index = new CustomNameIndex();
            index.Add(new CustomRule("거미", CustomBinding.Group4, CustomSizing.Stretch, false), null);
            index.Add(new CustomRule("360도 회전 블록", CustomBinding.HorizontalRun, CustomSizing.OnePerGroup, true),
                new[] { "360도 회전하는 블록" });
            index.Add(new CustomRule("버튼", CustomBinding.PerCell, CustomSizing.Stretch, true), null);
            index.Add(new CustomRule("올라가면 보이는 영역", CustomBinding.Area, CustomSizing.Stretch, false), null);
            return index;
        }

        [TestCase("거미?", "거미")]
        [TestCase(" 거 미 ？", "거미")]
        [TestCase("Wall Block", "wallblock")]
        [TestCase("떨어지는 오브젝트 (피해야함)", "떨어지는오브젝트(피해야함)")]
        [TestCase("", "")]
        [TestCase((string)null, "")]
        public void Normalize_DropsSpacesQuestionMarksAndCase(string raw, string expected) {
            Assert.AreEqual(expected, CustomNames.Normalize(raw));
        }

        [TestCase("거미?")]
        [TestCase("거미")]
        [TestCase(" 거미 ? ")]
        public void Find_IgnoresSpacesAndQuestionMarks(string name) {
            Assert.IsTrue(Index().TryFind(name, out var rule));
            Assert.AreEqual("거미", rule.EntryName);
        }

        [Test]
        public void Find_AliasGivesEntryName() {
            Assert.IsTrue(Index().TryFind("360도 회전하는 블록", out var rule));
            Assert.AreEqual("360도 회전 블록", rule.EntryName);
            Assert.AreEqual(CustomBinding.HorizontalRun, rule.Binding);
            Assert.IsFalse(Index().TryFind("360도 회전", out _));
        }

        [Test]
        public void Find_IgnoresCase() {
            var index = new CustomNameIndex();
            index.Add(new CustomRule("Wall", CustomBinding.PerCell, CustomSizing.Stretch, true), new[] { "BIG wall" });
            Assert.IsTrue(index.TryFind("wALL", out _));
            Assert.IsTrue(index.TryFind("bigwall", out var rule));
            Assert.AreEqual("Wall", rule.EntryName);
        }

        [Test]
        public void Index_ReportsAliasUsedByTwoEntries() {
            var index = new CustomNameIndex();
            index.Add(new CustomRule("A", CustomBinding.PerCell, CustomSizing.Stretch, true), new[] { "공통" });
            index.Add(new CustomRule("B", CustomBinding.PerCell, CustomSizing.Stretch, true), new[] { "공 통" });
            Assert.IsTrue(index.TryFind("공통", out var rule));
            Assert.AreEqual("A", rule.EntryName);
            Assert.AreEqual(1, index.Collisions.Count);
        }

        [Test]
        public void Resolve_MatchesByNameNotId() {
            var index = Index();
            var spider = CustomBlockResolver.Resolve(new CustomBlock("c_zzz", "거미?", "#000", "none"), index,
                NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomPlaceholder, spider.Outcome);
            Assert.AreEqual(CustomKey.Make("거미", "none"), spider.Key);
            Assert.AreEqual(Placement.Group4, spider.Rule.Placement);

            var rotate = CustomBlockResolver.Resolve(new CustomBlock("c_x", "360도 회전하는 블록", "", "oneway"), index,
                NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomPrefab, rotate.Outcome);
            Assert.AreEqual(CustomKey.Make("360도 회전 블록", "oneway"), rotate.Key);

            var area = CustomBlockResolver.Resolve(new CustomBlock("c_a", "올라가면 보이는 영역", "", "none"), index,
                NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomArea, area.Outcome);
            Assert.AreEqual(Placement.Area, area.Rule.Placement);
            Assert.AreEqual(AreaKind.Named, area.Rule.Area);
        }

        [Test]
        public void Resolve_NotInMapFallsBackByCol() {
            var result = CustomBlockResolver.Resolve(new CustomBlock("c_q", "처음 보는 블록", "", "solid"), Index(),
                NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.Fallback, result.Outcome);
            Assert.AreEqual(ToolKeys.Ground, result.Key);
        }

        [TestCase("버튼", "solid")]
        [TestCase("거미", "none")]
        [TestCase("a|b", "hazard")]
        public void Key_RoundTrips(string name, string col) {
            Assert.IsTrue(CustomKey.TryParse(CustomKey.Make(name, col), out var n, out var c));
            Assert.AreEqual(name, n);
            Assert.AreEqual(col, c);
            Assert.IsFalse(CustomKey.TryParse("ground", out _, out _));
        }

        [Test]
        public void SameNameDifferentCol_ReportedAndBothGoToSameEntry() {
            var customs = new[] {
                new CustomBlock("c_1", "버튼", "", "solid"),
                new CustomBlock("c_2", "버튼 ", "", "none"),
                new CustomBlock("c_3", "버튼", "", "solid"),
                new CustomBlock("c_4", "거미?", "", "none")
            };
            var index = Index();
            var conflict = CustomColConflicts.Find(customs, index).Single();
            Assert.AreEqual("버튼", conflict.EntryName);
            CollectionAssert.AreEqual(new[] { "solid", "none" }, conflict.Cols);
            CollectionAssert.AreEqual(new[] { "c_1", "c_2", "c_3" }, conflict.CustomIds);

            var solid = CustomBlockResolver.Resolve(customs[0], index, NoExtraRules.Instance);
            var none = CustomBlockResolver.Resolve(customs[1], index, NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomPrefab, solid.Outcome);
            Assert.AreEqual(CustomOutcome.CustomPrefab, none.Outcome);
            Assert.IsTrue(CustomKey.TryParse(solid.Key, out var solidName, out var solidCol));
            Assert.IsTrue(CustomKey.TryParse(none.Key, out var noneName, out var noneCol));
            Assert.AreEqual(solidName, noneName);
            Assert.AreEqual("solid", solidCol);
            Assert.AreEqual("none", noneCol);
        }

        [Test]
        public void Plan_GroupsAndNamedAreas() {
            // 3x2 맵: 위 행은 거미 2칸 + 영역 1칸, 아래 행은 영역 3칸
            var stage = new ToolStage("m", "1-1", 3, 2,
                new[] { "c_s", "c_s", "c_a", "c_a", "c_a", "c_a" }, new[] { 0, 0, 0, 0, 0, 0 }, new ToolNote[0], "",
                false, "");
            var file = new CursorStageFile(3, new[] { stage },
                new[] { new CustomBlock("c_s", "거미?", "", "none"), new CustomBlock("c_a", "올라가면 보이는 영역", "", "none") },
                new[] { new ToolSlot(new[] { "m" }, "") });
            var plan = ImportPlanner.Plan(stage, file, NoExtraRules.Instance, Index());

            var group = plan.Groups.Single();
            Assert.AreEqual(CustomKey.Make("거미", "none"), group.Key);
            Assert.AreEqual(2, group.Cells.Count);

            var area = plan.Areas.Single(a => a.Kind == AreaKind.Named);
            Assert.AreEqual(CustomKey.Make("올라가면 보이는 영역", "none"), area.Key);
            Assert.AreEqual(4, area.Cells.Count);
            Assert.AreEqual(0, plan.Unmapped.Count);
            Assert.AreEqual(CustomOutcome.CustomPlaceholder, plan.Customs.Single(c => c.Id == "c_s").Outcome);
        }

        [Test]
        public void RealFileNames_AllHaveDistinctNormalForms() {
            // stages.json에 실제로 쓰인 이름 중 정규화가 겹치는 것은 의도한 것(같은 이름)뿐이어야 한다.
            var names = new[] { "360도 회전 블록", "360도 회전하는 블록", "길게 누르기 버튼", "길게 누르는 버튼" };
            Assert.AreEqual(names.Length, new HashSet<string>(names.Select(CustomNames.Normalize)).Count);
        }
    }
}
