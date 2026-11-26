using System.Collections.Generic;
using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    // StagePlan으로 카탈로그 에셋을 갱신한다. 손으로 연결한 씬·썸네일·상태는 그대로 둔다.
    internal static class StageCatalogBuilder {
        private const string JibinLevel = "Assets/JJB/JJBLevel.unity";

        // 조사로 확인된 연결. 루트 이름은 JJBLevel 안의 실제 이름(Level1 아래 형제)이다.
        private static readonly (string id, string scene, string subRoot)[] ConfirmedLinks = {
            ("1-T", JibinLevel, "Stage1-Tutorial"),
            ("1-1", JibinLevel, "Stage1-1"),
            ("1-2", JibinLevel, "Stage1-2"),
            ("1-3", JibinLevel, "Stage1-3"),
            ("1-4", JibinLevel, "Stage1-4"),
            ("1-5", JibinLevel, "Stage1-5"),
            ("1-6", JibinLevel, "Stage1-6")
        };

        public static StageCatalog Build(FlowLog log) {
            FlowPaths.EnsureFolder(FlowPaths.Data);
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            var created = catalog == null;
            if (created) {
                catalog = ScriptableObject.CreateInstance<StageCatalog>();
                AssetDatabase.CreateAsset(catalog, FlowPaths.Catalog);
            }

            SyncWorlds(catalog.EditableWorlds);
            SyncEntries(catalog.EditableEntries, log);
            ApplyConfirmedLinks(catalog, log);
            EditorUtility.SetDirty(catalog);
            log.Info((created ? "만듦 " : "갱신 ") + FlowPaths.Catalog);
            return catalog;
        }

        private static void SyncWorlds(List<WorldEntry> worlds) {
            worlds.Clear();
            foreach (var plan in StagePlan.Worlds)
                worlds.Add(new WorldEntry { world = plan.World, displayName = plan.DisplayName, folderName = plan.FolderName });
        }

        private static void SyncEntries(List<StageEntry> entries, FlowLog log) {
            var old = entries.GroupBy(e => e.id).ToDictionary(g => g.Key, g => g.First());
            foreach (var stray in old.Keys.Except(StagePlan.Entries.Select(p => p.Id)))
                log.Warn($"기획에 없는 항목 {stray} 제거 (씬 {old[stray].scenePath})");

            entries.Clear();
            foreach (var plan in StagePlan.Entries) {
                var entry = old.TryGetValue(plan.Id, out var kept) ? kept : new StageEntry();
                entry.id = plan.Id;
                entry.world = plan.World;
                entry.number = plan.Number;
                entry.fileName = plan.FileName;
                entry.abilities = plan.Abilities.ToList();
                if (string.IsNullOrEmpty(entry.scenePath)) entry.state = StageReadiness.NotReady;
                entries.Add(entry);
            }
        }

        // 비어 있거나 같은 씬에 연결된 항목만 채운다. 다른 씬이 손으로 연결돼 있으면 그대로 둔다.
        private static void ApplyConfirmedLinks(StageCatalog catalog, FlowLog log) {
            foreach (var (id, scenePath, subRoot) in ConfirmedLinks) {
                var entry = catalog.Find(id);
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (entry == null || scene == null) {
                    log.Warn($"{id}: 연결 대상 없음 ({scenePath})");
                    continue;
                }

                if (!string.IsNullOrEmpty(entry.scenePath) && entry.scenePath != scenePath) {
                    log.Warn($"{id}: 이미 {entry.scenePath}에 연결돼 있어 {subRoot} 연결을 건너뜀");
                    continue;
                }

                entry.sceneAsset = scene;
                entry.scenePath = scenePath;
                entry.subRootName = subRoot;
                entry.state = StageReadiness.Ready;
                log.Info($"{id} -> {scenePath} [{subRoot}] Ready");
            }
        }

        public static bool LinkScene(StageCatalog catalog, string id, SceneAsset scene, FlowLog log) {
            var entry = catalog.Find(id);
            if (entry == null) {
                log.Warn($"카탈로그에 {id}가 없어 씬을 연결하지 않았다");
                return false;
            }

            var path = AssetDatabase.GetAssetPath(scene);
            if (!string.IsNullOrEmpty(entry.scenePath) && entry.scenePath != path) {
                log.Warn($"{id}에 이미 {entry.scenePath}가 연결돼 있어 바꾸지 않았다");
                return false;
            }

            entry.sceneAsset = scene;
            entry.scenePath = path;
            entry.subRootName = "";
            entry.state = StageReadiness.Ready;
            EditorUtility.SetDirty(catalog);
            log.Info($"카탈로그 {id} -> {path} (Ready)");
            return true;
        }
    }
}
