using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static LDY.Script.PlayerVisualContext;

namespace LDY.Script {
    internal static class PlayerVisualFixer {
        public static void Run() {
            var changes = new List<string>();
            if (TryBuildV2(out var set)) {
                AssignToPrefab(set, changes);
                RevertSceneOverrides(changes);
            }

            Log(changes.Count == 0 ? "변경 없음" : $"변경 {changes.Count}건");
            foreach (var change in changes)
                Log("변경: " + change);
        }

        private static bool TryBuildV2(out PlayerAnimSet set) {
            set = null;
            var files = AsepriteCatalog.Scan();
            foreach (var f in files)
                Log($"후보 {f.Path} importer={f.ImporterType} 스프라이트={f.SpriteCount} 클립={f.ClipNames.Length}");

            var candidates = files.Where(f => AsepriteCatalog.IsV2Candidate(f)).OrderByDescending(f => f.LastWrite).ToList();
            if (candidates.Count == 0) {
                var empty = files.Count > 0 && files.All(f => f.SpriteCount == 0);
                Log(empty
                    ? "중단: 모든 aseprite의 스프라이트가 0개입니다. 임포터가 동작하지 않거나 재임포트가 필요합니다 (Assets > Reimport)"
                    : "중단: 43프레임에 rope 태그를 가진 aseprite가 없습니다");
                return false;
            }

            var chosen = candidates[0];
            Log($"선택: {chosen.Path} (후보 {candidates.Count}개 중 수정 시각이 가장 최근인 것)");
            if (!PlayerAnimSetBuilder.BuildV2From(chosen.Path))
                return false;

            set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(PlayerAnimSetBuilder.SetV2Path);
            return set != null;
        }

        private static void AssignToPrefab(PlayerAnimSet set, List<string> changes) {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try {
                var pink = root.transform.Find(PinkName);
                var renderer = pink != null ? pink.GetComponent<PlayerRenderer>() : null;
                var sr = pink != null ? pink.GetComponent<SpriteRenderer>() : null;
                if (renderer == null || sr == null) {
                    Log($"중단: 프리팹에 {PinkName}의 PlayerRenderer/SpriteRenderer가 없음");
                    return;
                }

                var so = new SerializedObject(renderer);
                var property = so.FindProperty("animSet");
                if (property.objectReferenceValue != set) {
                    property.objectReferenceValue = set;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    changes.Add($"프리팹 {PinkName} PlayerRenderer.animSet → {set.name}");
                }

                var idle = set.FirstFrame(PlayerAnimId.Idle);
                if (idle != null && sr.sprite != idle) {
                    sr.sprite = idle;
                    changes.Add($"프리팹 {PinkName} SpriteRenderer.sprite → {idle.name}");
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // 세트와 스프라이트 오버라이드만 되돌린다. 위치 등 다른 오버라이드는 건드리지 않는다.
        private static void RevertSceneOverrides(List<string> changes) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            using var session = SceneSession.Open(ScenePath);
            var instance = FindInstance(session.Scene, prefab);
            var pink = instance != null ? instance.transform.Find(PinkName) : null;
            if (pink == null) {
                Log("씬 인스턴스에서 PinkVisual을 찾지 못해 오버라이드를 건드리지 않음");
                return;
            }

            var reverted = RevertIfOverridden(pink.GetComponent<PlayerRenderer>(), "animSet", changes);
            reverted |= RevertIfOverridden(pink.GetComponent<SpriteRenderer>(), "m_Sprite", changes);
            if (!reverted)
                return;

            changes.Add(session.SaveIfSafe()
                ? "SampleScene 저장"
                : "SampleScene에 저장되지 않은 기존 변경이 있어 저장하지 않음. 직접 저장하세요");
        }

        private static bool RevertIfOverridden(Object component, string propertyPath, List<string> changes) {
            if (component == null)
                return false;

            var property = new SerializedObject(component).FindProperty(propertyPath);
            if (property == null || !property.prefabOverride)
                return false;

            PrefabUtility.RevertPropertyOverride(property, InteractionMode.AutomatedAction);
            changes.Add($"씬 인스턴스 {component.GetType().Name}.{propertyPath} 오버라이드 되돌림");
            return true;
        }
    }
}
