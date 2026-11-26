using LDY.FlowEditor;
using LDY.Stage;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.StageImportEditor {
    internal enum AssetState {
        Auto,
        Temp,
        Missing
    }

    // 한 월드 대응표에서 키에 맞는 타일·프리팹을 고르고, 임시인지 알려 준다.
    internal sealed class MapAssets {
        private readonly StageTileMap _map;
        private readonly int _world;
        private readonly FlowLog _log;

        public MapAssets(StageTileMap map, int world, FlowLog log) {
            _map = map;
            _world = world;
            _log = log;
        }

        public bool TryTile(string key, out TileBase tile, out TileLayerKind layer) {
            tile = null;
            layer = TileLayerKind.Ground;
            if (!_map.TryGet(key, out var entry) || entry.kind != EntryKind.Tile || entry.tile == null) return false;
            tile = entry.tile;
            layer = entry.tileLayer;
            return true;
        }

        public GameObject Prefab(string key) {
            if (_map.TryGet(key, out var entry) && entry.kind == EntryKind.Prefab && entry.prefab != null)
                return entry.prefab;
            return ToolKeys.AbilityOf(key) != null ? TempPrefabFactory.Ensure(key, _log) : null;
        }

        public AssetState TileState(string key) {
            if (!TryTile(key, out var tile, out _)) return AssetState.Missing;
            if (_world == WorldMapStore.FirstWorld) return AssetState.Auto;
            return tile == WorldMapStore.FirstWorldTile(key, _log) ? AssetState.Temp : AssetState.Auto;
        }

        public AssetState PrefabState(string key) {
            if (_map.TryGet(key, out var entry) && entry.kind == EntryKind.Prefab && entry.prefab != null)
                return AssetDatabase.GetAssetPath(entry.prefab).StartsWith(StageImportPaths.TempPrefabs)
                    ? AssetState.Temp
                    : AssetState.Auto;
            return ToolKeys.AbilityOf(key) != null ? AssetState.Temp : AssetState.Missing;
        }
    }
}
