using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    public static class CursorCharacterMenu {
        private const string Root = "LDY/Cursor Character/";

        [MenuItem(Root + "0. Clean Reimport")]
        private static void CleanReimport() {
            Report(CursorCleanReimport.Run);
        }

        [MenuItem(Root + "1. Match Import Settings")]
        private static void MatchImport() {
            Report(MatchStep);
        }

        [MenuItem(Root + "2. Build Anim Set")]
        private static void BuildSet() {
            Report(BuildStep);
        }

        [MenuItem(Root + "3. Assign To Players")]
        private static void Assign() {
            Report(CursorPlayerAssigner.Run);
        }

        [MenuItem(Root + "4. Update Rope Grip Profile")]
        private static void UpdateGrip() {
            Report(CursorGripProfileUpdater.Run);
        }

        [MenuItem(Root + "Dump Clips")]
        private static void DumpClips() {
            Report(CursorClipDumper.Run);
        }

        [MenuItem(Root + "Run All (1-4)")]
        private static void RunAll() {
            Report(report => MatchStep(report) && BuildStep(report) && CursorPlayerAssigner.Run(report)
                             && CursorGripProfileUpdater.Run(report));
        }

        private static bool MatchStep(List<string> report) {
            return CursorImportMatcher.MatchByName(report);
        }

        private static bool BuildStep(List<string> report) {
            var built = PlayerAnimSetBuilder.BuildCursor();
            report.Add(built ? $"{PlayerAnimSetBuilder.CursorSetPath} 생성/갱신" : "세트 생성 실패. Console의 에러를 확인하세요");
            return built;
        }

        private static void Report(System.Func<List<string>, bool> step) {
            var report = new List<string>();
            var ok = step(report);
            AssetDatabase.SaveAssets();
            var message = "[CursorCharacter] " + (ok ? "완료" : "중단") + "\n- " + string.Join("\n- ", report);
            if (ok)
                Debug.Log(message);
            else
                Debug.LogError(message);
        }
    }
}
