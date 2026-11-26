using System.Linq;
using LDY.Background;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundSceneWindow : EditorWindow {
        private const string AllowedSceneRoot = "Assets/LDY/";

        public static void Open() => GetWindow<BackgroundSceneWindow>(true, "Add Stage Background");

        private void OnGUI() {
            var scene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("열린 씬", string.IsNullOrEmpty(scene.path) ? "(저장 안 됨)" : scene.path);
            if (!scene.path.StartsWith(AllowedSceneRoot)) {
                EditorGUILayout.HelpBox(AllowedSceneRoot + " 아래 씬에만 추가한다.", MessageType.Warning);
                return;
            }

            foreach (var source in BackgroundSources.All())
                if (GUILayout.Button(source.name))
                    Add(scene, source);
        }

        private static void Add(Scene scene, StageBackgroundSource source) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(source.PrefabPath);
            if (prefab == null) {
                EditorUtility.DisplayDialog("Add Stage Background", "프리팹이 없다. Build Stage Backgrounds를 먼저 실행: " + source.PrefabPath, "OK");
                return;
            }

            if (scene.GetRootGameObjects().Any(go => PrefabUtility.GetCorrespondingObjectFromSource(go) == prefab)) {
                EditorUtility.DisplayDialog("Add Stage Background", "이미 씬에 있다: " + prefab.name, "OK");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Add Stage Background");
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = instance;
        }
    }
}
