using System.Linq;
using LDY.StageSelect.Core;
using NUnit.Framework;

namespace LDY.Tests.StageSelect {
    public class TreeLinesTests {
        private static readonly TreeMetrics Metrics = new TreeMetrics(15, 3, 16, 16, 16);

        // 선택 화면 트리: 내 컴퓨터 > 로컬 디스크 > GAME > STAGES > 월드 3개 + 설정.ini
        private static readonly int[] Explorer = { 0, 1, 2, 3, 4, 4, 4, 4 };

        private static TreeSegment Vertical(TreeDrawing drawing, int x) =>
            drawing.Segments.Single(s => s.Axis == LineAxis.Vertical && s.X == x);

        [Test]
        public void IconXGrowsByIndentPerDepth() {
            Assert.AreEqual(15, Metrics.IconX(0));
            Assert.AreEqual(31, Metrics.IconX(1));
            Assert.AreEqual(79, Metrics.IconX(4));
            Assert.AreEqual(Metrics.IconCenterX(3), Metrics.ConnectorX(4));
        }

        [Test]
        public void HorizontalRunsFromParentCenterToChildIcon() {
            var drawing = TreeLines.Build(Explorer, Metrics);
            for (var row = 1; row < Explorer.Length; row++) {
                var depth = Explorer[row];
                var line = drawing.Segments.Single(s => s.Axis == LineAxis.Horizontal && s.Y == Metrics.RowMid(row));
                Assert.AreEqual(Metrics.IconCenterX(depth - 1), line.X, "row " + row);
                Assert.AreEqual(Metrics.IconX(depth) - 1, line.EndX, "row " + row);
            }
        }

        [Test]
        public void RootHasNoHorizontalLine() {
            var drawing = TreeLines.Build(Explorer, Metrics);
            Assert.IsFalse(drawing.Segments.Any(s => s.Axis == LineAxis.Horizontal && s.Y == Metrics.RowMid(0)));
        }

        [Test]
        public void VerticalStartsUnderParentIconAndEndsAtLastChildMiddle() {
            var drawing = TreeLines.Build(Explorer, Metrics);
            var stages = Vertical(drawing, Metrics.IconCenterX(3));
            Assert.AreEqual(Metrics.RowTop(4), stages.Y);
            Assert.AreEqual(Metrics.RowMid(7), stages.EndY);
        }

        [Test]
        public void LastChildEndsAsCorner() {
            // 0
            // ├ 1   (자식 있음)
            // │ ├ 2
            // │ └ 2
            // └ 1
            var depths = new[] { 0, 1, 2, 2, 1 };
            var drawing = TreeLines.Build(depths, Metrics);

            var root = Vertical(drawing, Metrics.IconCenterX(0));
            Assert.AreEqual(Metrics.RowMid(4), root.EndY, "루트 세로선은 마지막 자식(4행)에서 끝난다");

            var inner = Vertical(drawing, Metrics.IconCenterX(1));
            Assert.AreEqual(Metrics.RowMid(3), inner.EndY, "안쪽 세로선은 3행에서 └로 끝난다");

            var corner = drawing.Segments.Single(s => s.Axis == LineAxis.Horizontal && s.Y == Metrics.RowMid(3));
            Assert.AreEqual(inner.X, corner.X, "└의 가로선은 세로선 끝 픽셀에서 시작한다");
            Assert.AreEqual(inner.EndY, corner.Y);
        }

        [Test]
        public void SiblingSubtreeDoesNotExtendParentLinePastLastChild() {
            Assert.AreEqual(4, TreeLines.LastChild(new[] { 0, 1, 2, 2, 1 }, 0));
            Assert.AreEqual(3, TreeLines.LastChild(new[] { 0, 1, 2, 2, 1 }, 1));
            Assert.AreEqual(-1, TreeLines.LastChild(new[] { 0, 1, 2, 2, 1 }, 2));
            Assert.AreEqual(-1, TreeLines.LastChild(new[] { 0, 1, 1 }, 2));
        }

        [Test]
        public void BoxesOnlyForNodesWithChildren() {
            var drawing = TreeLines.Build(Explorer, Metrics);
            Assert.AreEqual(new[] { 0, 1, 2, 3 }, drawing.Boxes.Select(b => b.Row).ToArray());
        }

        [Test]
        public void BoxIsCenteredOnConnectorAndRowMiddle() {
            var drawing = TreeLines.Build(Explorer, Metrics);
            foreach (var box in drawing.Boxes) {
                var depth = Explorer[box.Row];
                Assert.AreEqual(Metrics.ConnectorX(depth), box.X + TreeLines.BoxSize / 2);
                Assert.AreEqual(Metrics.RowMid(box.Row), box.Y + TreeLines.BoxSize / 2);
                Assert.GreaterOrEqual(box.X, 2, "패널 테두리(2px) 안쪽");
                Assert.Less(box.X + TreeLines.BoxSize - 1, Metrics.IconX(depth), "상자가 아이콘을 덮지 않는다");
            }
        }

        [Test]
        public void DotsAlternateAlongBothAxes() {
            Assert.IsTrue(TreeLines.IsLit(0, 0));
            Assert.IsFalse(TreeLines.IsLit(1, 0));
            Assert.IsFalse(TreeLines.IsLit(0, 1));
            Assert.IsTrue(TreeLines.IsLit(1, 1));
            Assert.IsTrue(TreeLines.IsLit(23, 11));
        }

        [Test]
        public void PhaseIsSharedAtJunctions() {
            // 세로선 위 한 점과 그 점에서 시작하는 가로선이 같은 켜짐 값을 갖고, 양쪽 이웃은 반대 값이다.
            var drawing = TreeLines.Build(Explorer, Metrics);
            foreach (var h in drawing.Segments.Where(s => s.Axis == LineAxis.Horizontal)) {
                var v = drawing.Segments.Single(s => s.Axis == LineAxis.Vertical && s.X == h.X);
                Assert.That(h.Y, Is.InRange(v.Y, v.EndY));
                var here = TreeLines.IsLit(h.X, h.Y);
                Assert.AreNotEqual(here, TreeLines.IsLit(h.X + 1, h.Y));
                Assert.AreNotEqual(here, TreeLines.IsLit(h.X, h.Y + 1));
                Assert.AreNotEqual(here, TreeLines.IsLit(h.X, h.Y - 1));
            }
        }
    }
}
