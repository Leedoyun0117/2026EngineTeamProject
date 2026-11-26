using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace LDY.FontsEditor {
    internal sealed class PixelFontAssetBuilder {
        private const int AtlasSize = 1024;
        private const int Padding = 1;
        private const GlyphRenderMode RenderMode = GlyphRenderMode.RASTER;

        public void Build(PixelFontCatalog.Entry entry, List<string> log) {
            var font = AssetDatabase.LoadAssetAtPath<Font>(entry.sourcePath);
            if (font == null) {
                log.Add($"경고: TTF 없음 {entry.sourcePath}");
                return;
            }

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(entry.outputPath);
            if (existing != null) {
                Verify(existing, entry, log);
                ApplyPointFilter(existing);
                log.Add("유지 " + entry.outputPath);
                return;
            }

            var asset = TMP_FontAsset.CreateFontAsset(font, entry.samplingPointSize, Padding, RenderMode, AtlasSize,
                AtlasSize, AtlasPopulationMode.Dynamic);
            if (asset == null) {
                log.Add($"경고: 폰트 에셋 생성 실패 {entry.sourcePath}");
                return;
            }

            EnsureFolder(Path.GetDirectoryName(entry.outputPath));
            asset.name = Path.GetFileNameWithoutExtension(entry.outputPath);
            AssetDatabase.CreateAsset(asset, entry.outputPath);
            asset.atlasTextures[0].name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = asset.name + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            ApplyPointFilter(asset);
            log.Add($"만듦 {entry.outputPath} (Dynamic, {entry.samplingPointSize}pt, {RenderMode})");
        }

        private static void Verify(TMP_FontAsset asset, PixelFontCatalog.Entry entry, List<string> log) {
            if ((int)asset.faceInfo.pointSize != entry.samplingPointSize)
                log.Add($"경고: {entry.outputPath} 샘플링 크기 {asset.faceInfo.pointSize} != {entry.samplingPointSize}");
            if (asset.atlasRenderMode != RenderMode)
                log.Add($"경고: {entry.outputPath} 렌더 모드 {asset.atlasRenderMode} != {RenderMode}");
            if (asset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
                log.Add($"경고: {entry.outputPath} Dynamic이 아님");
        }

        private static void ApplyPointFilter(TMP_FontAsset asset) {
            foreach (var texture in asset.atlasTextures.Where(t => t != null && t.filterMode != FilterMode.Point)) {
                texture.filterMode = FilterMode.Point;
                EditorUtility.SetDirty(texture);
            }

            EditorUtility.SetDirty(asset);
        }

        private static void EnsureFolder(string folder) {
            folder = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }

    internal static class PixelFontMenu {
        private const string CatalogPath = "Assets/LDY/Data/Fonts/PixelFontCatalog.asset";

        [MenuItem("LDY/Fonts/Build Pixel Font Assets")]
        private static void BuildPixelFontAssets() {
            var log = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<PixelFontCatalog>(CatalogPath);
            if (catalog == null) log.Add("경고: 카탈로그 없음 " + CatalogPath);
            else {
                PixelFontSources.Sync(catalog, log);
                foreach (var entry in catalog.Entries)
                    new PixelFontAssetBuilder().Build(entry, log);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("LDY Pixel Fonts\n  " + string.Join("\n  ", log));
        }
    }
}
