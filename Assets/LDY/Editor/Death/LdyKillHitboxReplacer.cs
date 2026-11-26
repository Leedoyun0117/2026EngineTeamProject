using System.Linq;
using HES.Script.Object;
using LDY.Flow;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // LDY 프리팹에 LDY 빌더가 직접 붙인 KillHitbox만 StageDeathZone(Hazard)으로 바꾼다.
    // 팀원 프리팹에서 물려받은 KillHitbox는 건드리지 않는다(PlayerDeath의 파괴 감지로 처리).
    internal static class LdyKillHitboxReplacer {
        private static readonly string[] Roots = { "Assets/LDY/Prefabs" };

        public static void Run() {
            var changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", Roots)) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var count = Replace(path);
                if (count == 0)
                    continue;

                changed++;
                Debug.Log($"[PlayerDeath] {path}: KillHitbox {count}개 → StageDeathZone");
            }

            Debug.Log($"[PlayerDeath] KillHitbox 교체 완료. 바뀐 프리팹 {changed}개");
        }

        private static int Replace(string path) {
            var contents = PrefabUtility.LoadPrefabContents(path);
            try {
                var owned = contents.GetComponentsInChildren<KillHitbox>(true).Where(IsOwnedHere).ToArray();
                foreach (var hitbox in owned) {
                    var go = hitbox.gameObject;
                    Object.DestroyImmediate(hitbox);
                    if (!go.TryGetComponent<StageDeathZone>(out _))
                        go.AddComponent<StageDeathZone>();
                }

                if (owned.Length > 0)
                    PrefabUtility.SaveAsPrefabAsset(contents, path);

                return owned.Length;
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static bool IsOwnedHere(Component component) =>
            !PrefabUtility.IsPartOfPrefabInstance(component) || PrefabUtility.IsAddedComponentOverride(component);
    }
}
