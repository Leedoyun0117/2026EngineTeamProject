using System.Collections.Generic;
using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests.Flow {
    public class SubStagePlanTests {
        private static readonly string[] Known = {
            "Stage1-Tutorial", "Stage1-1", "Stage1-2", "Stage1-3", "Stage1-4", "Stage1-5", "Stage1-6"
        };

        private static readonly string[] Siblings = {
            "Stage1-6", "Stage1-1", "Stage1-2", "Stage1-4", "Stage1-3", "Stage1-5", "Stage1-Tutorial"
        };

        [TestCase(null)]
        [TestCase("")]
        public void EmptySubRootLoadsWholeScene(string subRoot) {
            var plan = SubStagePlan.For(subRoot, Siblings, Known);

            Assert.IsFalse(plan.UsesSubRoot);
            Assert.IsNull(plan.Activate);
            CollectionAssert.IsEmpty(plan.Deactivate);
        }

        [Test]
        public void SubRootTurnsOffOtherKnownSiblings() {
            var plan = SubStagePlan.For("Stage1-2", Siblings, Known);

            Assert.IsTrue(plan.UsesSubRoot);
            Assert.AreEqual("Stage1-2", plan.Activate);
            CollectionAssert.AreEquivalent(
                new[] { "Stage1-6", "Stage1-1", "Stage1-4", "Stage1-3", "Stage1-5", "Stage1-Tutorial" }, plan.Deactivate);
        }

        [Test]
        public void UnknownSiblingsAreLeftAlone() {
            var plan = SubStagePlan.For("Stage1-1", new[] { "Stage1-1", "Stage1-2", "Decor", "Level2" }, Known);

            CollectionAssert.AreEqual(new[] { "Stage1-2" }, plan.Deactivate);
        }

        [Test]
        public void StandalonePicksFirstActiveInCatalogOrder() {
            Assert.AreEqual("Stage1-3", SubStagePlan.PickActive(Known, new HashSet<string> { "Stage1-5", "Stage1-3" }));
            Assert.IsNull(SubStagePlan.PickActive(Known, new HashSet<string>()));
        }
    }

    public class StageObjectNamesTests {
        [TestCase("Stage1_GoalMusicFile", true)]
        [TestCase("Stage1_GoalMusicFile (2)", true)]
        [TestCase("Stage1_GoalMusicFile(Clone)", true)]
        [TestCase("Stage1_GoalMusicFileX", false)]
        [TestCase("Goal", false)]
        public void GoalIsRecognisedByPrefabName(string name, bool goal) {
            Assert.AreEqual(goal, StageObjectNames.IsGoal(name));
        }

        [TestCase("Start", 1)]
        [TestCase("spawn", 1)]
        [TestCase("StartMarker", 1)]
        [TestCase("Stage1_Start", 2)]
        [TestCase("Stage1_Start (1)", 2)]
        [TestCase("start_marker", 2)]
        [TestCase("Start (1)", 1)]
        [TestCase("Ground", -1)]
        [TestCase("Restart", -1)]
        public void SpawnRankByName(string name, int rank) {
            Assert.AreEqual(rank, StageObjectNames.SpawnRank(new SpawnCandidate(name, false)));
        }

        [Test]
        public void ComponentBeatsNames() {
            var candidates = new[] {
                new SpawnCandidate("Start", false),
                new SpawnCandidate("Stage1_Start", false),
                new SpawnCandidate("Anything", true)
            };

            Assert.AreEqual(2, StageObjectNames.BestSpawn(candidates));
        }

        [Test]
        public void NameBeatsMarkerPrefabAndFirstWinsTies() {
            var candidates = new[] {
                new SpawnCandidate("Stage1_Start", false),
                new SpawnCandidate("Spawn", false),
                new SpawnCandidate("Start", false)
            };

            Assert.AreEqual(1, StageObjectNames.BestSpawn(candidates));
        }

        [Test]
        public void NoCandidateGivesMinusOne() {
            Assert.AreEqual(-1, StageObjectNames.BestSpawn(new[] { new SpawnCandidate("Ground", false) }));
            Assert.AreEqual(-1, StageObjectNames.BestSpawn(new SpawnCandidate[0]));
        }
    }
}
