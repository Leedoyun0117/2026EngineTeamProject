using System.Linq;
using LDY.PlayerLocate;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 다중 스테이지 씬 준비: 루트 켜기 → 겹치는 매니저 끄기 → 골 연결 → 시작 지점 → 카메라 범위.
    // sceneLoaded(Start 전)에 불려서 켜진 루트의 Start가 정상적으로 돈다.
    public static class SubStageLoader {
        // 카탈로그의 루트를 씬에서 찾지 못하면 false. 호출 쪽(SceneFlow)이 선택 화면으로 돌려보낸다.
        public static bool Prepare(Scene scene, StageEntry entry, StageCatalog catalog, bool activate,
            ConflictingManagers managers) {
            var known = catalog.SubStagesIn(entry.scenePath).Select(e => e.subRootName).ToList();
            var root = activate
                ? SubStageRoots.Apply(scene, entry.subRootName, known)
                : SubStageRoots.Find(scene, entry.subRootName);
            if (root == null) {
                Debug.LogError($"[Flow] {entry.id}: 카탈로그의 루트 '{entry.subRootName}'를 {scene.name}에서 찾지 못했다. " +
                               "팀원 씬의 루트 이름이 바뀌었는지 확인한다(LDY/Merge/Validate Open Stage)");
                return false;
            }

            if (managers != null)
                foreach (var disabled in managers.Disable(scene, entry.id))
                    Debug.Log($"[Flow] {entry.id}: {disabled} 런타임에서 끔");

            var goals = StageGoalAttacher.Attach(root);
            if (goals == 0) Debug.LogWarning($"[Flow] {entry.id}: {entry.subRootName} 안에 골이 없다");

            var spawn = PlayerSpawner.FindSpawn(root);
            var player = PrimaryPlayerLocator.Find();
            if (spawn == null)
                Debug.LogWarning($"[Flow] {entry.id}: {entry.subRootName} 안에 시작 지점이 없어 플레이어를 옮기지 않았다");
            else if (!PlayerSpawner.MoveTo(player, spawn))
                Debug.LogWarning($"[Flow] {entry.id}: 플레이어가 없어 시작 지점({spawn.name})으로 옮기지 못했다");

            Debug.Log($"[Flow] {entry.id}: {entry.subRootName} 준비, 골 {goals}개, 카메라 {StageCameraBinder.Bind(root)}");
            return true;
        }

        // 바로 실행: 켜져 있는 등록 루트가 있으면 그 스테이지로 본다. 아무것도 켜지 않는다.
        public static StageEntry DetectActive(Scene scene, StageCatalog catalog) {
            var entries = catalog.SubStagesIn(scene.path);
            if (entries.Count == 0) return null;
            var name = SubStageRoots.ActiveRootName(scene, entries.Select(e => e.subRootName).ToList());
            return name == null ? null : entries.Find(e => e.subRootName == name);
        }
    }
}
