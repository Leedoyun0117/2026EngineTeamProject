using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Stage;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace LDY.StageEditor {
    internal static class PickupCatalogs {
        public const string StagePrefix = "Stage1_";
        private const string PickupPrefabPrefix = StagePrefix + "Ui";

        public static Stage1PickupCatalog Load(SetupReport report) {
            var catalog = AssetDatabase.LoadAssetAtPath<Stage1PickupCatalog>(StagePaths.PickupCatalog);
            if (catalog == null) report.Warn("흡수 UI 카탈로그가 없다: " + StagePaths.PickupCatalog);
            return catalog;
        }

        // Stage1_Ui* 프리팹 중 카탈로그에 없는 것. 덮어쓰거나 지우지 않고 보존만 한다.
        public static IEnumerable<string> UnlistedPickupPrefabs(Stage1PickupCatalog catalog) {
            return AssetDatabase.FindAssets("t:Prefab", new[] { StagePaths.Prefabs })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => IsUnlisted(catalog, path))
                .OrderBy(path => path);
        }

        public static bool IsUnlisted(Stage1PickupCatalog catalog, string prefabPath) {
            if (catalog == null) return false;
            var name = Path.GetFileNameWithoutExtension(prefabPath);
            return name.StartsWith(PickupPrefabPrefix) && !catalog.ContainsPrefab(name);
        }
    }

    // 카탈로그 경로에 아직 없는 aseprite를 받은 편지함 폴더에서 AssetDatabase.MoveAsset으로 옮긴다. GUID 유지 확인.
    internal sealed class PickupSpriteMover {
        private const string Inbox = "Assets/Main/04.Sprites";

        public void Run(SetupReport report) {
            var catalog = PickupCatalogs.Load(report);
            if (catalog == null) return;

            foreach (var entry in catalog.Entries) {
                var target = entry.asepritePath;
                var source = Inbox + "/" + Path.GetFileName(target);
                var sourceGuid = GuidOf(source);
                var targetGuid = GuidOf(target);

                if (targetGuid != null && sourceGuid != null) {
                    report.Warn($"중단: {source}와 {target}가 둘 다 있다. 직접 확인 필요");
                    return;
                }

                if (targetGuid != null) continue;
                if (sourceGuid == null) {
                    report.Warn($"{entry.id}: {target}도 {source}도 없다");
                    continue;
                }

                var error = AssetDatabase.MoveAsset(source, target);
                var movedGuid = GuidOf(target);
                if (!string.IsNullOrEmpty(error) || movedGuid != sourceGuid) {
                    report.Warn($"중단: {source} 이동 실패. error='{error}', GUID {sourceGuid} -> {movedGuid ?? "없음"}");
                    return;
                }

                report.Updated($"Move {source} -> {target} (GUID {sourceGuid} 전후 동일)");
            }

            ReportLeftovers(report);
        }

        private static void ReportLeftovers(SetupReport report) {
            if (!AssetDatabase.IsValidFolder(Inbox)) return;
            var left = Directory.GetFiles(Inbox).Where(f => !f.EndsWith(".meta")).ToList();
            if (left.Count == 0) report.Skipped(Inbox + ": 비었음, 폴더는 그대로 둠");
            foreach (var file in left) report.Skipped(file.Replace('\\', '/') + ": 남은 파일, 건드리지 않음");
        }

        private static string GuidOf(string path) {
            if (AssetDatabase.LoadMainAssetAtPath(path) == null) return null;
            var guid = AssetDatabase.AssetPathToGUID(path);
            return string.IsNullOrEmpty(guid) ? null : guid;
        }
    }

    internal static class PickupCatalogRestorer {
        private const string SpriteFolder = "Assets/LDY/Sprites/stage1_music_objects/stage1_music_objects";

        public static readonly Stage1PickupCatalog.Entry Zoom = new Stage1PickupCatalog.Entry {
            id = "zoom",
            asepritePath = SpriteFolder + "/ui_zoom.aseprite",
            prefabName = "Stage1_UiZoom",
            abilityColor = new Color32(0xD1, 0x4B, 0x6B, 0xFF)
        };

        public static void Run(SetupReport report) {
            var catalog = PickupCatalogs.Load(report);
            if (catalog == null) return;
            if (!catalog.Upsert(Zoom)) {
                report.Skipped("zoom: 이미 카탈로그와 같음");
                return;
            }

            EditorUtility.SetDirty(catalog);
            report.Updated($"zoom -> {StagePaths.PickupCatalog} ({Zoom.prefabName}, #D14B6B)");
        }
    }

    internal sealed class PickupVerifier {
        public static readonly string[] ExpectedIds = { "pause", "cut", "drag", "alpha", "light", "tab", "copy", "zoom" };

        public void Run(SetupReport report) {
            var catalog = PickupCatalogs.Load(report);
            if (catalog == null) return;

            VerifyIds(catalog, report);
            foreach (var entry in catalog.Entries)
                VerifyPickup(entry, report);
        }

        private static void VerifyIds(Stage1PickupCatalog catalog, SetupReport report) {
            var ids = catalog.Entries.Select(e => e.id).ToList();
            foreach (var missing in ExpectedIds.Except(ids)) report.Warn($"카탈로그에 {missing} 없음");
            foreach (var extra in ids.Except(ExpectedIds)) report.Warn($"카탈로그에 예상 밖 항목 {extra}");
            foreach (var dup in ids.GroupBy(id => id).Where(g => g.Count() > 1)) report.Warn($"id 중복 {dup.Key}");
            if (ids.Count == ExpectedIds.Length && !ExpectedIds.Except(ids).Any())
                report.Updated($"OK 카탈로그 {ids.Count}종: {string.Join(", ", ids)}");
        }

        private static void VerifyPickup(Stage1PickupCatalog.Entry entry, SetupReport report) {
            var prefabPath = StagePaths.Prefab(entry.prefabName);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var renderer = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
            var sprite = renderer != null ? renderer.sprite : null;
            var spritePath = sprite != null ? AssetDatabase.GetAssetPath(sprite) : null;
            if (prefab == null) report.Warn($"{entry.id}: 프리팹 없음 {prefabPath}");
            else if (sprite == null) report.Warn($"{entry.id}: SpriteRenderer 스프라이트 없음 {prefabPath}");
            else if (spritePath != entry.asepritePath) report.Warn($"{entry.id}: 스프라이트 출처 다름 {spritePath}");
            else report.Updated($"OK {entry.id}: {prefabPath} -> {sprite.name}");

            var importer = AssetImporter.GetAtPath(entry.asepritePath) as AsepriteImporter;
            if (importer == null) {
                report.Warn($"{entry.id}: aseprite 임포터 없음 {entry.asepritePath}");
                return;
            }

            var compression = importer.GetImporterPlatformSettings(BuildTarget.StandaloneWindows64).textureCompression;
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, Stage1ArtRules.PixelsPerUnit)
                || importer.filterMode != FilterMode.Point
                || compression != TextureImporterCompression.Uncompressed)
                report.Warn($"{entry.id}: 임포트 설정 불일치 (PPU {importer.spritePixelsPerUnit}, {importer.filterMode}, {compression})");
        }
    }
}
