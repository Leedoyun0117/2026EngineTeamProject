using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using LDY.Stage;
using NUnit.Framework;
using UnityEditor;

namespace LDY.Tests.Flow {
    // 실제 카탈로그 에셋 검사. LDY/Flow/Build Flow Assets를 돌리기 전이면 결론 없음으로 끝난다.
    public class StageCatalogAssetTests {
        private const string CatalogPath = "Assets/LDY/Data/Flow/StageCatalog.asset";
        private const string PickupPath = "Assets/LDY/Data/Stage/Stage1PickupCatalog.asset";

        private static StageCatalog LoadCatalog() {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(CatalogPath);
            if (catalog == null) Assert.Inconclusive(CatalogPath + " 없음");
            return catalog;
        }

        [Test]
        public void CatalogPassesRules() {
            CollectionAssert.IsEmpty(StageCatalogRules.Validate(LoadCatalog().Slots()));
        }

        [Test]
        public void CatalogHasThreeWorldsWithPaths() {
            var catalog = LoadCatalog();

            Assert.AreEqual(3, catalog.Worlds.Count);
            Assert.AreEqual(@"C:\GAME\STAGES\STAGE_1_뮤직플레이어", catalog.World(1).FolderPath);
        }

        [Test]
        public void FileNamesFollowNamingRule() {
            foreach (var entry in LoadCatalog().Entries)
                Assert.AreEqual(StageNaming.FileName(entry.world, entry.number), entry.fileName, entry.id);
        }

        [Test]
        public void EmptySceneMeansNotReady() {
            foreach (var entry in LoadCatalog().Entries.Where(e => string.IsNullOrEmpty(e.scenePath)))
                Assert.IsFalse(entry.IsReady, entry.id);
        }

        [Test]
        public void AbilitiesExistInPickupCatalog() {
            var pickups = AssetDatabase.LoadAssetAtPath<Stage1PickupCatalog>(PickupPath);
            if (pickups == null) Assert.Inconclusive(PickupPath + " 없음");

            foreach (var entry in LoadCatalog().Entries)
            foreach (var ability in entry.abilities)
                Assert.IsTrue(pickups.ContainsId(ability), $"{entry.id}: {ability}");
        }
    }
}
