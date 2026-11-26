using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LDY.FlowEditor {
    // 타이틀(LDY 소유)의 AsyncSceneLoader.sceneName을 선택 화면으로 바꾼다. 타입은 Assembly-CSharp에 있어 이름으로 찾는다.
    internal static class TitleConnector {
        private const string LoaderType = "LDY.Script.AsyncSceneLoader";

        public static void Run(FlowLog log) {
            if (!File.Exists(FlowPaths.TitleScene)) {
                log.Warn("타이틀 씬 없음 " + FlowPaths.TitleScene);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(FlowPaths.TitleScene, OpenSceneMode.Single);
            var target = Path.GetFileNameWithoutExtension(FlowPaths.SelectScene);
            var found = false;
            foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                if (behaviour == null || behaviour.GetType().FullName != LoaderType) continue;
                found = true;
                var so = new SerializedObject(behaviour);
                var property = so.FindProperty("sceneName");
                if (property.stringValue == target) {
                    log.Info("이미 연결됨: sceneName = " + target);
                    continue;
                }

                log.Info($"sceneName {property.stringValue} -> {target}");
                property.stringValue = target;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
            }

            if (!found) log.Warn("타이틀 씬에서 AsyncSceneLoader를 찾지 못했다");
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }
    }
}
