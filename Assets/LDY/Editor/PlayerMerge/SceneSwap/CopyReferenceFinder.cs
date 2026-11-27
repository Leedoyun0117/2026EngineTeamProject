using System.IO;
using System.Linq;
using System.Text;
using LDY.PlayerMerge;
using UnityEditor;

namespace LDY.Script {
    // B'(PlayerCopy 원본)를 가리키는 에셋과 코드 문자열을 찾는다. 교체는 태영과 함께 한다.
    internal static class CopyReferenceFinder {
        private static readonly string[] AssetExtensions = { ".prefab", ".unity", ".asset" };
        private const string CodeMarker = "PlayerCopy";

        public static string Report() {
            var text = new StringBuilder($"\n## {PlayerPrefabPaths.HtyCopy}를 참조하는 곳 (교체는 2단계에 태영과 함께)\n");
            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && AssetExtensions.Contains(Path.GetExtension(p)) && p != PlayerPrefabPaths.HtyCopy)
                .Where(p => AssetDatabase.GetDependencies(p, false).Contains(PlayerPrefabPaths.HtyCopy))
                .ToList();
            text.AppendLine($"[에셋 직접 참조] {(assets.Count == 0 ? "없음" : "")}");
            foreach (var path in assets)
                text.AppendLine("  - " + path);

            var code = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Replace('\\', '/').StartsWith("Assets/LDY/"))
                .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (f, i, line)))
                .Where(x => x.line.Contains(CodeMarker))
                .ToList();
            text.AppendLine($"[코드 문자열 '{CodeMarker}' (LDY 제외)] {(code.Count == 0 ? "없음" : "")}");
            foreach (var (file, index, line) in code)
                text.AppendLine($"  - {file.Replace('\\', '/')}:{index + 1} {line.Trim()}");

            return text.ToString();
        }
    }
}
