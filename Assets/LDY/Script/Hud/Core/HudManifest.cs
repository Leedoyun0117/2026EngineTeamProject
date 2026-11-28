using System.Collections.Generic;
using LDY.Background.Core;

namespace LDY.Hud.Core {
    public sealed class HudManifestEntry {
        public string Name;
        public int FrameWidth;
        public int Height;
        public int Left;
        public int Bottom;
        public int Right;
        public int Top;
        public string ImageType;
        public int Frames = 1;
        public int FrameMilliseconds;

        public bool HasBorder => Left > 0 || Bottom > 0 || Right > 0 || Top > 0;
        public bool IsTiled => ImageType == "Tiled";
    }

    // ingame_hud/manifest.json. size는 한 프레임 크기, border_LBRT는 프레임마다의 9-slice.
    public static class HudManifest {
        public static Dictionary<string, HudManifestEntry> Parse(string json) {
            var result = new Dictionary<string, HudManifestEntry>();
            if (!(JsonParser.Parse(json) is Dictionary<string, object> root))
                return result;

            foreach (var pair in root) {
                if (pair.Value is Dictionary<string, object> spec)
                    result[pair.Key] = Read(pair.Key, spec);
            }

            return result;
        }

        private static HudManifestEntry Read(string name, Dictionary<string, object> spec) {
            var entry = new HudManifestEntry { Name = name };
            var size = Ints(spec, "size");
            if (size.Count >= 2) {
                entry.FrameWidth = size[0];
                entry.Height = size[1];
            }

            var border = Ints(spec, "border_LBRT");
            if (border.Count >= 4) {
                entry.Left = border[0];
                entry.Bottom = border[1];
                entry.Right = border[2];
                entry.Top = border[3];
            }

            if (spec.TryGetValue("imageType", out var type))
                entry.ImageType = type as string;
            if (spec.TryGetValue("frames", out var frames) && frames is double f && f >= 1)
                entry.Frames = (int)f;
            if (spec.TryGetValue("ms", out var ms) && ms is double m)
                entry.FrameMilliseconds = (int)m;
            return entry;
        }

        private static List<int> Ints(Dictionary<string, object> spec, string key) {
            var list = new List<int>();
            if (!spec.TryGetValue(key, out var value) || !(value is List<object> items))
                return list;

            foreach (var item in items)
                if (item is double d)
                    list.Add((int)d);
            return list;
        }
    }
}
