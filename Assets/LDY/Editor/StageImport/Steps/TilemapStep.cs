using System.Collections.Generic;
using LDY.Stage;
using LDY.StageImport.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.StageImportEditor {
    // 템플릿 Grid의 Ground/Platform/Hazard에 칸마다 타일을 놓는다. 장식·투명 레이어는 필요할 때만 만든다.
    internal sealed class TilemapStep : ISceneStep {
        private const string GridName = "Grid";

        public void Apply(SceneContext context) {
            var grid = context.FindRoot(GridName);
            if (grid == null) {
                context.Log.Warn("템플릿 Grid가 없어 타일을 놓지 못했다");
                return;
            }

            var layers = new TilemapLayers(grid.transform);
            var missing = new SortedDictionary<string, int>();
            var temp = new SortedSet<string>();
            Place(context, layers, context.Plan.Tiles, missing, temp);
            var decoMissing = new SortedDictionary<string, int>();
            Place(context, layers, context.Plan.Decorations, decoMissing, temp);

            foreach (var key in temp) context.Log.Warn($"임시 타일: '{key}'에 1스테이지 타일을 썼다 (월드 {context.Job.Assignment.World})");
            foreach (var pair in missing) context.Log.Warn($"타일 없음 '{pair.Key}' {pair.Value}칸 건너뜀");
            foreach (var pair in decoMissing) context.Log.Info($"장식 타일 없음 '{pair.Key}' {pair.Value}칸 건너뜀");
            context.Log.Info($"타일 {context.Plan.Tiles.Count}칸, 장식 {context.Plan.Decorations.Count}칸 처리");
        }

        private static void Place(SceneContext context, TilemapLayers layers, IEnumerable<KeyedCell> cells,
            IDictionary<string, int> missing, ISet<string> temp) {
            foreach (var cell in cells) {
                if (!context.Assets.TryTile(cell.Key, out var tile, out var layer)) {
                    missing.TryGetValue(cell.Key, out var count);
                    missing[cell.Key] = count + 1;
                    continue;
                }

                if (context.Assets.TileState(cell.Key) == AssetState.Temp) temp.Add(cell.Key);
                layers.Get(layer).SetTile(new Vector3Int(cell.Cell.X, cell.Cell.Y, 0), tile);
            }
        }
    }

    internal sealed class TilemapLayers {
        private readonly Transform _grid;
        private readonly Dictionary<TileLayerKind, Tilemap> _cache = new Dictionary<TileLayerKind, Tilemap>();

        public TilemapLayers(Transform grid) {
            _grid = grid;
        }

        public Tilemap Get(TileLayerKind kind) {
            if (_cache.TryGetValue(kind, out var tilemap)) return tilemap;
            tilemap = Find(NameOf(kind)) ?? Create(kind);
            _cache[kind] = tilemap;
            return tilemap;
        }

        private static string NameOf(TileLayerKind kind) {
            switch (kind) {
                case TileLayerKind.Platform: return "Platform";
                case TileLayerKind.Hazard: return "Hazard";
                case TileLayerKind.Invisible: return "InvisibleBlock";
                case TileLayerKind.Decoration: return "Decoration";
                default: return "Ground";
            }
        }

        private Tilemap Find(string name) {
            var child = _grid.Find(name);
            return child != null ? child.GetComponent<Tilemap>() : null;
        }

        private Tilemap Create(TileLayerKind kind) {
            var go = new GameObject(NameOf(kind), typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(_grid, false);
            var renderer = go.GetComponent<TilemapRenderer>();
            if (kind == TileLayerKind.Decoration) {
                renderer.sortingOrder = -1;
                return go.GetComponent<Tilemap>();
            }

            var layer = LayerMask.NameToLayer("Ground");
            if (layer >= 0) go.layer = layer;
            go.AddComponent<TilemapCollider2D>();
            if (kind == TileLayerKind.Invisible) go.AddComponent<HideTilemapRendererAtRuntime>();
            renderer.sortingOrder = 3;
            return go.GetComponent<Tilemap>();
        }
    }
}
