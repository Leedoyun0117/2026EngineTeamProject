using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Hud.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // ingame_hud 스프라이트와 manifest 조회. 프레임이 여러 개인 그림은 "<이름>_strip.png" 한 장에 가로로 붙어 있다.
    internal sealed class HudSprites {
        public const string StripSuffix = "_strip";

        private readonly Dictionary<string, HudManifestEntry> _manifest;
        private readonly HudLog _log;

        private HudSprites(Dictionary<string, HudManifestEntry> manifest, HudLog log) {
            _manifest = manifest;
            _log = log;
        }

        public static HudSprites Load(HudLog log) {
            if (!File.Exists(HudPaths.Manifest)) {
                log.Warn("manifest 없음 " + HudPaths.Manifest);
                return null;
            }

            return new HudSprites(HudManifest.Parse(File.ReadAllText(HudPaths.Manifest)), log);
        }

        public IReadOnlyDictionary<string, HudManifestEntry> Manifest => _manifest;

        public static string KeyOf(string fileName) =>
            fileName.EndsWith(StripSuffix) ? fileName.Substring(0, fileName.Length - StripSuffix.Length) : fileName;

        public HudManifestEntry Entry(string key) => _manifest.TryGetValue(key, out var entry) ? entry : null;

        public Sprite Single(string key) {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{HudPaths.Sprites}/{key}.png");
            if (sprite == null)
                _log.Warn($"스프라이트 없음 {key}.png");
            return sprite;
        }

        public Sprite[] Frames(string key) {
            var path = $"{HudPaths.Sprites}/{key}{StripSuffix}.png";
            var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .Where(s => s.name.StartsWith(key + "_"))
                .OrderBy(s => FrameIndex(s.name))
                .ToArray();
            if (frames.Length == 0)
                _log.Warn($"프레임 없음 {path} (1. Import Art 먼저)");
            return frames;
        }

        public int FrameMilliseconds(string key) {
            var entry = Entry(key);
            return entry != null && entry.FrameMilliseconds > 0 ? entry.FrameMilliseconds : 120;
        }

        public Image.Type ImageType(string key) {
            var entry = Entry(key);
            if (entry == null)
                return Image.Type.Simple;
            if (entry.IsTiled)
                return Image.Type.Tiled;
            return entry.HasBorder ? Image.Type.Sliced : Image.Type.Simple;
        }

        private static int FrameIndex(string spriteName) {
            var underscore = spriteName.LastIndexOf('_');
            return underscore >= 0 && int.TryParse(spriteName.Substring(underscore + 1), out var index) ? index : 0;
        }
    }
}
