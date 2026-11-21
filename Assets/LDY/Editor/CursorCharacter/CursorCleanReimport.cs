using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // 오염된 .meta를 버리고 aseprite 파일을 새로 임포트한 뒤, 세트 빌드, 연결, 덤프까지 이어서 실행한다.
    internal static class CursorCleanReimport {
        private const string BackupPath = "Temp/player_anim_cursor_backup.aseprite";
        private const string ExpectedLayerName = "player_anim_cursor";
        private const int ExpectedSpriteCount = 44;

        private static readonly Regex FrameName = new(@"^Frame_(\d+)$");

        public static bool Run(List<string> report) {
            if (!CursorAssetLocator.TryFind(CursorAssetLocator.CursorFile, out var path, report))
                return false;

            var state = Inspect(path);
            if (state.Problems.Count == 0) {
                report.Add($"{path} 의 .meta 가 이미 깨끗함(레이어 '{state.LayerName}', 스프라이트 {state.Sprites.Count}개). 재임포트를 건너뜀");
            }
            else {
                report.Add($"{path} 의 .meta 상태: {string.Join("; ", state.Problems)}");
                if (!Reimport(path, report))
                    return false;
            }

            return CursorImportMatcher.MatchByName(report)
                   && Verify(path, report)
                   && Step("2. Build Anim Set", PlayerAnimSetBuilder.BuildCursor, report)
                   && Step("3. Assign To Players", () => CursorPlayerAssigner.Run(report), report)
                   && Step("Dump Clips", () => CursorClipDumper.Run(report), report);
        }

        private static bool Reimport(string path, List<string> report) {
            var original = File.ReadAllBytes(path);
            WriteBackup(original);
            report.Add($"백업: {BackupPath} ({original.Length} bytes)");

            if (!AssetDatabase.DeleteAsset(path)) {
                report.Add($"중단: {path} 를 지우지 못함. 백업은 {BackupPath} 에 남아 있음");
                return false;
            }

            File.WriteAllBytes(path, original);
            if (!File.ReadAllBytes(path).SequenceEqual(original)) {
                report.Add($"중단: 다시 쓴 파일이 원본과 다름. 백업은 {BackupPath} 에 남아 있음");
                return false;
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null) {
                report.Add($"중단: 새로 임포트하지 못함. 파일은 복구되어 있고 백업은 {BackupPath} 에 남아 있음");
                return false;
            }

            File.Delete(BackupPath);
            report.Add($"{path} 와 .meta 를 지우고 새로 임포트함(임포터 {importer.GetType().Name}). 백업을 지움");
            return true;
        }

        private static void WriteBackup(byte[] bytes) {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupPath));
            File.WriteAllBytes(BackupPath, bytes);
            if (!File.ReadAllBytes(BackupPath).SequenceEqual(bytes))
                throw new IOException($"백업 검증 실패: {BackupPath}");
        }

        private static bool Verify(string path, List<string> report) {
            var state = Inspect(path);
            report.Add($"새 .meta: 레이어 이름 '{state.LayerName}', 스프라이트 {state.Sprites.Count}개");
            if (state.Problems.Count == 0)
                return true;

            report.Add($"중단: {string.Join("; ", state.Problems)}");
            report.Add($"실제 스프라이트 이름: {string.Join(", ", state.Sprites)}");
            return false;
        }

        private static ImportState Inspect(string path) {
            var layerName = ReadLayerName(path);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Select(s => s.name).ToList();
            var problems = new List<string>();
            if (layerName != ExpectedLayerName)
                problems.Add($"레이어 이름이 '{layerName}' (기대값 {ExpectedLayerName})");
            if (sprites.Count != ExpectedSpriteCount)
                problems.Add($"스프라이트가 {sprites.Count}개 (기대값 {ExpectedSpriteCount})");

            var numbers = sprites.Select(NumberOf).ToList();
            if (numbers.Any(n => n < 0 || n >= ExpectedSpriteCount) || numbers.Distinct().Count() != numbers.Count)
                problems.Add("스프라이트 이름이 Frame_0~Frame_43 이 아님");

            return new ImportState(layerName, sprites, problems);
        }

        private static string ReadLayerName(string path) {
            var importer = AssetImporter.GetAtPath(path);
            var layers = importer != null ? new SerializedObject(importer).FindProperty("asepriteLayers") : null;
            if (layers == null || layers.arraySize == 0)
                return "(레이어 없음)";

            var name = layers.GetArrayElementAtIndex(0).FindPropertyRelative("name");
            return name != null ? name.stringValue : "(name 속성 없음)";
        }

        private static int NumberOf(string spriteName) {
            var match = FrameName.Match(spriteName);
            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
        }

        private static bool Step(string label, Func<bool> action, List<string> report) {
            var ok = action();
            report.Add($"{label}: {(ok ? "완료" : "실패")}");
            return ok;
        }

        private readonly struct ImportState {
            public readonly string LayerName;
            public readonly List<string> Sprites;
            public readonly List<string> Problems;

            public ImportState(string layerName, List<string> sprites, List<string> problems) {
                LayerName = layerName;
                Sprites = sprites;
                Problems = problems;
            }
        }
    }
}
