using System.Collections.Generic;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 칸마다·가로 런·4방향 묶음 프리팹을 "Imported Objects" 아래에 놓는다.
    internal sealed class PrefabStep : ISceneStep {
        private const string RootName = "Imported Objects";
        private const float HiddenAlpha = 0.5f;

        public void Apply(SceneContext context) {
            var root = context.EnsureRoot(RootName);
            var placed = new SortedDictionary<string, int>();
            var missing = new SortedDictionary<string, int>();

            foreach (var item in context.Plan.Prefabs) {
                var go = Spawn(context, item.Key, Parent(root, item.Key), missing);
                if (go == null) continue;
                PrefabAligner.Center(go, SceneContext.RectCenter(item.Rect));
                if (item.Rect.Width > 1) RunWidth.Apply(go, item.Rect.Width);
                Count(placed, item.Key);
            }

            var groupIndex = 0;
            foreach (var group in context.Plan.Groups) {
                var holder = new GameObject($"{group.Key}_group_{groupIndex++}").transform;
                holder.SetParent(Parent(root, group.Key), false);
                foreach (var cell in group.Cells) {
                    var go = Spawn(context, group.Key, holder, missing);
                    if (go == null) continue;
                    PrefabAligner.Center(go, SceneContext.CellCenter(cell));
                    if (group.Key == ToolKeys.GhostHidden) Fade(go);
                    Count(placed, group.Key);
                }
            }

            foreach (var pair in placed)
                context.Log.Info($"프리팹 '{pair.Key}' {pair.Value}개{StateSuffix(context, pair.Key)}");
            foreach (var pair in missing) context.Log.Warn($"프리팹 없음 '{pair.Key}' {pair.Value}칸 건너뜀");
        }

        private static GameObject Spawn(SceneContext context, string key, Transform parent,
            IDictionary<string, int> missing) {
            var prefab = context.Assets.Prefab(key);
            if (prefab == null) {
                Count(missing, key);
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, context.Scene);
            go.transform.SetParent(parent, true);
            return go;
        }

        private static Transform Parent(Transform root, string key) {
            var child = root.Find(key);
            if (child != null) return child;
            child = new GameObject(key).transform;
            child.SetParent(root, false);
            return child;
        }

        private static void Fade(GameObject go) {
            foreach (var renderer in go.GetComponentsInChildren<SpriteRenderer>()) {
                var color = renderer.color;
                color.a = HiddenAlpha;
                renderer.color = color;
            }
        }

        private static string StateSuffix(SceneContext context, string key) =>
            context.Assets.PrefabState(key) == AssetState.Temp ? " (임시 분홍 프리팹)" : "";

        private static void Count(IDictionary<string, int> counts, string key) {
            counts.TryGetValue(key, out var count);
            counts[key] = count + 1;
        }
    }

    // 가로 런 프리팹(이동 발판)을 칸 수만큼 넓힌다. 플레이어가 자식으로 붙는 발판이라 스케일 대신 크기를 바꾼다.
    internal static class RunWidth {
        public static void Apply(GameObject go, int width) {
            var renderer = go.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null) {
                var size = renderer.drawMode == SpriteDrawMode.Simple && renderer.sprite != null
                    ? (Vector2)renderer.sprite.bounds.size
                    : renderer.size;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(size.x * width, size.y);
            }

            foreach (var box in go.GetComponentsInChildren<BoxCollider2D>())
                box.size = new Vector2(box.size.x * width, box.size.y);
        }
    }
}
