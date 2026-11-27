using System.Collections.Generic;
using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportRuleTests {
        private sealed class FakeRules : IKeyRuleSource {
            private readonly Dictionary<string, KeyRule> _rules;

            public FakeRules(Dictionary<string, KeyRule> rules) {
                _rules = rules;
            }

            public bool TryGet(string key, out KeyRule rule) => _rules.TryGetValue(key, out rule);
        }

        [Test]
        public void SlotIds_WithTutorial() {
            var result = StageIdAssigner.Assign(StageImportSample.Load(), new SlotIdRule(true, "T"));
            CollectionAssert.AreEqual(new[] { "1-T", "1-1", "2-T" }, result.Assigned.ConvertAll(a => a.StageId));
            CollectionAssert.AreEqual(new[] { "m_tut", "m_ghost", "m_exp" }, result.Assigned.ConvertAll(a => a.MapId));
            CollectionAssert.AreEqual(new[] { 1, 1, 2 }, result.Assigned.ConvertAll(a => a.World));
            CollectionAssert.AreEqual(new[] { "m_scratch" }, result.Unslotted);
            CollectionAssert.AreEqual(new[] { "m_missing" }, result.MissingInStages);
        }

        [Test]
        public void SlotIds_WithoutTutorial() {
            var result = StageIdAssigner.Assign(StageImportSample.Load(), new SlotIdRule(false, "T"));
            CollectionAssert.AreEqual(new[] { "1-1", "1-2", "2-1" }, result.Assigned.ConvertAll(a => a.StageId));
        }

        [TestCase(0, "3-T")]
        [TestCase(1, "3-1")]
        [TestCase(6, "3-6")]
        public void IdFor_TutorialFirst(int index, string expected) {
            Assert.AreEqual(expected, StageIdAssigner.IdFor(3, index, new SlotIdRule(true, "T")));
        }

        [TestCase("solid", CustomOutcome.Fallback, "ground")]
        [TestCase("oneway", CustomOutcome.Fallback, "platform")]
        [TestCase("hazard", CustomOutcome.Fallback, "spike")]
        [TestCase("none", CustomOutcome.Skipped, null)]
        [TestCase("slippery", CustomOutcome.Unknown, null)]
        public void Custom_FallsBackByCol(string col, CustomOutcome outcome, string key) {
            var result = CustomBlockResolver.Resolve(new CustomBlock("c_x", "X", "#fff", col), NoExtraRules.Instance);
            Assert.AreEqual(outcome, result.Outcome);
            Assert.AreEqual(key, result.Key);
        }

        [Test]
        public void Custom_PrefersMappedName() {
            var rules = new FakeRules(new Dictionary<string, KeyRule> { { "Wall", new KeyRule(Placement.Single) } });
            var result = CustomBlockResolver.Resolve(new CustomBlock("c_wall", "Wall", "", "solid"), rules);
            Assert.AreEqual(CustomOutcome.MappedByName, result.Outcome);
            Assert.AreEqual("Wall", result.Key);
            Assert.AreEqual(Placement.Single, result.Rule.Placement);
        }

        [Test]
        public void Custom_MissingDefinitionIsUnknown() {
            Assert.AreEqual(CustomOutcome.Unknown, CustomBlockResolver.Resolve(null, NoExtraRules.Instance).Outcome);
        }
    }
}
