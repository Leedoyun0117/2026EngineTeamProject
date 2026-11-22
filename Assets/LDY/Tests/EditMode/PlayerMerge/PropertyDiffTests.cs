using System.Collections.Generic;
using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class PropertyDiffTests {
        [Test]
        public void Compare_ReportsDifferentAndOnlySidesSortedByKey() {
            var baseMap = new Dictionary<string, string> { ["b"] = "1", ["a"] = "same", ["c"] = "x" };
            var oursMap = new Dictionary<string, string> { ["b"] = "2", ["a"] = "same", ["d"] = "y" };

            var rows = PropertyDiff.Compare(baseMap, oursMap);

            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(("b", DiffKind.Different), (rows[0].Key, rows[0].Kind));
            Assert.AreEqual(("c", DiffKind.OnlyBase), (rows[1].Key, rows[1].Kind));
            Assert.AreEqual(("d", DiffKind.OnlyOurs), (rows[2].Key, rows[2].Kind));
            Assert.AreEqual("1", rows[0].BaseValue);
            Assert.AreEqual("2", rows[0].OursValue);
        }

        [Test]
        public void Compare_IdenticalMaps_IsEmpty() {
            var map = new Dictionary<string, string> { ["a"] = "1" };

            Assert.IsEmpty(PropertyDiff.Compare(map, new Dictionary<string, string>(map)));
        }

        [Test]
        public void Render_EscapesPipesAndListsRows() {
            var rows = new List<DiffRow> { new("k", "a|b", "c", DiffKind.Different) };

            var text = DiffMarkdown.Render(rows, "지빈이", "내 것");

            StringAssert.Contains("| k | Different | a\\|b | c |", text);
            StringAssert.Contains("지빈이", text);
        }
    }
}
