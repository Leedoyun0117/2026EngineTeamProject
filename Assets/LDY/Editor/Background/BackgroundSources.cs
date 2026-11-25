using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using UnityEditor;

namespace LDY.BackgroundEditor {
    internal static class BackgroundSources {
        public static IReadOnlyList<StageBackgroundSource> All() {
            return AssetDatabase.FindAssets("t:" + nameof(StageBackgroundSource))
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .Select(p => AssetDatabase.LoadAssetAtPath<StageBackgroundSource>(p))
                .Where(s => s != null)
                .ToList();
        }

        public static bool TryRead(StageBackgroundSource source, BackgroundBuildLog log, out StageBackgroundLayout layout) {
            layout = null;
            if (!AssetDatabase.IsValidFolder(source.FolderPath) || !File.Exists(source.JsonPath)) {
                log.Warn($"{source.name}: 데이터 없음 ({source.FolderPath})");
                return false;
            }

            try {
                layout = StageBackgroundLoader.Load(File.ReadAllText(source.JsonPath), source.FolderPath,
                    source.PixelsPerUnit, source.JsonPath, File.Exists);
            }
            catch (BackgroundDataException e) {
                log.Warn($"{source.name}: json 오류 {e.Message}");
                return false;
            }

            var missing = layout.RequiredFiles.Where(f => !File.Exists(f)).ToList();
            if (missing.Count > 0) {
                log.Warn($"{source.name}: json이 참조하는 파일 없음 -> 중단\n    " + string.Join("\n    ", missing));
                return false;
            }

            foreach (var key in layout.UnsupportedWidgets) log.Warn($"{source.name}: 지원 목록에 없는 위젯 '{key}' 생성 안 함");
            return true;
        }
    }
}
