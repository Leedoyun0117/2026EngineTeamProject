using System.IO;
using System.Text;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerCompareReport {
        public static void Run() {
            var baseRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPaths.Jibin);
            var oursRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPaths.Shared);
            try {
                var rows = PropertyDiff.Compare(PlayerPrefabFlattener.Flatten(baseRoot),
                    PlayerPrefabFlattener.Flatten(oursRoot));
                var markdown = Build(rows);
                Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPaths.DiffReport));
                File.WriteAllText(PlayerPrefabPaths.DiffReport, markdown, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(PlayerPrefabPaths.DiffReport);
                Debug.Log($"[PlayerCompare] 차이 {rows.Count}개. {PlayerPrefabPaths.DiffReport}\n{markdown}");
            }
            finally {
                PrefabUtility.UnloadPrefabContents(oursRoot);
                PrefabUtility.UnloadPrefabContents(baseRoot);
            }
        }

        private static string Build(System.Collections.Generic.IReadOnlyList<DiffRow> rows) {
            var text = new StringBuilder();
            text.AppendLine("# Player 프리팹 비교");
            text.AppendLine();
            text.AppendLine($"- 지빈이: {PlayerPrefabPaths.Jibin}");
            text.AppendLine($"- 내 것: {PlayerPrefabPaths.Shared}");
            text.AppendLine($"- 차이 {rows.Count}개 (OnlyBase = 지빈이에만, OnlyOurs = 내 것에만)");
            text.AppendLine();
            text.Append(DiffMarkdown.Render(rows, "지빈이", "내 것"));
            return text.ToString();
        }
    }
}
