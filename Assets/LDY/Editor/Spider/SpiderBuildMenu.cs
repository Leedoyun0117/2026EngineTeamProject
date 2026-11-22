using LDY.Spider;
using UnityEditor;
using UnityEngine;

namespace LDY.SpiderEditor {
    internal static class SpiderBuildMenu {
        [MenuItem("LDY/Stage2/Build Spider And Trap")]
        public static void Run() {
            if (!new Stage2ArtLoader().TryLoad(out var sprites, out var error)) {
                Debug.LogError("[Stage2] " + error);
                return;
            }

            var spiderSettings = SpiderPaths.LoadOrCreate<SpiderSettings>(SpiderPaths.Data + "/SpiderSettings.asset");
            var trapSettings = SpiderPaths.LoadOrCreate<TrapSettings>(SpiderPaths.Data + "/TrapSettings.asset");
            AssetDatabase.SaveAssets();

            var trap = TrapPrefabBuilder.Build(sprites, trapSettings);
            var spiderBuilder = new SpiderPrefabBuilder(sprites, spiderSettings);
            var spider = spiderBuilder.Build();
            new SpiderTestSceneBuilder().Build(spider, trap, spiderBuilder.RestHeight());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Stage2] 만듦: " + SpiderPrefabBuilder.Path + ", " + TrapPrefabBuilder.Path + ", " +
                      SpiderTestSceneBuilder.ScenePath);
        }
    }
}
