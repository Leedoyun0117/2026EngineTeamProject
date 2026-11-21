using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // PlayerAnimSet_Cursor의 클립마다 실제로 들어간 스프라이트를 Console과 PNG로 내보낸다.
    internal static class CursorClipDumper {
        public const string OutputPath = "Assets/LDY/Debug/cursor_clips.png";

        public static bool Run(List<string> report) {
            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(PlayerAnimSetBuilder.CursorSetPath);
            if (set == null) {
                report.Add($"중단: {PlayerAnimSetBuilder.CursorSetPath} 가 없음. 2번을 먼저 실행하세요");
                return false;
            }

            var rows = new List<ClipSheetRow>();
            foreach (var entry in set.Clips) {
                var sprites = entry.clip?.frames ?? new Sprite[0];
                var name = CursorTagSpecs.NameOf(entry.id);
                rows.Add(new ClipSheetRow(name, sprites));
                report.Add($"{entry.id} → {name} → {Describe(sprites)}");
            }

            if (rows.Count == 0) {
                report.Add("중단: 세트에 클립이 없음");
                return false;
            }

            WritePng(rows);
            report.Add($"그림 저장: {OutputPath}");
            return true;
        }

        private static string Describe(IEnumerable<Sprite> sprites) {
            return string.Join(", ", sprites.Select(s => s != null ? $"{s.name}[{SourceOf(s)}]" : "(null)"));
        }

        private static string SourceOf(Sprite sprite) {
            return Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite));
        }

        private static void WritePng(IReadOnlyList<ClipSheetRow> rows) {
            var texture = ClipSheetRenderer.Render(rows);
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
                File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
            }
            finally {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(OutputPath);
        }
    }
}
