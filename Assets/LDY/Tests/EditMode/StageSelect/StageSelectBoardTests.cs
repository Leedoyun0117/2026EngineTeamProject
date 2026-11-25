using System.Linq;
using LDY.Flow.Core;
using LDY.StageSelect.Core;
using NUnit.Framework;

namespace LDY.Tests.StageSelect {
    public class PixelSpaceTests {
        private static readonly PixelSpace Space = new PixelSpace(640f, 360f, BoardMetrics.PixelsPerUnit, 0f, 0f);

        [Test]
        public void OrthographicSizeMatchesReferenceHeight() {
            Assert.AreEqual(5f, Space.OrthographicSize, 1e-5f);
        }

        [Test]
        public void CanvasCornersAndCenter() {
            var topLeft = Space.ToWorld(0f, 0f);
            Assert.AreEqual(-320f / 36f, topLeft.X, 1e-5f);
            Assert.AreEqual(5f, topLeft.Y, 1e-5f);

            var center = Space.ToWorld(320f, 180f);
            Assert.AreEqual(0f, center.X, 1e-5f);
            Assert.AreEqual(0f, center.Y, 1e-5f);

            var bottomRight = Space.ToWorld(640f, 360f);
            Assert.AreEqual(320f / 36f, bottomRight.X, 1e-5f);
            Assert.AreEqual(-5f, bottomRight.Y, 1e-5f);
        }

        [Test]
        public void CameraCenterOffsetsEverything() {
            var moved = new PixelSpace(640f, 360f, 36f, 10f, -2f).ToWorld(320f, 180f);
            Assert.AreEqual(10f, moved.X, 1e-5f);
            Assert.AreEqual(-2f, moved.Y, 1e-5f);
        }

        [Test]
        public void OnePixelIsOneOverPpu() {
            var a = Space.ToWorld(100f, 100f);
            var b = Space.ToWorld(101f, 101f);
            Assert.AreEqual(1f / 36f, b.X - a.X, 1e-5f);
            Assert.AreEqual(-1f / 36f, b.Y - a.Y, 1e-5f);
        }

        [TestCase(10.4f, 10f)]
        [TestCase(10.5f, 11f)]
        [TestCase(10.6f, 11f)]
        [TestCase(-0.5f, 0f)]
        [TestCase(-0.6f, -1f)]
        public void SnapsToWholePixels(float value, float expected) {
            Assert.AreEqual(expected, PixelSpace.Snap(value));
        }

        [Test]
        public void ConversionUsesSnappedPixels() {
            var snapped = Space.ToWorld(10f, 21f);
            var raw = Space.ToWorld(10.4f, 20.6f);
            Assert.AreEqual(snapped.X, raw.X, 1e-6f);
            Assert.AreEqual(snapped.Y, raw.Y, 1e-6f);
        }

        [Test]
        public void RectBecomesWorldBoxWithYUp() {
            var box = Space.ToWorld(new PixelRect(320f, 180f, 36f, 72f));
            Assert.AreEqual(0f, box.Left, 1e-5f);
            Assert.AreEqual(1f, box.Right, 1e-5f);
            Assert.AreEqual(0f, box.Top, 1e-5f);
            Assert.AreEqual(-2f, box.Bottom, 1e-5f);
        }
    }

    public class BoardPlanTests {
        // 선택 화면 기본 배치의 파일 목록 패널(StageSelectSceneBuilder: 창 (6,4), 패널 x 3+163+3, y 54+15, 456x241)
        private static readonly PixelRect Panel = new PixelRect(175f, 73f, 456f, 241f);
        private static readonly string[] Ids = { "1-T", "1-1", "1-2", "1-3", "1-4", "1-5", "1-6" };

        [Test]
        public void EachThumbnailTopIsAPlatform() {
            var plan = BoardPlanner.Plan(Panel, Ids, 16f);
            Assert.AreEqual(Ids.Length, plan.Files.Count);
            Assert.AreEqual(Ids, plan.Files.Select(f => f.Id).ToArray());

            var first = plan.Files[0];
            Assert.AreEqual(Panel.X + FileGridLayout.PaddingLeft + FileGridLayout.Thumbnail.X, first.Left);
            Assert.AreEqual(first.Left + FileGridLayout.Thumbnail.Width, first.Right);
            Assert.AreEqual(Panel.Y + FileGridLayout.PaddingTop + FileGridLayout.Thumbnail.Y, first.Top);

            var secondRow = plan.Files[FileGridLayout.Columns];
            Assert.AreEqual(first.Left, secondRow.Left);
            Assert.AreEqual(first.Top + FileGridLayout.RowPitch, secondRow.Top);
        }

