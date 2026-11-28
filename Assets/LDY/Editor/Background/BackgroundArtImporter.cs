using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundArtImporter {
        public const string WhitePixelPath = "Assets/LDY/Art/BackgroundShared/bg_white_px.png";

        public void Import(StageBackgroundLayout layout, float pixelsPerUnit, BackgroundBuildLog log) {
            foreach (var file in layout.SingleSpriteFiles)
                if (ConfigureSingle(file, pixelsPerUnit))
                    log.Info("Import single " + file);

            foreach (var sheet in layout.Sheets)
                if (ConfigureSheet(sheet.File, sheet.Frames, pixelsPerUnit))
                    log.Info($"Import sheet {sheet.File} ({sheet.Frames} frames)");
        }

        public void EnsureWhitePixel(BackgroundBuildLog log) {
            if (!File.Exists(WhitePixelPath)) {
                Directory.CreateDirectory(Path.GetDirectoryName(WhitePixelPath));
                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                File.WriteAllBytes(WhitePixelPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(WhitePixelPath);
                log.Info("Created " + WhitePixelPath);
            }

            ConfigureSingle(WhitePixelPath, 1f);
        }

        private static bool ConfigureSingle(string path, float pixelsPerUnit) {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;
            var dirty = ApplyCommon(importer, SpriteImportMode.Single, pixelsPerUnit);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteAlignment != (int)SpriteAlignment.Center) {
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                dirty = true;
            }

            if (dirty) importer.SaveAndReimport();
            return dirty;
        }

        private static bool ConfigureSheet(string path, int frames, float pixelsPerUnit) {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;
            var dirty = ApplyCommon(importer, SpriteImportMode.Multiple, pixelsPerUnit);
            if (dirty) importer.SaveAndReimport();
            return SliceStrip(importer, path, frames) || dirty;
        }

        private static bool ApplyCommon(TextureImporter importer, SpriteImportMode mode, float pixelsPerUnit) {
            var dirty = importer.textureType != TextureImporterType.Sprite
                        || importer.spriteImportMode != mode
                        || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit)
                        || importer.filterMode != FilterMode.Point
                        || importer.textureCompression != TextureImporterCompression.Uncompressed
                        || importer.mipmapEnabled;
            if (!dirty) return false;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = mode;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            return true;
        }

        private static bool SliceStrip(TextureImporter importer, string path, int frames) {
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var frameWidth = width / frames;
            var baseName = Path.GetFileNameWithoutExtension(path);

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var existing = provider.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First());

            var rects = Enumerable.Range(0, frames).Select(i => new SpriteRect {
                name = $"{baseName}_{i}",
                rect = new Rect(i * frameWidth, 0, frameWidth, height),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = existing.TryGetValue($"{baseName}_{i}", out var old) ? old.spriteID : GUID.Generate()
            }).ToArray();

            if (SameSlices(existing.Values, rects)) return false;
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        private static bool SameSlices(IEnumerable<SpriteRect> current, IReadOnlyCollection<SpriteRect> wanted) {
            var list = current.ToList();
            return list.Count == wanted.Count &&
                   wanted.All(w => list.Any(c => c.name == w.name && c.rect == w.rect && c.alignment == w.alignment));
        }
    }
}
