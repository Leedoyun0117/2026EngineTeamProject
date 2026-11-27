using System.Collections.Generic;
using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using LDY.PlayerMerge;
using LDY.TitlePlayer.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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
            else {
                checks.Add(CheckConflictingManagers(all));
            }

            checks.Add(CheckPlayer(all));
            checks.Add(CheckCamera(all));
            checks.Add(CheckMissingScripts(all));
            checks.Add(CheckMissingPrefabs(all));
            checks.Add(CheckBuildSettings(scene));
            checks.Add(CheckTitleInput());
            return checks;
        }

        // 런타임 ConflictingManagers와 같은 규칙(ManagerScan)으로, FlowSettings의 팀원 매니저 타입이 이 씬에 있는지 본다.
        private static StageCheck CheckConflictingManagers(List<GameObject> all) {
            const string label = "끌 팀원 매니저 타입 (FlowSettings)";
            var settings = AssetDatabase.LoadAssetAtPath<FlowSettings>(FlowPaths.Settings);
            if (settings == null) return new StageCheck(false, label, "FlowSettings 없음 " + FlowPaths.Settings);
            var targets = (settings.conflictingManagerTypes ?? new string[0]).Where(n => !string.IsNullOrEmpty(n)).ToList();
            if (targets.Count == 0) return new StageCheck(true, label, "설정이 비어 있어 끌 매니저 없음");

            var typeNames = all.SelectMany(go => go.GetComponents<MonoBehaviour>())
                .Where(b => b != null).Select(b => b.GetType().FullName);
            var scan = ManagerScan.Evaluate(typeNames, targets, settings.similarManagerHint);
            if (scan.FoundAny) return new StageCheck(true, label, string.Join(", ", scan.Matched));
            var similar = scan.Similar.Count > 0 ? " / 비슷한 컴포넌트: " + string.Join(", ", scan.Similar) : "";
            return new StageCheck(false, label, "씬에 없음: " + string.Join(", ", targets) + similar);
        }

        // 타이틀 플레이어(AltPlayer)의 입력 에셋에 LDY가 이름으로 찾는 맵·액션이 모두 있는지 본다.
        private static StageCheck CheckTitleInput() {
            const string label = "타이틀 입력 액션 (AltPlayer)";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FlowPaths.AltPlayer);
            var input = prefab != null ? prefab.GetComponent<PlayerInput>() : null;
            var actions = input != null ? input.actions : null;
            if (actions == null) return new StageCheck(false, label, FlowPaths.AltPlayer + "의 PlayerInput 입력 에셋 없음");

            var missing = InputActionNames.Missing(actions.Select(a => a.name), InputActionNames.Required);
            if (actions.FindActionMap(InputActionNames.Map, false) == null) missing.Insert(0, "맵 " + InputActionNames.Map);
            return missing.Count == 0
                ? new StageCheck(true, label, $"{actions.name}: {InputActionNames.Map}/{string.Join(", ", InputActionNames.Required)}")
                : new StageCheck(false, label, $"{actions.name}에 없음: {string.Join(", ", missing)}");
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
            const string label = "Player 1개 + 최종 플레이어 프리팹";
            var players = all.Where(go => go.CompareTag(PlayerMatch.DefaultTag)).ToList();
            if (players.Count != 1)
                return new StageCheck(false, label, $"Player 태그 {players.Count}개: {Names(players)}");
            var player = players[0];
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(player))
                return new StageCheck(false, label, player.name + "은 프리팹 인스턴스 루트가 아니다");
            var source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(player);
            return source == PlayerPrefabPaths.Final
                ? new StageCheck(true, label, source)
                : new StageCheck(false, label, $"{PlayerPrefabPaths.Final}가 아니다: {source} (LDY/Player/Merge/2. Swap Scenes로 교체)");
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
