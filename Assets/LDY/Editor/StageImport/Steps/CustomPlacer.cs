using System.Collections.Generic;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // custom 대응표 블록을 놓는다. 프리팹이 있으면 묶는 방식·크기 방식대로, 장식 그림만 있으면 충돌 없는 그림, 둘 다 없으면 분홍 임시 블록.
    internal sealed class CustomPlacer {
        private readonly SceneContext _context;

        public CustomPlacer(SceneContext context) {
            _context = context;
        }

        public SortedDictionary<string, int> Placed { get; } = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> Temp { get; } = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> Missing { get; } = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> Skipped { get; } = new SortedDictionary<string, int>();

        // 이 이름의 칸은 놓지 않는다(L/R 정렬처럼 다른 규칙이 맡을 때).
        public HashSet<string> Skip { get; } = new HashSet<string>();

        // 항목 이름별로 놓은 프리팹 인스턴스(묶음당 하나일 때 뒤처리용).
        public Dictionary<string, List<GameObject>> Spawned { get; } = new Dictionary<string, List<GameObject>>();

        // 칸마다·가로 런. rect가 한 칸이면 칸마다와 같다.
        public void PlaceRect(string key, GridRect rect, Transform parent) {
            if (!_context.Customs.TryEntry(key, out var entry, out var col)) {
                Count(Missing, key, rect.Width * rect.Height);
                return;
            }

            if (Skip.Contains(entry.name)) {
                Count(Skipped, entry.name, rect.Width * rect.Height);
                return;
            }

            if (entry.prefab == null) {
                if (entry.decoration != null) Decoration(entry, rect, parent);
                else {
                    PlaceholderBlock.Create(entry.name, col, ToRect(rect), parent, true);
                    Count(Temp, entry.name, rect.Width * rect.Height);
                }

                return;
            }

            if (rect.Width * rect.Height > 1 && entry.sizing == CustomSizing.RepeatPerCell) {
                var holder = Holder(parent, entry.name);
                for (var x = rect.X; x < rect.X + rect.Width; x++)
                for (var y = rect.Y; y < rect.Y + rect.Height; y++)
                    Spawn(entry, GridRect.Cell(new GridPoint(x, y)), holder, false);
                return;
            }

            if (IsOnePerGroup(entry.sizing)) {
                PlaceOne(entry, Cells(rect), parent);
                return;
            }

            Spawn(entry, rect, parent, entry.sizing == CustomSizing.Stretch);
        }

        // 4방향 묶음. 늘리기는 묶음을 덮는 사각형마다 하나, 하나 계열은 GroupSpot이 고른 자리에 원래 크기로 하나.
        public void PlaceGroup(string key, IReadOnlyList<GridPoint> cells, Transform parent) {
            if (!_context.Customs.TryEntry(key, out var entry, out var col)) {
                Count(Missing, key, cells.Count);
                return;
            }

            if (Skip.Contains(entry.name)) {
                Count(Skipped, entry.name, cells.Count);
                return;
            }

            var holder = Holder(parent, entry.name);
            if (entry.prefab == null) {
                if (entry.decoration != null) {
                    foreach (var rect in RectCover.Cover(cells)) Decoration(entry, rect, holder);
                    return;
                }

                // 이름표는 묶음마다 한 번(첫 조각)만 붙인다.
                var label = true;
                foreach (var rect in Pieces(entry.sizing, cells)) {
                    PlaceholderBlock.Create(entry.name, col, ToRect(rect), holder, label);
                    label = false;
                }

                Count(Temp, entry.name, cells.Count);
                return;
            }

            if (IsOnePerGroup(entry.sizing)) {
                PlaceOne(entry, cells, holder);
                return;
            }

            switch (entry.sizing) {
                case CustomSizing.Stretch:
                    foreach (var rect in RectCover.Cover(cells)) Spawn(entry, rect, holder, true);
                    break;
                default:
                    foreach (var cell in cells) Spawn(entry, GridRect.Cell(cell), holder, false);
                    break;
            }
        }

        private static bool IsOnePerGroup(CustomSizing sizing) =>
            sizing == CustomSizing.OnePerGroup || sizing == CustomSizing.OneAtLeft || sizing == CustomSizing.HangFromTop;

        private void PlaceOne(CustomBlockMap.Entry entry, IReadOnlyList<GridPoint> cells, Transform parent) {
            var spot = GroupSpot.Pick(entry.sizing, cells);
            if (entry.sizing != CustomSizing.HangFromTop) {
                Spawn(entry, spot.Rect, parent, false);
                return;
            }

            // 밧줄: 고정점(맨 위 칸 윗변 = 천장 면 가운데)에 피벗을 두고 렌더러 범위로 옮기지 않는다. 길이는 그 열의 칸 수.
            var go = Instantiate(entry, parent);
            go.transform.position = new Vector3(spot.AnchorX, spot.AnchorY, 0f);
            RopeLength.Apply(go, spot.Length, _context.Log);
        }

        private GameObject Spawn(CustomBlockMap.Entry entry, GridRect rect, Transform parent, bool stretch) {
            var go = Instantiate(entry, parent);
            go.transform.position = SceneContext.RectCenter(rect);
            if (stretch && (rect.Width > 1 || rect.Height > 1)) StretchToRect.Apply(go, rect.Width, rect.Height);
            PrefabAligner.Center(go, SceneContext.RectCenter(rect));
            return go;
        }

        private GameObject Instantiate(CustomBlockMap.Entry entry, Transform parent) {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(entry.prefab, _context.Scene);
            go.transform.SetParent(parent, true);
            if (!Spawned.TryGetValue(entry.name, out var list)) Spawned[entry.name] = list = new List<GameObject>();
            list.Add(go);
            Count(Placed, entry.name, 1);
            return go;
        }

        // 충돌 없는 장식 그림. 사각형 크기로 늘린다(가장자리 4점 스프라이트면 Tiled, 아니면 스케일).
        private void Decoration(CustomBlockMap.Entry entry, GridRect rect, Transform parent) {
            var go = new GameObject(entry.name + "_Deco");
            go.transform.SetParent(parent, false);
            go.transform.position = SceneContext.RectCenter(rect);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = entry.decoration;
            renderer.sortingOrder = 3;
            var size = entry.decoration.bounds.size;
            if (entry.decoration.vertices.Length == 4) {
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(rect.Width, rect.Height);
            }
            else if (size.x > 0f && size.y > 0f) {
                go.transform.localScale = new Vector3(rect.Width / size.x, rect.Height / size.y, 1f);
            }

            Count(Placed, entry.name + " (장식)", 1);
        }

        private static List<GridPoint> Cells(GridRect rect) {
            var cells = new List<GridPoint>();
            for (var x = rect.X; x < rect.X + rect.Width; x++)
            for (var y = rect.Y; y < rect.Y + rect.Height; y++)
                cells.Add(new GridPoint(x, y));
            return cells;
        }

        private static IEnumerable<GridRect> Pieces(CustomSizing sizing, IReadOnlyList<GridPoint> cells) {
            if (sizing != CustomSizing.RepeatPerCell) return RectCover.Cover(cells);
            var list = new List<GridRect>();
            foreach (var cell in cells) list.Add(GridRect.Cell(cell));
            return list;
        }

        private static Transform Holder(Transform parent, string name) {
            var go = new GameObject(name + "_group");
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Rect ToRect(GridRect r) => new Rect(r.X, r.Y, r.Width, r.Height);

        private static void Count(IDictionary<string, int> counts, string key, int amount) {
            counts.TryGetValue(key, out var count);
            counts[key] = count + amount;
        }
    }

    // 묶음 크기로 늘린다. 가장 위 SpriteRenderer를 Tiled로 바꿔 크기를 맞추고, 같은 오브젝트의 BoxCollider2D도 같은 비율로 키운다.
    // 자식 오브젝트(버튼 등)는 건드리지 않는다.
    internal static class StretchToRect {
        public static void Apply(GameObject go, int width, int height) {
            var renderer = go.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null) return;
            var world = renderer.bounds.size;
            if (world.x <= 0f || world.y <= 0f) return;

            var factor = new Vector2(width / world.x, height / world.y);
            var size = renderer.drawMode == SpriteDrawMode.Simple ? (Vector2)renderer.sprite.bounds.size : renderer.size;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = Vector2.Scale(size, factor);
            foreach (var box in renderer.GetComponents<BoxCollider2D>()) {
                box.size = Vector2.Scale(box.size, factor);
                box.offset = Vector2.Scale(box.offset, factor);
            }
        }
    }
}
