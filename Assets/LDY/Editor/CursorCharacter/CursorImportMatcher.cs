using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace LDY.Script {
    // 새 캐릭터 aseprite의 임포트 설정 여섯 가지(Import Mode, Layer Import Mode, PPU, Pivot, Filter, Compression)만
    // pinkV2에서 읽어 직접 대입한다. 스프라이트 이름, 레이어 데이터, ID는 건드리지 않는다.
    internal static class CursorImportMatcher {
        private const string Settings = "asepriteImporterSettings.";
        private static readonly BuildTarget Platform = BuildTarget.StandaloneWindows64;

        public static bool MatchByName(List<string> report) {
            return CursorAssetLocator.TryFind(CursorAssetLocator.PinkV2File, out var reference, report)
                   && CursorAssetLocator.TryFind(CursorAssetLocator.CursorFile, out var target, report)
                   && Match(reference, target, report);
        }

        public static bool Match(string referencePath, string targetPath, List<string> report) {
            if (!TryLoad(referencePath, report, out var reference) || !TryLoad(targetPath, report, out var target))
                return false;

            var problems = new List<string>();
            if (!TryRead(reference, referencePath, problems, out var wanted)
                || !TryRead(target, targetPath, problems, out var current)) {
                report.AddRange(problems);
                return false;
            }

            if (wanted.Equals(current)) {
                report.Add("임포트 설정 6가지가 이미 pinkV2와 같음");
                return true;
            }

            Apply(target, wanted);
            target.SaveAndReimport();
            report.Add($"임포트 설정 6가지를 pinkV2에 맞춤: {wanted}");
            return true;
        }

        private static bool TryLoad(string path, List<string> report, out AsepriteImporter importer) {
            var asset = AssetImporter.GetAtPath(path);
            importer = asset as AsepriteImporter;
            if (importer != null)
                return true;

            report.Add(asset == null
                ? $"중단: {path} 의 임포터가 null"
                : $"중단: {path} 의 임포터가 AsepriteImporter가 아니라 {asset.GetType().FullName}");
            return false;
        }

        private static bool TryRead(AsepriteImporter importer, string path, List<string> problems, out ImportValues values) {
            var so = new SerializedObject(importer);
            var fileMode = Find(so, "fileImportMode", path, problems);
            var layerMode = Find(so, "layerImportMode", path, problems);
            var pivotSpace = Find(so, "defaultPivotSpace", path, problems);
            var pivotPosition = Find(so, "customPivotPosition", path, problems);
            var platform = importer.GetImporterPlatformSettings(Platform);

            values = default;
            if (problems.Count > 0) {
                problems.Add($"{path} 의 최상위 속성: {string.Join(", ", TopLevelNames(so))}");
                return false;
            }

            values = new ImportValues {
                FileImportMode = fileMode.intValue,
                LayerImportMode = layerMode.intValue,
                PixelsPerUnit = importer.spritePixelsPerUnit,
                PivotSpace = pivotSpace.intValue,
                PivotAlignment = importer.pivotAlignment,
                PivotPosition = pivotPosition.vector2Value,
                Filter = importer.filterMode,
                Compression = platform?.textureCompression
            };
            return true;
        }

        private static SerializedProperty Find(SerializedObject so, string name, string path, List<string> problems) {
            var property = so.FindProperty(Settings + name);
            if (property == null)
                problems.Add($"중단: {path} 에 속성 {Settings}{name} 이 없음");

            return property;
        }

        private static IEnumerable<string> TopLevelNames(SerializedObject so) {
            var iterator = so.GetIterator();
            var names = new List<string>();
            for (var enter = true; iterator.NextVisible(enter); enter = false)
                names.Add(iterator.name);

            return names.Distinct();
        }

        private static void Apply(AsepriteImporter importer, ImportValues values) {
            var so = new SerializedObject(importer);
            so.FindProperty(Settings + "fileImportMode").intValue = values.FileImportMode;
            so.FindProperty(Settings + "layerImportMode").intValue = values.LayerImportMode;
            so.FindProperty(Settings + "defaultPivotSpace").intValue = values.PivotSpace;
            so.FindProperty(Settings + "customPivotPosition").vector2Value = values.PivotPosition;
            so.ApplyModifiedPropertiesWithoutUndo();

            importer.spritePixelsPerUnit = values.PixelsPerUnit;
            importer.pivotAlignment = values.PivotAlignment;
            importer.filterMode = values.Filter;
            ApplyCompression(importer, values.Compression);
            EditorUtility.SetDirty(importer);
        }

        private static void ApplyCompression(AsepriteImporter importer, TextureImporterCompression? compression) {
            var platform = importer.GetImporterPlatformSettings(Platform);
            if (!compression.HasValue || platform == null)
                return;

            platform.textureCompression = compression.Value;
            importer.SetImporterPlatformSettings(platform);
        }

        private struct ImportValues : IEquatable<ImportValues> {
            public int FileImportMode;
            public int LayerImportMode;
            public float PixelsPerUnit;
            public int PivotSpace;
            public SpriteAlignment PivotAlignment;
            public Vector2 PivotPosition;
            public FilterMode Filter;
            public TextureImporterCompression? Compression;

            public bool Equals(ImportValues other) {
                return FileImportMode == other.FileImportMode && LayerImportMode == other.LayerImportMode
                       && Mathf.Approximately(PixelsPerUnit, other.PixelsPerUnit) && PivotSpace == other.PivotSpace
                       && PivotAlignment == other.PivotAlignment && PivotPosition == other.PivotPosition
                       && Filter == other.Filter && Compression == other.Compression;
            }

            public override bool Equals(object obj) {
                return obj is ImportValues other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(FileImportMode, LayerImportMode, PixelsPerUnit, PivotSpace, PivotAlignment,
                    Filter, Compression);
            }

            public override string ToString() {
                return $"importMode={FileImportMode}, layerMode={LayerImportMode}, ppu={PixelsPerUnit}, "
                       + $"pivot={PivotAlignment}/{PivotPosition}/space{PivotSpace}, filter={Filter}, "
                       + $"compression={(Compression.HasValue ? Compression.Value.ToString() : "기본값")}";
            }
        }
    }
}