        [Test]
        public void WallsHugThePanelFromOutside() {
            var plan = BoardPlanner.Plan(Panel, Ids, 16f);
            Assert.AreEqual(Panel.Bottom, plan.Floor.Y);
            Assert.AreEqual(Panel.Y, plan.Ceiling.Bottom);
            Assert.AreEqual(Panel.X, plan.LeftWall.Right);
            Assert.AreEqual(Panel.Right, plan.RightWall.X);
            Assert.AreEqual(Panel.Height, plan.LeftWall.Height);
            Assert.AreEqual(Panel.Width + 32f, plan.Floor.Width);
        }

        [Test]
        public void ItemsBelowTheFloorGetNoPlatform() {
            var many = Enumerable.Range(0, 12).Select(i => "s" + i).ToArray();
            var plan = BoardPlanner.Plan(Panel, many, 16f);
            Assert.IsTrue(plan.Files.All(f => f.Top < Panel.Bottom));
            Assert.Less(plan.Files.Count, many.Length);
        }

        [Test]
        public void EmptyGridStillHasFloorAndWalls() {
            var plan = BoardPlanner.Plan(Panel, new string[0], 16f);
            Assert.AreEqual(0, plan.Files.Count);
            Assert.AreEqual(Panel.Bottom, plan.Floor.Y);
        }

        [Test]
        public void PlatformsConvertToPixelAlignedWorldEdges() {
            var space = new PixelSpace(640f, 360f, 36f, 0f, 0f);
            var span = BoardPlanner.Plan(Panel, Ids, 16f).Files[0];
            var left = space.ToWorld(span.Left, span.Top);
            Assert.AreEqual((span.Left - 320f) / 36f, left.X, 1e-5f);
            Assert.AreEqual((180f - span.Top) / 36f, left.Y, 1e-5f);
        }

        [Test]
        public void RowsAndFloorAreWithinJumpReach() {
            // TitlePlayerTuning(점프 14, 유지 0.3초, 추가중력 30 / 0.15초 후) 최고 높이 약 3.17유닛.
            const float jumpPixels = 3.17f * BoardMetrics.PixelsPerUnit;
            var plan = BoardPlanner.Plan(Panel, Ids, 16f);
            var lowestTop = plan.Files.Max(f => f.Top);
            Assert.Less(FileGridLayout.RowPitch, jumpPixels - 8f);
            Assert.Less(Panel.Bottom - lowestTop, jumpPixels - 8f);
        }

        [Test]
        public void PlanFindsPlatformIndexById() {
            var plan = BoardPlanner.Plan(Panel, Ids, 16f);
            Assert.AreEqual(2, plan.IndexOf("1-2"));
            Assert.AreEqual(-1, plan.IndexOf("9-9"));
            Assert.AreEqual(-1, plan.IndexOf(null));
        }
    }

    public class StandSelectionTests {
        [Test]
        public void StandingPlatformBecomesSelection() {
            var plan = BoardPlanner.Plan(new PixelRect(0f, 0f, 456f, 241f), new[] { "a", "b", "c" }, 16f);
            Assert.AreEqual("b", StandSelection.Next("a", plan.Files[1].Id));
        }

        [Test]
        public void NoPlatformKeepsSelection() {
            Assert.AreEqual("a", StandSelection.Next("a", null));
            Assert.AreEqual("a", StandSelection.Next("a", ""));
            Assert.IsNull(StandSelection.Next(null, null));
        }
    }

    public class SpawnRuleTests {
        private static readonly string[] Ids = { "1-T", "1-1", "1-2", "1-3" };

        [Test]
        public void FirstIncompleteWhenNothingWasCleared() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Open, StageStatus.Locked, StageStatus.Locked };
            Assert.AreEqual(1, SpawnRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void JustClearedWins() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Cleared, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(1, SpawnRule.Pick(Ids, statuses, "1-1"));
        }

        [Test]
        public void UnknownClearedIdFallsBackToFirstIncomplete() {
            var statuses = new[] { StageStatus.Cleared, StageStatus.Cleared, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(2, SpawnRule.Pick(Ids, statuses, "9-9"));
        }

        [Test]
        public void LockedIsNotIncomplete() {
            var statuses = new[] { StageStatus.Locked, StageStatus.NotReady, StageStatus.Open, StageStatus.Locked };
            Assert.AreEqual(2, SpawnRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void AllClearedFallsBackToFirst() {
            var statuses = Enumerable.Repeat(StageStatus.Cleared, 4).ToArray();
            Assert.AreEqual(0, SpawnRule.Pick(Ids, statuses, null));
        }

        [Test]
        public void EmptyListHasNoSpawn() {
            Assert.AreEqual(-1, SpawnRule.Pick(new string[0], new StageStatus[0], "1-1"));
        }
    }
}
