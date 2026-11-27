using System.Linq;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportPlanTests {
        private static ImportPlan PlanOf(string mapId) {
            var file = StageImportSample.Load();
            return ImportPlanner.Plan(file.FindStage(mapId), file, NoExtraRules.Instance);
        }

        [Test]
        public void Tutorial_TilesStartGoalAndRun() {
            var plan = PlanOf("m_tut");
            Assert.AreEqual(10, plan.Tiles.Count(t => t.Key == "ground"));
            Assert.AreEqual(3, plan.Tiles.Count(t => t.Key == "spike"));
            CollectionAssert.AreEqual(new[] { new GridPoint(1, 2) }, plan.Starts);
            CollectionAssert.AreEqual(new[] { new GridPoint(6, 3) }, plan.Goals);

            var run = plan.Prefabs.Single(p => p.Key == "mov_h");
            Assert.AreEqual(new GridRect(3, 2, 3, 1), run.Rect);
            var pickup = plan.Prefabs.Single(p => p.Key == "abs_drag");
            Assert.AreEqual(GridRect.Cell(new GridPoint(2, 3)), pickup.Rect);

            Assert.AreEqual(0, plan.Areas.Count);
            Assert.AreEqual(0, plan.Unmapped.Count);
            Assert.AreEqual(19, plan.BlockCount);
        }

        [Test]
        public void Ghost_GroupsByKeyIn4Directions() {
            var plan = PlanOf("m_ghost");
            var ghost = plan.Groups.Single(g => g.Key == "ghost");
            CollectionAssert.AreEquivalent(new[] { new GridPoint(1, 1), new GridPoint(1, 2), new GridPoint(2, 2) }, ghost.Cells);
            var hidden = plan.Groups.Single(g => g.Key == ToolKeys.GhostHidden);
            CollectionAssert.AreEqual(new[] { new GridPoint(4, 2) }, hidden.Cells);
        }

        [Test]
        public void Ghost_DarkAndTabAreas() {
            var plan = PlanOf("m_ghost");
            var dark = plan.Areas.Where(a => a.Kind == AreaKind.Dark).OrderBy(a => a.Cells.Count).ToList();
            Assert.AreEqual(2, dark.Count);
            CollectionAssert.AreEqual(new[] { new GridPoint(0, 0) }, dark[0].Cells);
            CollectionAssert.AreEquivalent(new[] { new GridPoint(4, 2), new GridPoint(4, 3), new GridPoint(5, 3) }, dark[1].Cells);
            Assert.AreEqual(3, dark[1].Rects.Sum(r => r.Width * r.Height));

            var tab = plan.Areas.Single(a => a.Kind == AreaKind.Tab);
            CollectionAssert.AreEqual(new[] { new GridRect(5, 1, 1, 2) }, tab.Rects);
        }

        [Test]
        public void Ghost_CustomsUnmappedNotesAndDecorations() {
            var plan = PlanOf("m_ghost");
            Assert.AreEqual(2, plan.Tiles.Count(t => t.Key == "ground"));
            Assert.AreEqual(1, plan.Tiles.Count(t => t.Key == "platform"));
            Assert.AreEqual(1, plan.Tiles.Count(t => t.Key == "spike"));
            Assert.AreEqual(new GridPoint(5, 3), plan.Tiles.Single(t => t.Key == "spike").Cell);

            var wall = plan.Customs.Single(c => c.Id == "c_wall");
            Assert.AreEqual(CustomOutcome.Fallback, wall.Outcome);
            Assert.AreEqual(2, wall.Count);
            Assert.AreEqual(CustomOutcome.Skipped, plan.Customs.Single(c => c.Id == "c_deco").Outcome);

            CollectionAssert.AreEqual(new[] { "laser" }, plan.Unmapped.Keys);
            Assert.AreEqual(1, plan.Prefabs.Count(p => p.Key == "abs_resize"));
            CollectionAssert.AreEqual(new[] { new GridPoint(0, 3) }, plan.Decorations.Select(d => d.Cell));
            Assert.AreEqual(new GridPoint(1, 2), plan.Notes.Single().Cell);
            Assert.AreEqual("고스트 묶음", plan.Notes.Single().Text);
        }

        [Test]
        public void MappedCustomName_UsesSourceRule() {
            var file = StageImportSample.Load();
            var rules = new MappedPlank();
            var plan = ImportPlanner.Plan(file.FindStage("m_ghost"), file, rules);
            Assert.AreEqual(0, plan.Tiles.Count(t => t.Key == "platform"));
            Assert.AreEqual(1, plan.Prefabs.Count(p => p.Key == "Plank"));
        }

        private sealed class MappedPlank : IKeyRuleSource {
            public bool TryGet(string key, out KeyRule rule) {
                rule = new KeyRule(Placement.Single);
                return key == "Plank";
            }
        }
    }
}
