using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace LDY.FontsEditor {
    // 팀 폴더(Assets/Main/05.Fonts)의 TTF는 옮기거나 복사하지 않고 그대로 참조한다.
    // 만든 TMP 에셋은 LDY 폴더에 둔다. 샘플링 크기는 각 폰트의 픽셀 격자 크기.
    internal static class PixelFontSources {
        private const string TeamFolder = "Assets/Main/05.Fonts/fonts";
        private const string OutputFolder = "Assets/LDY/Fonts";

        private static readonly (string folder, string file, int pointSize)[] Fonts = {
            (TeamFolder, "Galmuri11.ttf", 12),
            (TeamFolder, "Galmuri11-Bold.ttf", 12),
            (TeamFolder, "Galmuri9.ttf", 10),
            (TeamFolder, "neodgm.ttf", 16)
        };

        public static void Sync(PixelFontCatalog catalog, List<string> log) {
            var changed = false;
            foreach (var (folder, file, pointSize) in Fonts) {
                var source = folder + "/" + file;
                if (!File.Exists(source)) {
                    log.Add($"없음, 건너뜀 {source}");
                    continue;
                }

                var entry = new PixelFontCatalog.Entry {
                    sourcePath = source,
                    samplingPointSize = pointSize,
                    outputPath = $"{OutputFolder}/{Path.GetFileNameWithoutExtension(file)} Pixel.asset"
                };
                if (!catalog.Upsert(entry)) continue;
                changed = true;
                log.Add($"카탈로그 {entry.outputPath} <- {source} ({pointSize}pt)");
            }

            if (changed) EditorUtility.SetDirty(catalog);
        }
    }
}
