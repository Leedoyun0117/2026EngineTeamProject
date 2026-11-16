using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    internal static class PlayerVisualContext {
        public const string PrefabPath = "Assets/prefab/Player.prefab";
        public const string ScenePath = "Assets/LDY/Scenes/SampleScene.unity";
        public const string PinkName = "PinkVisual";
        public const string OldName = "Visual";

        public static void Log(string message) {
            Debug.Log("[PlayerVisual] " + message);
        }

        public static GameObject FindInstance(Scene scene, GameObject prefabAsset) {
            foreach (var root in scene.GetRootGameObjects())
                if (PrefabUtility.GetCorrespondingObjectFromSource(root) == prefabAsset)
                    return root;

            return null;
        }
    }

    // 이미 열려 있는 씬은 그대로 쓰고, 닫혀 있던 씬만 Additive로 열었다가 닫는다.
    internal sealed class SceneSession : IDisposable {
        private readonly bool _openedHere;

        private SceneSession(Scene scene, bool openedHere) {
            Scene = scene;
            _openedHere = openedHere;
        }

        public Scene Scene { get; }
        public bool WasDirty { get; private set; }

        public static SceneSession Open(string path) {
            var scene = EditorSceneManager.GetSceneByPath(path);
            if (scene.isLoaded)
                return new SceneSession(scene, false) { WasDirty = scene.isDirty };

            return new SceneSession(EditorSceneManager.OpenScene(path, OpenSceneMode.Additive), true);
        }

        // 사용자가 저장하지 않은 변경이 있는 씬은 우리가 대신 저장하지 않는다.
        public bool SaveIfSafe() {
            EditorSceneManager.MarkSceneDirty(Scene);
            if (WasDirty && !_openedHere)
                return false;

            return EditorSceneManager.SaveScene(Scene);
        }

        public void Dispose() {
            if (_openedHere)
                EditorSceneManager.CloseScene(Scene, true);
        }
    }
}
