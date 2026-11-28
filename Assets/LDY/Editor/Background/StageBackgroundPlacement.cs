using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.BackgroundEditor {
    public enum PlacementResult {
        Added,
        AlreadyPresent,
        NoPrefab
    }

    public static class StageBackgroundPlacement {
        public static GameObject PrefabForWorld(int world) {
            var source = BackgroundSources.All().FirstOrDefault(s =>
                BackgroundWorld.TryFromAssetName(s.name, out var w) && w == world);
            return source != null ? AssetDatabase.LoadAssetAtPath<GameObject>(source.PrefabPath) : null;
        }

        public static GameObject PrefabForStage(string stageId) =>
            BackgroundWorld.TryFromStageId(stageId, out var world) ? PrefabForWorld(world) : null;

        public static bool HasBackground(Scene scene) =>
            scene.GetRootGameObjects().Any(go => go.GetComponentInChildren<StageBackground>(true) != null);

        public static PlacementResult AddTo(Scene scene, GameObject prefab) {
            if (prefab == null) return PlacementResult.NoPrefab;
            if (HasBackground(scene)) return PlacementResult.AlreadyPresent;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Add Stage Background");
            BackgroundMapFitter.Fit(instance.GetComponent<StageBackground>());
            return PlacementResult.Added;
        }
    }
}
