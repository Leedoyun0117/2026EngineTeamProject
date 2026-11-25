using System.Collections.Generic;
using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.FlowEditor {
    internal readonly struct StageCheck {
        public StageCheck(bool passed, string label, string detail) {
            Passed = passed;
            Label = label;
            Detail = detail;
        }

        public bool Passed { get; }
        public string Label { get; }
        public string Detail { get; }
    }

    internal readonly struct SubStageRow {
        public SubStageRow(string id, string root, bool found, int goals, string spawn, string bounds) {
            Id = id;
            Root = root;
            Found = found;
            Goals = goals;
            Spawn = spawn;
            Bounds = bounds;
        }

        public string Id { get; }
        public string Root { get; }
        public bool Found { get; }
        public int Goals { get; }
        public string Spawn { get; }
        public string Bounds { get; }
        public bool Passed => Found && Goals >= 1 && Spawn != null;
    }

    // 씬을 읽기만 한다. 고치거나 저장하지 않는다.
    internal static class StageSceneValidator {
        private static readonly string[] BoundsNames = { "CameraBounds", "Bounds" };

        public static bool IsMultiStage(Scene scene, StageCatalog catalog) =>
            catalog != null && catalog.SubStagesIn(scene.path).Count > 0;

        public static List<StageCheck> Run(Scene scene, StageCatalog catalog) {
            var all = AllObjects(scene);
            var checks = new List<StageCheck>();
            if (!IsMultiStage(scene, catalog)) {
                checks.Add(CheckRoot(all, catalog));
                checks.Add(CheckGoal(all));
            }

            checks.Add(CheckPlayer(all));
            checks.Add(CheckCamera(all));
            checks.Add(CheckMissingScripts(all));
            checks.Add(CheckMissingPrefabs(all));
            checks.Add(CheckBuildSettings(scene));
            return checks;
        }

        // 다중 스테이지 모드: 카탈로그에 등록된 루트마다 골·시작 지점·카메라 범위를 본다. 런타임과 같은 규칙을 쓴다.
        public static List<SubStageRow> RunSubStages(Scene scene, StageCatalog catalog) {
            var rows = new List<SubStageRow>();
            foreach (var entry in catalog.SubStagesIn(scene.path)) {
                var root = SubStageRoots.Find(scene, entry.subRootName);
                if (root == null) {
                    rows.Add(new SubStageRow(entry.id, entry.subRootName, false, 0, null, "-"));
                    continue;
                }

                var transforms = root.GetComponentsInChildren<Transform>(true);
                var goals = transforms.Count(t => t.GetComponent<StageGoal>() != null || StageObjectNames.IsGoal(t.name));
                var spawn = PlayerSpawner.FindSpawn(root);
                var bounds = transforms.FirstOrDefault(t => BoundsNames.Contains(t.name) && t.GetComponent<Collider2D>() != null);
                rows.Add(new SubStageRow(entry.id, entry.subRootName, true, goals, spawn != null ? spawn.name : null,
                    bounds != null ? bounds.name : "타일맵 범위로 계산"));
            }

            return rows;
        }

        private static List<GameObject> AllObjects(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject).ToList();

        private static StageCheck CheckRoot(List<GameObject> all, StageCatalog catalog) {
            const string label = "StageRoot + 카탈로그 id";
            var roots = all.Select(go => go.GetComponent<StageRoot>()).Where(r => r != null).ToList();
            if (roots.Count != 1) return new StageCheck(false, label, $"StageRoot {roots.Count}개 (1개여야 함)");
            var id = roots[0].StageId;
            if (string.IsNullOrEmpty(id)) return new StageCheck(false, label, "id가 비었다");
            if (catalog == null) return new StageCheck(false, label, "카탈로그 에셋 없음");
            return catalog.Find(id) != null
                ? new StageCheck(true, label, id)
                : new StageCheck(false, label, $"'{id}'가 카탈로그에 없다");
        }

        private static StageCheck CheckPlayer(List<GameObject> all) {
            const string label = "Player 1개 + 프리팹 인스턴스";
            var players = all.Where(go => go.CompareTag(PlayerMatch.DefaultTag)).ToList();
            if (players.Count != 1)
                return new StageCheck(false, label, $"Player 태그 {players.Count}개: {Names(players)}");
            var player = players[0];
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(player))
                return new StageCheck(false, label, player.name + "은 프리팹 인스턴스 루트가 아니다");
            var source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(player);
            return FlowPaths.KnownPlayers.Contains(source)
                ? new StageCheck(true, label, source)
                : new StageCheck(false, label, "알려진 Player 프리팹이 아니다: " + source);
        }

        private static StageCheck CheckGoal(List<GameObject> all) {
            var count = all.Count(go => go.GetComponent<StageGoal>() != null);
            return new StageCheck(count >= 1, "StageGoal 1개 이상", count + "개");
        }

        private static StageCheck CheckCamera(List<GameObject> all) {
            var cameras = all.Where(go => go.GetComponent<Camera>() != null).ToList();
            return new StageCheck(cameras.Count == 1, "Camera 1개", $"{cameras.Count}개 {Names(cameras)}");
        }

        private static StageCheck CheckMissingScripts(List<GameObject> all) {
            var broken = all.Where(go => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0).ToList();
            return new StageCheck(broken.Count == 0, "Missing Script 없음", broken.Count == 0 ? "" : Names(broken));
        }

        private static StageCheck CheckMissingPrefabs(List<GameObject> all) {
            var broken = all.Where(go => PrefabUtility.IsPrefabAssetMissing(go)).ToList();
            return new StageCheck(broken.Count == 0, "Missing Prefab 없음", broken.Count == 0 ? "" : Names(broken));
        }

        private static StageCheck CheckBuildSettings(Scene scene) {
            var ok = !string.IsNullOrEmpty(scene.path) && BuildSettingsSync.Contains(scene.path);
            return new StageCheck(ok, "Build Settings 포함", string.IsNullOrEmpty(scene.path) ? "저장 안 된 씬" : scene.path);
        }

        private static string Names(IEnumerable<GameObject> objects) =>
            string.Join(", ", objects.Take(8).Select(go => go.name));
    }
}
