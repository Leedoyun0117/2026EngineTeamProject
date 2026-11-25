using System.Collections.Generic;
using LDY.Flow.Core;

namespace LDY.StageSelect.Core {
    // 선택 화면에 들어왔을 때 플레이어가 서는(= 처음 선택되는) 파일.
    // 방금 클리어한 파일 → 가장 앞의 미완료(열림) 파일 → 첫 파일. 비어 있으면 -1.
    public static class SpawnRule {
        public static int Pick(IReadOnlyList<string> ids, IReadOnlyList<StageStatus> statuses, string justClearedId) {
            if (ids.Count == 0) return -1;
            if (!string.IsNullOrEmpty(justClearedId))
                for (var i = 0; i < ids.Count; i++)
                    if (ids[i] == justClearedId)
                        return i;

            for (var i = 0; i < statuses.Count && i < ids.Count; i++)
                if (statuses[i] == StageStatus.Open)
                    return i;

            return 0;
        }
    }
}
