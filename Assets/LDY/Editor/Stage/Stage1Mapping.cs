using System.Collections.Generic;
using LDY.Stage;
using UnityEditor;
using UnityEngine;

namespace LDY.StageEditor {
    internal sealed class StageTileMapBuilder {
        private const string AssetPath = StagePaths.Data + "/StageTileMap.asset";

        public void Build(Stage1TileSet tiles, SetupReport report) {
            var map = AssetUpsert.LoadOrCreate<StageTileMap>(AssetPath, out var created);
            var entries = new List<StageTileMap.Entry> {
                TileEntry("ground", tiles.Ground, TileLayerKind.Ground),
                TileEntry("platform", tiles.Platform, TileLayerKind.Platform),
                TileEntry("spike", tiles.Spikes[(int)SpikeDirection.Up], TileLayerKind.Hazard),
                PrefabEntry("start", "Start", ClusterMode.PerCell),
                PrefabEntry("goal", "GoalMusicFile", ClusterMode.PerCell),
                PrefabEntry("box", "BoxCdCase", ClusterMode.PerCell),
                PrefabEntry("mov_h", "MovingPlatformH", ClusterMode.StretchHorizontal),
                PrefabEntry("mov_v", "MovingPlatformV", ClusterMode.StretchHorizontal),
                PrefabEntry("ghost", "GhostBlock", ClusterMode.PerCell),
                PrefabEntry("ghost_on", "GhostBlock", ClusterMode.PerCell),
                PrefabEntry("cut", "CutBlock", ClusterMode.PerCell)
            };
            map.SetEntries(entries);
            EditorUtility.SetDirty(map);
            report.Record(created, "StageTileMap " + AssetPath);
            foreach (var entry in entries)
                if (entry.kind == EntryKind.Prefab && entry.prefab == null)
                    report.Warn($"StageTileMap '{entry.key}': 프리팹이 없어 비워 둠");
        }

        private static StageTileMap.Entry TileEntry(string key, ScriptableObject tile, TileLayerKind layer) {
            return new StageTileMap.Entry {
                key = key,
                kind = EntryKind.Tile,
                tile = (UnityEngine.Tilemaps.TileBase)tile,
                tileLayer = layer
            };
        }

        private static StageTileMap.Entry PrefabEntry(string key, string placeholderName, ClusterMode cluster) {
            return new StageTileMap.Entry {
                key = key,
                kind = EntryKind.Prefab,
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StagePaths.Prefab("Stage1_" + placeholderName)),
                cluster = cluster,
                fixedSize = Vector2Int.one
            };
        }
    }
}
