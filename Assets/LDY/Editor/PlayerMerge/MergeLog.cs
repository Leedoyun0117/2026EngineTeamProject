using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    internal sealed class MergeLog {
        public List<string> Changed { get; } = new();
        public List<string> Notes { get; } = new();

        public void Print(string title, bool ok) {
            var text = $"{title} {(ok ? "완료" : "중단")}\n"
                       + Section("바꾼 항목", Changed, "변경 없음 (이미 최신)")
                       + Section("참고", Notes, null);
            if (ok)
                Debug.Log(text);
            else
                Debug.LogError(text);
        }

        private static string Section(string title, List<string> lines, string empty) {
            if (lines.Count == 0)
                return empty == null ? "" : $"[{title}] {empty}\n";

            return $"[{title}]\n- {string.Join("\n- ", lines)}\n";
        }
    }
}
