using System.IO;
using System.Linq;
using LDY.Hud.Core;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LDY.Hud.HudEditor {
    // manifest 기준으로 Point·비압축·FullRect·9-slice 임포트. 프레임 그림은 가로로 잘라 프레임마다 같은 border를 준다.
    // 선택 점선(hud_select_*)은 월드 SpriteRenderer용이라 PPU 40, 나머지는 UI용 PPU 100.
    internal static class HudArtImporter {
        private const float UiPixelsPerUnit = 100f;
        private const float WorldPixelsPerUnit = 40f;
        private static readonly string[] WorldSprites = { "hud_select_ylw", "hud_select_red" };

        public static void Run(HudLog log) {
            var sprites = HudSprites.Load(log);
            if (sprites == null)
                return;

            foreach (var file in Directory.GetFiles(HudPaths.Sprites, "*.png")) {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name.StartsWith("preview_"))
                    continue;

                var key = HudSprites.KeyOf(name);
                var entry = sprites.Entry(key);
                if (entry == null) {
                    log.Warn($"manifest에 {key} 없음, 건너뜀");
                    continue;
                }

                var path = $"{HudPaths.Sprites}/{name}.png";
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) {
                    log.Warn("임포터 없음 " + path);
                    continue;
                }

                var ppu = WorldSprites.Contains(key) ? WorldPixelsPerUnit : UiPixelsPerUnit;
                var changed = entry.Frames > 1 ? ImportStrip(importer, key, entry, ppu) : ImportSingle(importer, entry, ppu);
                if (changed)
                    log.Info($"임포트 {name} (frames {entry.Frames}, PPU {ppu})");
            }

            foreach (var key in sprites.Manifest.Keys.Where(k => !File.Exists($"{HudPaths.Sprites}/{k}.png") &&
                                                                 !File.Exists($"{HudPaths.Sprites}/{k}{HudSprites.StripSuffix}.png")))
                log.Warn($"manifest에는 있지만 PNG 없음: {key}");
        }

        private static bool ImportSingle(TextureImporter importer, HudManifestEntry entry, float ppu) {
            var border = Border(entry);
            var changed = ApplyCommon(importer, SpriteImportMode.Single, ppu) | importer.spriteBorder != border;
            if (!changed)
                return false;

            importer.spriteBorder = border;
            importer.SaveAndReimport();
            return true;
        }

        private static bool ImportStrip(TextureImporter importer, string key, HudManifestEntry entry, float ppu) {
            var settingsChanged = ApplyCommon(importer, SpriteImportMode.Multiple, ppu);
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            var existing = provider.GetSpriteRects();
            var rects = Enumerable.Range(0, entry.Frames).Select(i => Frame(key, entry, i, existing)).ToArray();
            if (!settingsChanged && SameRects(existing, rects))
                return false;

            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        private static SpriteRect Frame(string key, HudManifestEntry entry, int index, SpriteRect[] existing) {
            var name = $"{key}_{index}";
            var previous = existing.FirstOrDefault(r => r.name == name);
            return new SpriteRect {
                name = name,
                rect = new Rect(index * entry.FrameWidth, 0, entry.FrameWidth, entry.Height),
                border = Border(entry),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = previous != null ? previous.spriteID : GUID.Generate()
            };
        }

        private static bool SameRects(SpriteRect[] a, SpriteRect[] b) =>
            a.Length == b.Length && a.Zip(b, (x, y) => x.name == y.name && x.rect == y.rect && x.border == y.border).All(same => same);

        private static bool ApplyCommon(TextureImporter importer, SpriteImportMode mode, float ppu) {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            var changed = importer.textureType != TextureImporterType.Sprite
                          || importer.spriteImportMode != mode
                          || !Mathf.Approximately(importer.spritePixelsPerUnit, ppu)
                          || importer.filterMode != FilterMode.Point
                          || importer.textureCompression != TextureImporterCompression.Uncompressed
                          || importer.mipmapEnabled
                          || settings.spriteMeshType != SpriteMeshType.FullRect;
            if (!changed)
                return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = mode;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            return true;
        }

        // border_LBRT [L,B,R,T] -> spriteBorder (x=L, y=B, z=R, w=T)
        private static Vector4 Border(HudManifestEntry entry) => new Vector4(entry.Left, entry.Bottom, entry.Right, entry.Top);
    }
}
