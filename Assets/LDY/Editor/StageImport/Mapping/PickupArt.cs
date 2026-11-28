using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 능력 이름으로 흡수 픽업 그림(ui_{능력})의 첫 프레임을 찾는다. 없으면 null.
    internal static class PickupArt {
        private static readonly string[] Extensions = { ".aseprite", ".png" };
        private const string FirstFrame = "Frame_0";

        public static Sprite Find(string ability) {
            if (string.IsNullOrEmpty(ability)) return null;

            foreach (var folder in StageImportPaths.PickupArtFolders)
            foreach (var extension in Extensions) {
                var sprites = AssetDatabase.LoadAllAssetsAtPath($"{folder}/ui_{ability}{extension}").OfType<Sprite>()
                    .OrderBy(s => s.name).ToList();
                if (sprites.Count > 0) return sprites.FirstOrDefault(s => s.name == FirstFrame) ?? sprites[0];
            }

            return null;
        }
    }
}
