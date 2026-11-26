using LDY.Flow.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class ManagerScanTests {
        private const string Target = "JJB.Script.Level.JJBLevelManager";

        [Test]
        public void TargetPresent_Found() {
            var scan = ManagerScan.Evaluate(new[] { "UnityEngine.Camera", Target, Target }, new[] { Target }, "LevelManager");
            Assert.That(scan.FoundAny, Is.True);
            Assert.That(scan.Matched, Is.EqualTo(new[] { Target }));
            Assert.That(scan.Similar, Is.Empty);
        }

        [Test]
        public void Renamed_NotFound_SimilarListed() {
            var scan = ManagerScan.Evaluate(new[] { "JJB.Script.Level.StageLevelManager", "Foo.Bar" }, new[] { Target },
                "LevelManager");
            Assert.That(scan.FoundAny, Is.False);
            Assert.That(scan.Similar, Is.EqualTo(new[] { "JJB.Script.Level.StageLevelManager" }));
        }

        [Test]
        public void HintMatchesShortNameOnly_IgnoreCase() {
            // 네임스페이스에만 들어간 조각은 비슷한 이름으로 보지 않는다.
            var scan = ManagerScan.Evaluate(new[] { "LevelManager.Tools.Helper", "A.levelmanagerX" }, new[] { Target },
                "LevelManager");
            Assert.That(scan.Similar, Is.EqualTo(new[] { "A.levelmanagerX" }));
        }

        [Test]
        public void EmptyHint_NoSimilar() {
            var scan = ManagerScan.Evaluate(new[] { "X.LevelManager" }, new[] { Target }, "");
            Assert.That(scan.Similar, Is.Empty);
        }

        [Test]
        public void NullInputs_Safe() {
            var scan = ManagerScan.Evaluate(null, null, null);
            Assert.That(scan.FoundAny, Is.False);
            Assert.That(scan.Similar, Is.Empty);
        }
    }
}
