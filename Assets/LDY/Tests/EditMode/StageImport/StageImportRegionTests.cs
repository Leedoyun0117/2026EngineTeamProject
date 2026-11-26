using System.Collections.Generic;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportRegionTests {
        private static bool[,] Mask(params string[] rowsTopDown) {
            var h = rowsTopDown.Length;
            var w = rowsTopDown[0].Length;
            var mask = new bool[w, h];
            for (var ty = 0; ty < h; ty++)
            for (var x = 0; x < w; x++)
                mask[x, h - 1 - ty] = rowsTopDown[ty][x] == '#';
            return mask;
        }

        [Test]
        public void HorizontalRuns_SplitsByGapAndRow() {
            var m = Mask(
                "##.###",
                ".#....");
            var runs = RegionFinder.HorizontalRuns(6, 2, (x, y) => m[x, y]);
            CollectionAssert.AreEqual(new[] {
                new GridRect(1, 0, 1, 1), new GridRect(0, 1, 2, 1), new GridRect(3, 1, 3, 1)
            }, runs);
        }

        [Test]
        public void Connected4_IgnoresDiagonals() {
            var m = Mask(
                "#.#",
                ".##",
                "#..");
            var regions = RegionFinder.Connected4(3, 3, (x, y) => m[x, y]);
            Assert.AreEqual(3, regions.Count);
            var sizes = new List<int>();
            foreach (var r in regions) sizes.Add(r.Count);
            sizes.Sort();
            CollectionAssert.AreEqual(new[] { 1, 1, 3 }, sizes);
        }

        [Test]
        public void RectCover_MergesSameWidthRowsAndCoversEveryCell() {
            var cells = new List<GridPoint> {
                new GridPoint(0, 0), new GridPoint(1, 0),
                new GridPoint(0, 1), new GridPoint(1, 1),
                new GridPoint(0, 2)
            };
            var rects = RectCover.Cover(cells);
            CollectionAssert.AreEqual(new[] { new GridRect(0, 0, 2, 2), new GridRect(0, 2, 1, 1) }, rects);
        }

        [Test]
        public void RectCover_LShapeHasNoOverlap() {
            var cells = new List<GridPoint> { new GridPoint(0, 0), new GridPoint(0, 1), new GridPoint(1, 1), new GridPoint(2, 1) };
            var rects = RectCover.Cover(cells);
            var area = 0;
            foreach (var r in rects) area += r.Width * r.Height;
            Assert.AreEqual(cells.Count, area);
        }
    }
}
