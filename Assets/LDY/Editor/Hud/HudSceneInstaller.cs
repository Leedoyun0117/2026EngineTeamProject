using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Hud.HudEditor {
    // 열린 씬에 HUD 프리팹 인스턴스가 없을 때만 하나 넣는다.
    internal static class HudSceneInstaller {
        public static void AddToOpenScene(HudLog log) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPaths.Prefab);
            if (prefab == null) {
                log.Warn(HudPaths.Prefab + " 없음. 3. Build Prefab 먼저 실행");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects()) {
                if (root.GetComponentInChildren<HudRoot>(true) == null)
                    continue;

                log.Info($"{scene.name}에 이미 HUD 있음({root.name}), 건너뜀");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Add HUD");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Info($"{scene.path}에 HUD 추가");
        }
    }
}
