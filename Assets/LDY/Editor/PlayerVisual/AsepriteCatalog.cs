using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal readonly struct AsepriteInfo {
        public readonly string Path;
        public readonly string ImporterType;
        public readonly int SpriteCount;
        public readonly string[] ClipNames;
        public readonly DateTime LastWrite;

        public AsepriteInfo(string path, string importerType, int spriteCount, string[] clipNames, DateTime lastWrite) {
            Path = path;
            ImporterType = importerType;
            SpriteCount = spriteCount;
            ClipNames = clipNames;
            LastWrite = lastWrite;
        }
    }

    internal static class AsepriteCatalog {
        private const int V2FrameCount = 43;
        private static readonly string[] RopeTags = { "rope_hang", "rope_climb", "rope_climb_down" };

        public static List<AsepriteInfo> Scan() {
            var found = new List<AsepriteInfo>();
            foreach (var guid in AssetDatabase.FindAssets("player_anim_pink")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".aseprite", StringComparison.OrdinalIgnoreCase))
                    continue;

                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                var importer = AssetImporter.GetAtPath(path);
                found.Add(new AsepriteInfo(path, importer != null ? importer.GetType().Name : "null",
                    assets.OfType<Sprite>().Count(), assets.OfType<AnimationClip>().Select(c => c.name).ToArray(),
                    File.GetLastWriteTime(path)));
            }

            return found;
        }

        // 클립 이름은 임포터가 태그 이름으로 만든다. 클립이 하나도 없으면 프레임 수만 본다.
        public static bool IsV2Candidate(in AsepriteInfo info) {
            if (info.SpriteCount != V2FrameCount)
                return false;

            var clips = info.ClipNames;
            return clips.Length == 0 || RopeTags.All(tag => clips.Contains(tag));
        }
    }
}
