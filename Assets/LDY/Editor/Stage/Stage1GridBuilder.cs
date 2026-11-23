using System;
using LDY.Stage;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.StageEditor {
    internal sealed class Stage1GridBuilder {
        public const string GridName = "Stage1_Grid";
        private const string GroundLayer = "Ground";
        private const string HazardLayer = "Gimmick";
        private const string KillHitboxType = "HES.Script.Object.KillHitbox, Assembly-CSharp";

        public void Build(SetupReport report) {
            PrefabUpsert.Edit(StagePaths.Prefab(GridName), CreateRoot, Configure, out var created);
            report.Record(created, "Prefab " + GridName);
        }

        private static GameObject CreateRoot() {
            return new GameObject(GridName, typeof(Grid));
        }

        private void Configure(GameObject root) {
            root.GetComponent<Grid>().cellSize = Vector3.one;
            ConfigureGround(Layer(root, "Ground", 0));
            ConfigurePlatform(Layer(root, "Platform", 1));
            ConfigureHazard(Layer(root, "Hazard", 2));
            ConfigureInvisible(Layer(root, "InvisibleBlock", 3));
        }

        // 안 보이지만 막는 블록: 벽과 같은 Ground 레이어 + 합쳐진 콜라이더, 렌더러는 런타임에만 끈다.
        private static void ConfigureInvisible(GameObject go) {
            ConfigureGround(go);
            ObjectTools.EnsureComponent<HideTilemapRendererAtRuntime>(go);
        }

        private static GameObject Layer(GameObject root, string name, int sortingOrder) {
            var go = ObjectTools.EnsureChild(root.transform, name).gameObject;
            ObjectTools.EnsureComponent<Tilemap>(go);
            ObjectTools.EnsureComponent<TilemapRenderer>(go).sortingOrder = sortingOrder;
            return go;
        }

        private static void ConfigureGround(GameObject go) {
            ObjectTools.SetLayer(go, GroundLayer);
            var collider = ObjectTools.EnsureComponent<TilemapCollider2D>(go);
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var composite = ObjectTools.EnsureComponent<CompositeCollider2D>(go);
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        private static void ConfigurePlatform(GameObject go) {
            ObjectTools.SetLayer(go, GroundLayer);
            var collider = ObjectTools.EnsureComponent<TilemapCollider2D>(go);
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var composite = ObjectTools.EnsureComponent<CompositeCollider2D>(go);
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.usedByEffector = true;
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            var effector = ObjectTools.EnsureComponent<PlatformEffector2D>(go);
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 160f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
        }

        private static void ConfigureHazard(GameObject go) {
            ObjectTools.SetLayer(go, HazardLayer);
            ObjectTools.EnsureComponent<TilemapCollider2D>(go).isTrigger = true;
            var killHitbox = Type.GetType(KillHitboxType);
            if (killHitbox == null) {
                Debug.LogWarning("KillHitbox 타입을 찾지 못해 Hazard에 사망 판정을 붙이지 못했다");
                return;
            }

            if (go.GetComponent(killHitbox) == null) go.AddComponent(killHitbox);
        }
    }
}
