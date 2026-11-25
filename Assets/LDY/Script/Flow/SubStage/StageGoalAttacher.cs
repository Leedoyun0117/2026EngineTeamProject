using LDY.Flow.Core;
using UnityEngine;

namespace LDY.Flow {
    // 루트 안의 Stage1_GoalMusicFile 인스턴스에 StageGoal을 런타임으로 붙인다. 씬·프리팹 원본은 바뀌지 않는다.
    public static class StageGoalAttacher {
        public static int Attach(GameObject root) {
            var count = 0;
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
                if (!StageObjectNames.IsGoal(t.name)) continue;
                if (t.GetComponent<Collider2D>() == null) {
                    Debug.LogWarning($"[Flow] {t.name}에 Collider2D가 없어 골로 쓰지 못한다", t);
                    continue;
                }

                if (t.GetComponent<StageGoal>() == null) t.gameObject.AddComponent<StageGoal>();
                count++;
            }

            return count;
        }
    }
}
