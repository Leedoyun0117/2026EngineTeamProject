using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // Assets/LDY/Art/Title 아래 PNG를 도트 스프라이트 설정(Sprite, Point, 압축 없음, 밉맵 없음)으로 가져온다.
    public class TitleArtImporter : AssetPostprocessor {
        public const string Folder = "Assets/LDY/Art/Title";

        // 모래시계 프레임. 도트 임포트 대상 폴더는 ArtFolders 한 곳에서만 늘린다.
        public const string HourglassFolder = "Assets/LDY/Art/HourGlass";

        private const int DefaultPixelsPerUnit = 100;

        // 창 UI 도트는 2배 확대된 키트라 1픽셀이 1/60유닛이 되게 맞춘다 (WindowKit.Pixel과 같은 값).
        private const int UiPixelsPerUnit = 60;
        private const string UiFolder = Folder + "/UI";

        public static readonly string[] ArtFolders = { Folder, HourglassFolder };

        // 9-slice 경계(px). 이 스프라이트들은 Sliced로 그려지므로 Full Rect 메시를 쓴다.
        private static readonly Dictionary<string, int> SliceBorders = new() {
            { "window_frame", 12 }, { "slider_track", 8 }, { "dropdown_box", 8 },
            { "dropdown_list_bg", 8 }, { "focus_frame", 8 }
        };

        // 파일 이름별 PPU 예외. 커서(48x72)가 폭 1, 높이 1.5유닛으로 서서 아이콘 타일(약 1.9유닛)과 어울리게 한다.
        private static readonly Dictionary<string, int> PixelsPerUnitOverrides = new() {
            { "player_cursor", 48 }
        };

        private void OnPreprocessTexture() {
            if (!IsTitleArt(assetPath))
                return;

            Apply((TextureImporter)assetImporter, assetPath);
        }

        [MenuItem("LDY/Title/Reimport Title Art")]
        public static void ReimportAll() {
            foreach (var folder in ArtFolders)
                AssetDatabase.ImportAsset(folder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }

        private static bool IsTitleArt(string path) {
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var folder in ArtFolders)
                if (path.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private static void Apply(TextureImporter importer, string path) {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            var name = Path.GetFileNameWithoutExtension(path);
            var isUi = path.StartsWith(UiFolder + "/", StringComparison.OrdinalIgnoreCase);
            importer.spritePixelsPerUnit = isUi ? UiPixelsPerUnit
                : PixelsPerUnitOverrides.TryGetValue(name, out var ppu) ? ppu
                : DefaultPixelsPerUnit;

            if (isUi && SliceBorders.TryGetValue(name, out var border)) {
                importer.spriteBorder = new Vector4(border, border, border, border);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
            }
        }
    }
}