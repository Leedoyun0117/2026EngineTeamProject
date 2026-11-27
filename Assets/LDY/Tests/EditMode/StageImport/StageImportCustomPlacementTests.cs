using System.Linq;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportCustomPlacementTests {
        private const string Door = "벽 L이랑 R을 둘다 먹어야지 열리는 벽";

        private static CustomNameIndex Index() {
            var index = new CustomNameIndex();
            index.Add(new CustomRule("벽", CustomBinding.Group4, CustomSizing.Stretch, false, CustomTileReplace.Ground), null);
            index.Add(new CustomRule("사슬", CustomBinding.Group4, CustomSizing.Stretch, false, CustomTileReplace.None, true),
                null);
            index.Add(new CustomRule("대포", CustomBinding.Group4, CustomSizing.OneAtLeft, true), null);
            index.Add(new CustomRule(Door, CustomBinding.Group4, CustomSizing.OnePerGroup, true), null);
            index.Add(new CustomRule("L", CustomBinding.PerCell, CustomSizing.Stretch, false), null);
            index.Add(new CustomRule("R", CustomBinding.PerCell, CustomSizing.Stretch, false), null);
            return index;
        }

        private static readonly CustomBlock[] Blocks = {
            new CustomBlock("c_w", "벽", "", "solid"),
            new CustomBlock("c_c", "사슬", "", "none"),
            new CustomBlock("c_k", "대포", "", "solid"),
            new CustomBlock("c_d", Door, "", "solid"),
            new CustomBlock("c_l", "L", "", "none"),
            new CustomBlock("c_r", "R", "", "none")
        };

        // 한 줄짜리 맵. 칸 문자열 그대로(빈 칸은 "").
        private static ImportPlan PlanRow(params string[] cells) {
            var stage = new ToolStage("m", "1-1", cells.Length, 1, cells, new int[cells.Length], new ToolNote[0], "",
                false, "");
            var file = new CursorStageFile(3, new[] { stage }, Blocks, new[] { new ToolSlot(new[] { "m" }, "") });
            return ImportPlanner.Plan(stage, file, NoExtraRules.Instance, Index());
        }

        private static GridPoint[] P(params int[] xy) =>
            Enumerable.Range(0, xy.Length / 2).Select(i => new GridPoint(xy[i * 2], xy[i * 2 + 1])).ToArray();

        [Test]
        public void OneAtLeft_PicksLeftmostCell() {
            var spot = GroupSpot.Pick(CustomSizing.OneAtLeft, P(5, 2, 3, 2, 4, 2));
            Assert.AreEqual(new GridRect(3, 2, 1, 1), spot.Rect);
        }

        [Test]
        public void OneAtLeft_TiesGoToLowerCell() {
            // ㄱ자 묶음: x=1 열에 두 칸이면 아래 칸
            var spot = GroupSpot.Pick(CustomSizing.OneAtLeft, P(1, 4, 2, 4, 1, 3));
            Assert.AreEqual(new GridRect(1, 3, 1, 1), spot.Rect);
        }

        [Test]
        public void OnePerGroup_UsesBoundsCenter() {
            var spot = GroupSpot.Pick(CustomSizing.OnePerGroup, P(2, 1, 3, 1, 4, 1, 2, 2));
            Assert.AreEqual(new GridRect(2, 1, 3, 2), spot.Rect);
            Assert.AreEqual(3.5f, spot.Rect.CenterX);
        }

        [Test]
        public void HangFromTop_AnchorOnCeilingAndLengthIsCellCount() {
            var spot = GroupSpot.Pick(CustomSizing.HangFromTop, P(6, 1, 6, 2, 6, 3, 6, 4));
            Assert.AreEqual(new GridRect(6, 4, 1, 1), spot.Rect);
            // 맨 위 칸(y=4)의 윗변 가운데 = (6.5, 5)
            Assert.AreEqual(6.5f, spot.AnchorX);
            Assert.AreEqual(5f, spot.AnchorY);
            Assert.AreEqual(4, spot.Length);
        }

        [Test]
        public void Cannon_RowBecomesOneGroup() {
            var plan = PlanRow("c_k", "c_k", "c_k", "", "c_k");
            Assert.AreEqual(2, plan.Groups.Count);
            var left = plan.Groups.Select(g => GroupSpot.Pick(CustomSizing.OneAtLeft, g.Cells).Rect.X).OrderBy(x => x);
            CollectionAssert.AreEqual(new[] { 0, 4 }, left.ToArray());
        }

        [Test]
        public void TileReplace_PutsGroundTiles() {
            var plan = PlanRow("c_w", "c_w", "", "c_w");
            Assert.AreEqual(3, plan.Tiles.Count(t => t.Key == ToolKeys.Ground));
            Assert.AreEqual(0, plan.Groups.Count);
            Assert.AreEqual(0, plan.Prefabs.Count);
            Assert.AreEqual(0, plan.Unmapped.Count);
            var use = plan.Customs.Single(c => c.Id == "c_w");
            Assert.AreEqual(CustomOutcome.CustomTile, use.Outcome);
            Assert.AreEqual(ToolKeys.Ground, use.Key);
        }

        [TestCase(CustomTileReplace.Platform, "platform")]
        [TestCase(CustomTileReplace.Spike, "spike")]
        public void TileReplace_OtherTiles(CustomTileReplace tile, string key) {
            var custom = new CustomBlock("c_x", "x", "", "none");
            var index = new CustomNameIndex();
            index.Add(new CustomRule("x", CustomBinding.PerCell, CustomSizing.Stretch, true, tile), null);
            var result = CustomBlockResolver.Resolve(custom, index, NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomTile, result.Outcome);
            Assert.AreEqual(key, result.Key);
            Assert.AreEqual(Placement.Tile, result.Rule.Placement);
        }

        [Test]
        public void Decoration_WithoutArtIsSkipped() {
            var plan = PlanRow("c_c", "c_c");
            Assert.AreEqual(CustomOutcome.Skipped, plan.Customs.Single().Outcome);
            Assert.AreEqual(0, plan.Groups.Count + plan.Prefabs.Count + plan.Tiles.Count);
            Assert.AreEqual(0, plan.Unmapped.Count);
        }

        [Test]
        public void Decoration_WithArtIsPlaced() {
            var index = new CustomNameIndex();
            index.Add(new CustomRule("사슬", CustomBinding.Group4, CustomSizing.Stretch, true, CustomTileReplace.None, true),
                null);
            var result = CustomBlockResolver.Resolve(new CustomBlock("c_c", "사슬", "", "none"), index,
                NoExtraRules.Instance);
            Assert.AreEqual(CustomOutcome.CustomPrefab, result.Outcome);
        }

        [Test]
        public void Lr_AlignedWhenOneEach() {
            var lr = LrPairing.Judge(PlanRow("c_l", "", "c_d", "c_d", "", "c_r"), Door, "L", "R");
            Assert.AreEqual(LrStatus.Aligned, lr.Status);
            Assert.AreEqual(new GridRect(0, 0, 1, 1), lr.L);
            Assert.AreEqual(new GridRect(5, 0, 1, 1), lr.R);
            Assert.AreEqual(new GridRect(2, 0, 2, 1), lr.Door);
        }

        [Test]
        public void Lr_TouchingCellsCountAsOne() {
            var lr = LrPairing.Judge(PlanRow("c_l", "c_l", "c_d", "c_r"), Door, "L", "R");
            Assert.AreEqual(LrStatus.Aligned, lr.Status);
            Assert.AreEqual(new GridRect(0, 0, 2, 1), lr.L);
        }

        [Test]
        public void Lr_NotUsedWithoutButtons() {
            Assert.AreEqual(LrStatus.NotUsed, LrPairing.Judge(PlanRow("c_d", "c_d"), Door, "L", "R").Status);
        }

        [TestCase(new[] { "c_l", "", "c_d" }, "R 없음")]
        [TestCase(new[] { "c_l", "", "c_r" }, "LRDoor 없음")]
        [TestCase(new[] { "c_l", "", "c_l", "c_d", "c_r" }, "L 2곳")]
        [TestCase(new[] { "c_d", "", "c_d", "c_l", "c_r" }, "LRDoor 2개")]
        public void Lr_MismatchIsReported(string[] row, string reason) {
            var lr = LrPairing.Judge(PlanRow(row), Door, "L", "R");
            Assert.AreEqual(LrStatus.Mismatch, lr.Status);
            StringAssert.Contains(reason, lr.Reason);
        }
    }
}
