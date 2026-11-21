using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LDY.StageEditor {
    internal sealed class ArtLocator {
        private const string FolderMarker = "stage1_music_objects";

        public string Find(string fileName) {
            var term = Path.GetFileNameWithoutExtension(fileName);
            foreach (var guid in AssetDatabase.FindAssets(term)) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(FolderMarker) && path.EndsWith("/" + fileName, StringComparison.OrdinalIgnoreCase))
                    return path;
            }

            return null;
        }

        public string Folder() {
            var manifest = Find("manifest.json");
            return manifest == null ? null : Path.GetDirectoryName(manifest).Replace('\\', '/');
        }
    }

    internal sealed class AsepriteArt {
        public string Path;
        public Sprite First;
        public RuntimeAnimatorController Controller;
        public Vector2 SizeUnits => First != null ? (Vector2)First.bounds.size : Vector2.zero;
    }

    internal sealed class AsepriteArtLoader {
        private readonly ArtLocator _locator;

        public AsepriteArtLoader(ArtLocator locator) {
            _locator = locator;
        }

        public AsepriteArt Load(string name) {
            var path = _locator.Find(name + ".aseprite");
            if (path == null) return null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var first = assets.OfType<Sprite>().OrderBy(s => FrameIndex(s.name)).FirstOrDefault();
            var controller = assets.OfType<RuntimeAnimatorController>().FirstOrDefault();
            return new AsepriteArt { Path = path, First = first, Controller = controller };
        }

        private static int FrameIndex(string spriteName) {
            var cut = spriteName.LastIndexOf('_');
            return cut >= 0 && int.TryParse(spriteName.Substring(cut + 1), out var n) ? n : int.MaxValue;
        }
    }

    internal static class Stage1ArtRules {
        public const float PixelsPerUnit = 40f;

        public static SpriteAlignment PivotOf(string artName) {
            return artName == "button" || artName == "long_press_button"
                ? SpriteAlignment.BottomCenter
                : SpriteAlignment.Center;
        }
    }

    internal sealed class AsepriteConfigurator {
        public bool Configure(string path) {
            var importer = AssetImporter.GetAtPath(path) as AsepriteImporter;
            if (importer == null) return false;

            var pivot = Stage1ArtRules.PivotOf(Path.GetFileNameWithoutExtension(path));
            var settings = importer.GetImporterPlatformSettings(BuildTarget.StandaloneWindows64);
            var dirty = !Mathf.Approximately(importer.spritePixelsPerUnit, Stage1ArtRules.PixelsPerUnit)
                        || importer.filterMode != FilterMode.Point
                        || importer.pivotAlignment != pivot
                        || settings.textureCompression != TextureImporterCompression.Uncompressed;
            if (!dirty) return false;

            importer.spritePixelsPerUnit = Stage1ArtRules.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.pivotAlignment = pivot;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetImporterPlatformSettings(settings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            return true;
        }
    }

    internal sealed class TilesetImporter {
        private const int TilePixels = 40;
        private const int ExpectedWidth = 160;
        private const int ExpectedHeight = 240;

        public static bool HasExpectedSize(string pngPath) {
            var texture = new Texture2D(2, 2);
            try {
                texture.LoadImage(File.ReadAllBytes(pngPath));
                return texture.width == ExpectedWidth && texture.height == ExpectedHeight;
            }
            finally {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public void Import(string pngPath) {
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = Stage1ArtRules.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var existing = provider.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First());

            var rects = LDY.Stage.TilesetLayout.AllNames().Select((name, i) => {
                var cell = LDY.Stage.TilesetLayout.RectAt(i % LDY.Stage.TilesetLayout.Columns,
                    i / LDY.Stage.TilesetLayout.Columns, TilePixels);
                return new SpriteRect {
                    name = name,
                    rect = new Rect(cell.X, cell.Y, cell.Width, cell.Height),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = existing.TryGetValue(name, out var old) ? old.spriteID : GUID.Generate()
                };
            }).ToArray();

            provider.SetSpriteRects(rects);
            var nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameProvider.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }
    }
}
