using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.SkillTargets.Core;
using UnityEditor;

namespace LDY.SkillTargetsEditor {
    // 규칙 표(SO)를 Core가 쓰는 요청 목록으로 바꾼다. 비어 있는 기획 대상과 모르는 능력 id는 issues로 모은다.
    internal static class RequestBuilder {
        private static readonly string[] SearchFolders = { "Assets/LDY", "Assets/Main", "Assets/HES", "Assets/JJB", "Assets/HTY" };

        public static List<TargetRequest> Build(SkillTargetRules rules, List<string> issues) {
            var abilities = rules.Abilities.Where(a => !string.IsNullOrEmpty(a.id))
                .GroupBy(a => a.id).ToDictionary(g => g.Key, g => ToRequirement(g.First()));
            var allPrefabs = new List<string>();
            var requests = new List<TargetRequest>();

            foreach (var target in rules.Targets) {
                var resolved = ResolveAbilities(target, abilities, issues);
                var paths = PrefabPathsOf(target, allPrefabs);
                if (paths.Count == 0) issues.Add($"비어 있음: {target.label} ({string.Join(", ", target.abilities)})");
                var material = target.material != null ? AssetDatabase.GetAssetPath(target.material) : null;
                foreach (var path in paths)
                    requests.Add(new TargetRequest(path, target.child, target.label, resolved,
                        target.rigidbodyIfMissing, material, target.layer));
            }

            return requests;
        }

        private static List<AbilityRequirement> ResolveAbilities(SkillTargetRules.Target target,
            IReadOnlyDictionary<string, AbilityRequirement> abilities, List<string> issues) {
            var resolved = new List<AbilityRequirement>();
            foreach (var id in target.abilities) {
                if (abilities.TryGetValue(id, out var ability)) resolved.Add(ability);
                else issues.Add($"모르는 능력 id '{id}' ({target.label})");
            }

            return resolved;
        }

        private static List<string> PrefabPathsOf(SkillTargetRules.Target target, List<string> allPrefabs) {
            var paths = target.prefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath).ToList();
            if (string.IsNullOrWhiteSpace(target.namePattern)) return paths.Distinct().ToList();

            if (allPrefabs.Count == 0)
                allPrefabs.AddRange(AssetDatabase.FindAssets("t:Prefab", SearchFolders).Select(AssetDatabase.GUIDToAssetPath));
            paths.AddRange(allPrefabs.Where(p => NamePattern.Matches(target.namePattern, Path.GetFileNameWithoutExtension(p))));
            return paths.Distinct().ToList();
        }

        private static AbilityRequirement ToRequirement(SkillTargetRules.Ability ability) {
            var components = ability.components.Where(c => !string.IsNullOrEmpty(c.typeName))
                .Select(c => new ComponentRequirement(c.typeName,
                    c.fields.Where(f => !string.IsNullOrEmpty(f.field)).ToDictionary(f => f.field, f => f.value ?? "")))
                .ToList();
            return new AbilityRequirement(ability.id, ability.myCan, ability.selectorUsesMyCan, ability.needsRigidbody, components);
        }
    }
}
