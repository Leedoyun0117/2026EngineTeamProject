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
