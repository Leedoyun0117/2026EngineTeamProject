using System.Linq;
using LDY.Stage;
using NUnit.Framework;

namespace LDY.Tests {
    public class StageLogicTests {
        private static readonly string[] ManifestOrder = {
            "ground_top_L", "ground_top_C", "ground_top_R", "ground_top_LR",
            "ground_mid_L", "ground_mid_C", "ground_mid_R", "ground_mid_LR",
            "ground_bottom_L", "ground_bottom_C", "ground_bottom_R", "ground_bottom_LR",
            "ground_strip_L", "ground_strip_C", "ground_strip_R", "ground_strip_LR",
            "platform_L", "platform_C", "platform_R", "platform_single",
            "spike_up", "spike_down", "spike_left", "spike_right"
        };

        [Test]
        public void Layout_MatchesManifestOrder() {
            CollectionAssert.AreEqual(ManifestOrder, TilesetLayout.AllNames().ToArray());
        }

        [Test]
        public void Ground_AllSixteenCombinationsMapToDistinctNames() {
            var names = new System.Collections.Generic.HashSet<string>();
            for (var bits = 0; bits < 16; bits++) {
                var a = new Adjacency((bits & 1) != 0, (bits & 2) != 0, (bits & 4) != 0, (bits & 8) != 0);
                names.Add(GroundTileNamer.Name(a));
            }

            Assert.AreEqual(16, names.Count);
        }

        [TestCase(false, true, false, true, "ground_top_L")]
        [TestCase(true, true, true, true, "ground_mid_C")]
        [TestCase(true, false, true, false, "ground_bottom_R")]
        [TestCase(false, false, false, false, "ground_strip_LR")]
        [TestCase(false, true, true, true, "ground_top_C")]
        [TestCase(true, true, false, false, "ground_mid_LR")]
        public void Ground_NamesFromNeighbors(bool up, bool down, bool left, bool right, string expected) {
            Assert.AreEqual(expected, GroundTileNamer.Name(new Adjacency(up, down, left, right)));
        }

        [Test]
        public void Ground_AdjacencyOfRoundTrips() {
            for (var v = 0; v < 4; v++)
            for (var h = 0; h < 4; h++)
                Assert.AreEqual(GroundTileNamer.Name(v, h), GroundTileNamer.Name(GroundTileNamer.AdjacencyOf(v, h)));
        }

        [TestCase(false, true, "platform_L")]
        [TestCase(true, true, "platform_C")]
        [TestCase(true, false, "platform_R")]
        [TestCase(false, false, "platform_single")]
        public void Platform_NamesFromNeighbors(bool left, bool right, string expected) {
            Assert.AreEqual(expected, PlatformTileNamer.Name(left, right));
        }

        [Test]
        public void Layout_FlipsRowsToBottomOrigin() {
            var top = TilesetLayout.RectAt(0, 0, 40);
            var bottom = TilesetLayout.RectAt(3, 5, 40);
            Assert.AreEqual(200, top.Y);
            Assert.AreEqual(120, bottom.X);
            Assert.AreEqual(0, bottom.Y);
        }

        [Test]
        public void BlockKeyIndex_FindsKnownAndRejectsUnknown() {
            var index = new BlockKeyIndex(new[] { "ground", "platform", "spike" });
            Assert.IsTrue(index.TryFind("platform", out var i));
            Assert.AreEqual(1, i);
            Assert.IsFalse(index.TryFind("unknown", out _));
            Assert.IsFalse(index.TryFind(null, out _));
        }

        [Test]
        public void BlockKeyIndex_KeepsFirstOnDuplicate() {
            var index = new BlockKeyIndex(new[] { "a", "b", "a" });
            index.TryFind("a", out var i);
            Assert.AreEqual(0, i);
        }
    }
}
