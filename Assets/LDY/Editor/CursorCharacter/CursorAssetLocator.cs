using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace LDY.Script {
    // 파일 이름으로 에셋 경로를 찾는다. 못 찾거나 둘 이상이면 경로 목록과 함께 실패한다.
    internal static class CursorAssetLocator {
        public const string PinkV2File = "player_anim_pinkV2.aseprite";
        public const string CursorFile = "player_anim_cursor.aseprite";

        public static bool TryFind(string fileName, out string path, List<string> report) {
            var term = Path.GetFileNameWithoutExtension(fileName);
            var found = AssetDatabase.FindAssets(term)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => Path.GetFileName(p) == fileName)
                .Distinct()
                .ToList();

            path = found.Count == 1 ? found[0] : null;
            if (found.Count == 1)
                return true;

            report.Add(found.Count == 0
                ? $"중단: {fileName} 을 프로젝트에서 찾지 못함"
                : $"중단: {fileName} 이 {found.Count}개 있음: {string.Join(", ", found)}");
            return false;
        }
    }
}
