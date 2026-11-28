using System.Linq;
using System.Text;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // 스테이지 임포터 대응표(CustomBlockMap, StageTileMap_World*)의 프리팹을 규칙과 비교한다. 읽기만 한다.
    internal static class ImporterMapCheck {
        private const string Folder = "Assets/LDY/Data/StageImport";

        public static void AppendTo(StringBuilder sb, PlanSet set) {
            var table = new MarkdownTable("대응표", "키", "프리팹", "주인", "규칙", "빠진 것/다른 값", "주의");
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { Folder })) {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) AddEntries(asset, set, table);
            }

            sb.AppendLine("\n### 스테이지 임포터 대응표에 연결된 프리팹");
            table.AppendTo(sb);
        }

        private static void AddEntries(ScriptableObject asset, PlanSet set, MarkdownTable table) {
            var entries = new SerializedObject(asset).FindProperty("entries");
            if (entries == null || !entries.isArray) return;

            for (var i = 0; i < entries.arraySize; i++) {
                var entry = entries.GetArrayElementAtIndex(i);
                if (!(entry.FindPropertyRelative("prefab")?.objectReferenceValue is GameObject prefab)) continue;

                var key = entry.FindPropertyRelative("name")?.stringValue ?? entry.FindPropertyRelative("key")?.stringValue;
                var path = AssetDatabase.GetAssetPath(prefab);
                var owner = DryRunReport.OwnerName(FolderOwners.Resolve(path));
                var plan = set.Prefabs.FirstOrDefault(p => p.Path == path);
                if (plan == null) {
                    table.Add(asset.name, key, prefab.name, owner, "규칙 없음", "", "");
                    continue;
                }

                foreach (var target in plan.Targets)
                    table.Add(asset.name, key, prefab.name, owner, string.Join("+", target.Target.AbilityIds),
                        DryRunReport.Changes(target), DryRunReport.Issues(target));
            }
        }
    }
}
