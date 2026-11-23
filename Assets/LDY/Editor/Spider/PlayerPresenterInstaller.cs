using UnityEditor;
using UnityEngine;

namespace LDY.SpiderEditor {
    // 공용 Player 프리팹에 사망 연출 컴포넌트를 붙인다. 코드가 아니라 프리팹 데이터만 바꾼다.
    internal static class PlayerPresenterInstaller {
        [MenuItem("LDY/Stage2/Install Death Presenter On Player Prefab")]
        public static void Run() {
            var path = SpiderPaths.PlayerPrefab;
            var contents = PrefabUtility.LoadPrefabContents(path);
            try {
                if (ExternalComponents.HasComponent(contents, ExternalComponents.DeathPresenter)) {
                    Debug.Log("[Stage2] 이미 붙어 있다: " + path);
                    return;
                }

                if (!ExternalComponents.TryAdd(contents, ExternalComponents.DeathPresenter))
                    return;

                PrefabUtility.SaveAsPrefabAsset(contents, path);
                Debug.Log("[Stage2] PlayerDeathPresenter를 붙였다: " + path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
