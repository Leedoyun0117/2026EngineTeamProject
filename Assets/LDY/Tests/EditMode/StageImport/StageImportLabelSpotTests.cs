using System.Linq;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportLabelSpotTests {
        private static GridPoint P(int x, int y) => new GridPoint(x, y);

        [Test]
        public void OneSpotPerConnectedGroupAtTopCenter() {
            var cells = new[] { P(2, 3), P(3, 3), P(4, 3), P(3, 4) };
            var spots = LabelSpots.PerGroup(cells);

            Assert.AreEqual(1, spots.Count);
            Assert.AreEqual(3.5f, spots[0].X, 1e-5f);
            Assert.AreEqual(5f, spots[0].Y, 1e-5f);
        }

        [Test]
        public void SeparateGroupsGetSeparateSpots() {
            var cells = new[] { P(0, 0), P(1, 0), P(5, 0), P(5, 1) };
            var spots = LabelSpots.PerGroup(cells).OrderBy(s => s.X).ToList();

            Assert.AreEqual(2, spots.Count);
            Assert.AreEqual(1f, spots[0].X, 1e-5f);
            Assert.AreEqual(1f, spots[0].Y, 1e-5f);
            Assert.AreEqual(5.5f, spots[1].X, 1e-5f);
            Assert.AreEqual(2f, spots[1].Y, 1e-5f);
        }

        [Test]
        public void DiagonalCellsAreNotConnected() {
            Assert.AreEqual(2, LabelSpots.PerGroup(new[] { P(0, 0), P(1, 1) }).Count);
        }

        [Test]
        public void DuplicateCellsCountOnceAndNegativeCoordinatesWork() {
            var spots = LabelSpots.PerGroup(new[] { P(-3, -2), P(-3, -2), P(-2, -2) });

            Assert.AreEqual(1, spots.Count);
            Assert.AreEqual(-2f, spots[0].X, 1e-5f);
            Assert.AreEqual(-1f, spots[0].Y, 1e-5f);
        }

        [Test]
        public void NoCellsNoSpots() {
            Assert.AreEqual(0, LabelSpots.PerGroup(new GridPoint[0]).Count);
        }
    }
}
