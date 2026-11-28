using LDY.Background.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundSceneWindow : EditorWindow {
        private const string Title = "Add Stage Background";
        private const string OwnSceneRoot = "Assets/LDY/";

        public static void Open() => GetWindow<BackgroundSceneWindow>(true, Title);

        private void OnGUI() {
            var scene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("열린 씬", string.IsNullOrEmpty(scene.path) ? "(저장 안 됨)" : scene.path);
            var foreign = !scene.path.StartsWith(OwnSceneRoot);
            if (foreign) EditorGUILayout.HelpBox(OwnSceneRoot + " 밖의 씬이다. 씬 주인만 실행한다.", MessageType.Warning);

            if (StageBackgroundPlacement.HasBackground(scene)) {
                EditorGUILayout.HelpBox("이미 배경이 있어 건너뛴다.", MessageType.Info);
                return;
            }

            foreach (var source in BackgroundSources.All()) {
                if (!BackgroundWorld.TryFromAssetName(source.name, out var world)) continue;
                var prefab = StageBackgroundPlacement.PrefabForWorld(world);
                using (new EditorGUI.DisabledScope(prefab == null))
                    if (GUILayout.Button($"World {world}: {source.name}" + (prefab == null ? " (프리팹 없음)" : "")))
                        Add(scene, prefab, foreign);
            }
        }

        private static void Add(Scene scene, GameObject prefab, bool foreign) {
            if (foreign && !EditorUtility.DisplayDialog(Title,
                    $"{scene.path}은(는) {OwnSceneRoot} 밖의 씬이다. 이 씬의 주인이 맞나?", "추가", "취소"))
                return;
            if (StageBackgroundPlacement.AddTo(scene, prefab) != PlacementResult.Added) return;
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"{Title}: {prefab.name} -> {scene.path} (저장은 Ctrl+S)");
        }
    }
}
