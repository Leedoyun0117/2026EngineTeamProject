using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerMergeRunner {
        public static void Run() {
            var steps = new List<string>();
            if (!CursorPpuStep.Run(steps) || !BuildAnimSet(steps)) {
                Print(false, steps, new MergeLog());
                return;
            }

            var log = new MergeLog();
            var ok = PlayerPrefabMerger.Run(log);
            AssetDatabase.SaveAssets();
            Print(ok, steps, log);
        }

        private static bool BuildAnimSet(List<string> steps) {
            var built = PlayerAnimSetBuilder.BuildCursor();
            steps.Add(built
                ? $"{PlayerAnimSetBuilder.CursorSetPath} 재빌드"
                : "PlayerAnimSet_Cursor 빌드 실패. Console의 에러를 확인하세요");
            return built;
        }

        private static void Print(bool ok, List<string> steps, MergeLog log) {
            var text = $"[PlayerMerge] {(ok ? "완료" : "중단")}\n"
                       + Section("선행 단계", steps)
                       + Section("바꾼 항목", log.Changed, log.HasChanges ? null : "변경 없음 (이미 합쳐져 있음)")
                       + Section("고친 깨진 값", log.Fixed)
                       + Section("참고", log.Notes);
            if (ok)
                Debug.Log(text);
            else
                Debug.LogError(text);
        }

        private static string Section(string title, List<string> lines, string empty = null) {
            if (lines.Count == 0)
                return empty == null ? "" : $"[{title}] {empty}\n";

            return $"[{title}]\n- {string.Join("\n- ", lines)}\n";
        }
    }
}
