using LDY.Flow;
using LDY.FlowEditor;
using LDY.StageImport.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.StageImportEditor {
    internal interface ISceneStep {
        void Apply(SceneContext context);
    }

    internal sealed class SceneContext {
        public SceneContext(Scene scene, ImportJob job, MapAssets assets, FlowLog log) {
            Scene = scene;
            Job = job;
            Assets = assets;
            Log = log;
        }

        public Scene Scene { get; }
        public ImportJob Job { get; }
        public ImportPlan Plan => Job.Plan;
        public MapAssets Assets { get; }
        public FlowLog Log { get; }

        public GameObject FindRoot(string name) {
            foreach (var root in Scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }

        public Transform EnsureRoot(string name) {
            var existing = FindRoot(name);
            if (existing != null) return existing.transform;
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, Scene);
            return go.transform;
        }

        public GameObject FindPlayer() {
            foreach (var root in Scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.CompareTag(PlayerMatch.DefaultTag))
                    return t.gameObject;
            return null;
        }

        public static Vector3 CellCenter(GridPoint p) => new Vector3(p.X + 0.5f, p.Y + 0.5f, 0f);
        public static Vector3 RectCenter(GridRect r) => new Vector3(r.CenterX, r.CenterY, 0f);
    }
}
