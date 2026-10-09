using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor
{
    // Assets/LDY/Art/Title 아래 PNG를 도트 스프라이트 설정(Sprite, Point, 압축 없음, 밉맵 없음)으로 가져온다.
    public class TitleArtImporter : AssetPostprocessor
    {
        public const string Folder = "Assets/LDY/Art/Title";

        private const int DefaultPixelsPerUnit = 100;
        // 창 UI 도트는 2배 확대된 키트라 1픽셀이 1/60유닛이 되게 맞춘다 (WindowKit.Pixel과 같은 값).
        private const int UiPixelsPerUnit = 60;
        private const string UiFolder = Folder + "/UI";

        // 9-slice 경계(px). 이 스프라이트들은 Sliced로 그려지므로 Full Rect 메시를 쓴다.
        private static readonly Dictionary<string, int> SliceBorders = new Dictionary<string, int>
        {
            { "window_frame", 12 }, { "slider_track", 8 }, { "dropdown_box", 8 },
            { "dropdown_list_bg", 8 }, { "focus_frame", 8 }
        };

        // 파일 이름별 PPU 예외. 커서(48x72)가 폭 1, 높이 1.5유닛으로 서서 아이콘 타일(약 1.9유닛)과 어울리게 한다.
        private static readonly Dictionary<string, int> PixelsPerUnitOverrides = new Dictionary<string, int>
        {
            { "player_cursor", 48 }
        };

        private void OnPreprocessTexture()
        {
            if (!IsTitleArt(assetPath))
                return;

            Apply((TextureImporter)assetImporter, assetPath);
        }

        [MenuItem("LDY/Title/Reimport Title Art")]
        public static void ReimportAll()
        {
            AssetDatabase.ImportAsset(Folder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }

        private static bool IsTitleArt(string path)
        {
            return path.StartsWith(Folder + "/", System.StringComparison.OrdinalIgnoreCase) &&
                   path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
        }

        private static void Apply(TextureImporter importer, string path)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            bool isUi = path.StartsWith(UiFolder + "/", System.StringComparison.OrdinalIgnoreCase);
            importer.spritePixelsPerUnit = isUi ? UiPixelsPerUnit
                : PixelsPerUnitOverrides.TryGetValue(name, out int ppu) ? ppu
                : DefaultPixelsPerUnit;

            if (isUi && SliceBorders.TryGetValue(name, out int border))
            {
                importer.spriteBorder = new Vector4(border, border, border, border);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
            }
        }
    }
}
