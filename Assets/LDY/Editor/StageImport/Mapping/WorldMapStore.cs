using System.Collections.Generic;
using LDY.FlowEditor;
using LDY.Stage;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.StageImportEditor {
    // StageTileMap_World{n}을 읽는다. 없거나 빠진 키가 있으면 기본값으로 채우고, 있는 항목은 건드리지 않는다.
    internal static class WorldMapStore {
        public const int FirstWorld = 1;

        public static StageTileMap Ensure(int world, FlowLog log) {
            var path = StageImportPaths.WorldMap(world);
            var map = AssetDatabase.LoadAssetAtPath<StageTileMap>(path);
            if (map == null) {
                FlowPaths.EnsureFolder(StageImportPaths.Data);
                map = ScriptableObject.CreateInstance<StageTileMap>();
                AssetDatabase.CreateAsset(map, path);
                log.Info("대응표 만듦 " + path);
            }

            var entries = new List<StageTileMap.Entry>(map.Entries);
            var added = AddMissing(entries, world, log);
            if (added > 0) {
                map.SetEntries(entries);
                EditorUtility.SetDirty(map);
                AssetDatabase.SaveAssets();
                log.Info($"대응표 {path}에 키 {added}개 추가");
            }

            return map;
        }

        public static TileBase FirstWorldTile(string key, FlowLog log) =>
            Ensure(FirstWorld, log).TryGet(key, out var entry) ? entry.tile : null;

        private static int AddMissing(List<StageTileMap.Entry> entries, int world, FlowLog log) {
            var keys = new HashSet<string>();
            foreach (var entry in entries) keys.Add(entry.key);
            var before = entries.Count;

            foreach (var (key, layer) in WorldMapSeed.TileKeys)
                if (keys.Add(key))
                    entries.Add(TileEntry(key, SeedTile(key, world, log), layer));
            foreach (var deco in ToolKeys.Decorations)
                if (keys.Add(deco))
                    entries.Add(TileEntry(deco, null, TileLayerKind.Decoration));
            foreach (var seed in WorldMapSeed.Prefabs(world))
                if (keys.Add(seed.Key))
                    entries.Add(PrefabEntry(seed));

            var pickups = new PickupCatalogLookup();
            foreach (var ability in ToolKeys.Abilities) {
                var seed = WorldMapSeed.Pickup(ability, pickups.PrefabNameOf(ability));
                if (keys.Add(seed.Key)) entries.Add(PrefabEntry(seed));
            }

            return entries.Count - before;
        }

        private static TileBase SeedTile(string key, int world, FlowLog log) {
            if (world != FirstWorld) return FirstWorldTile(key, log);
            var legacy = AssetDatabase.LoadAssetAtPath<StageTileMap>(StageImportPaths.LegacyTileMap);
            return legacy != null && legacy.TryGet(key, out var entry) ? entry.tile : null;
        }

        private static StageTileMap.Entry TileEntry(string key, TileBase tile, TileLayerKind layer) =>
            new StageTileMap.Entry { key = key, kind = EntryKind.Tile, tile = tile, tileLayer = layer };

        private static StageTileMap.Entry PrefabEntry(SeedPrefab seed) =>
            new StageTileMap.Entry {
                key = seed.Key, kind = EntryKind.Prefab, prefab = FirstExisting(seed.Candidates), cluster = seed.Cluster,
                fixedSize = Vector2Int.one
            };

        private static GameObject FirstExisting(IReadOnlyList<string> candidates) {
            foreach (var path in candidates) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) return prefab;
            }

            return null;
        }
    }
}
