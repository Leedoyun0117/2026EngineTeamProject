using System.IO;
using System.Linq;
using System.Text;

namespace LDY.StageImportEditor {
    internal static class SceneNaming {
        private static readonly char[] Invalid = Path.GetInvalidFileNameChars().Concat(new[] { '.' }).ToArray();

        public static string SceneName(string stageId, string mapName) {
            var safe = Sanitize(mapName);
            return string.IsNullOrEmpty(safe) ? $"Stage{stageId}" : $"Stage{stageId}_{safe}";
        }

        // 같은 이름 씬이 있을 때 새로 만들 이름: {기본}_v2, _v3 … 중 폴더에 없는 첫 번째.
        public static string NextFreeVersion(string folder, string baseName, out int version) {
            version = 2;
            while (File.Exists($"{folder}/{Versioned(baseName, version)}.unity")) version++;
            return Versioned(baseName, version);
        }

        public static string Versioned(string baseName, int version) =>
            version <= 1 ? baseName : $"{baseName}_v{version}";

        public static string Sanitize(string value) {
            if (string.IsNullOrEmpty(value)) return "";
            var sb = new StringBuilder(value.Length);
            foreach (var c in value.Trim()) {
                if (Invalid.Contains(c)) continue;
                sb.Append(char.IsWhiteSpace(c) ? '_' : c);
            }

            return sb.ToString().Trim('_');
        }
    }
}
