using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerPrefabMerger {
        private const string SpritePath = PlayerVisualContext.PinkName + "/GripPivot/Sprite";

        public static bool Run(MergeLog log) {
            var baseRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPaths.Jibin);
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPaths.Shared);
            try {
                var sprite = root.transform.Find(SpritePath)?.GetComponent<SpriteRenderer>();
                if (sprite == null) {
                    log.Notes.Add($"중단: {PlayerPrefabPaths.Shared} 에 {SpritePath} 의 SpriteRenderer가 없음. "
                                  + "'LDY/Player Visual/Setup Rope Grip'을 먼저 실행하세요");
                    return false;
                }

                new ChildSync(baseRoot.transform, root.transform, log).Run();
                new ComponentSync(baseRoot, root, log).Run();
                new LegacyVisualSwitch(root.transform, log).Run();
                new StageComponents(root, sprite, log).Run();
                new VisualPlacer(root, sprite, log).Run();

                if (log.HasChanges)
                    PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPaths.Shared);

                return true;
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
                PrefabUtility.UnloadPrefabContents(baseRoot);
            }
        }
    }
}
