using System.Collections.Generic;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace LDY.Script {
    internal static class CursorPpuStep {
        public static bool Run(List<string> report) {
            if (!CursorAssetLocator.TryFind(CursorAssetLocator.CursorFile, out var path, report))
                return false;

            if (AssetImporter.GetAtPath(path) is not AsepriteImporter importer) {
                report.Add($"중단: {path} 의 임포터가 AsepriteImporter가 아님");
                return false;
            }

            if (Mathf.Approximately(importer.spritePixelsPerUnit, PlayerArt.PixelsPerUnit)) {
                report.Add($"{path} PPU는 이미 {PlayerArt.PixelsPerUnit}");
                return true;
            }

            var before = importer.spritePixelsPerUnit;
            importer.spritePixelsPerUnit = PlayerArt.PixelsPerUnit;
            importer.SaveAndReimport();
            report.Add($"{path} PPU {before} -> {PlayerArt.PixelsPerUnit} (다른 임포트 설정은 유지)");
            return true;
        }
    }
}
