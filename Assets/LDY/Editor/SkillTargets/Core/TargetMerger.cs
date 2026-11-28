using System.Collections.Generic;
using System.Linq;

namespace LDY.SkillTargets.Core {
    // 같은 프리팹·같은 대상 오브젝트를 가리키는 요청을 하나로 합친다.
    public static class TargetMerger {
        public static IReadOnlyList<MergedTarget> Merge(IEnumerable<TargetRequest> requests) {
            return requests
                .GroupBy(r => (r.PrefabPath, r.Child))
                .Select(g => MergeGroup(g.ToList()))
                .ToList();
        }

        private static MergedTarget MergeGroup(IReadOnlyList<TargetRequest> group) {
            var target = new MergedTarget(group[0].PrefabPath, group[0].Child);
            foreach (var label in group.Select(r => r.Label).Where(l => !string.IsNullOrEmpty(l)).Distinct())
                target.Labels.Add(label);

            var abilities = group.SelectMany(r => r.Abilities).GroupBy(a => a.Id).Select(g => g.First()).ToList();
            target.AbilityIds.AddRange(abilities.Select(a => a.Id));
            target.NeedsRigidbody = abilities.Any(a => a.NeedsRigidbody);
            ResolveMyCan(target, abilities);
            MergeComponents(target, abilities);
            target.Body = Single(target, group.Select(r => r.Body), BodyPolicy.Skip, "Rigidbody2D 처리");
            target.MaterialPath = Single(target, group.Select(r => r.MaterialPath).Where(m => m != null), null, "머티리얼");
            target.Layer = Single(target, group.Select(r => r.Layer).Where(l => l >= 0), -1, "레이어");
            return target;
        }

        private static void ResolveMyCan(MergedTarget target, IReadOnlyList<AbilityRequirement> abilities) {
            var withValue = abilities.Where(a => !string.IsNullOrEmpty(a.MyCan)).ToList();
            var selected = withValue.Where(a => a.SelectorUsesMyCan).ToList();
            var candidates = (selected.Count > 0 ? selected : withValue).Select(a => a.MyCan).Distinct().ToList();
            target.MyCanConflicts.AddRange(candidates);
            if (candidates.Count == 1) target.MyCan = candidates[0];
        }

        private static void MergeComponents(MergedTarget target, IReadOnlyList<AbilityRequirement> abilities) {
            var order = new List<string>();
            var fields = new Dictionary<string, Dictionary<string, string>>();
            foreach (var component in abilities.SelectMany(a => a.Components)) {
                if (!fields.TryGetValue(component.TypeName, out var merged)) {
                    merged = new Dictionary<string, string>();
                    fields[component.TypeName] = merged;
                    order.Add(component.TypeName);
                }

                foreach (var pair in component.Fields) {
                    if (merged.TryGetValue(pair.Key, out var existing) && !FieldValues.Same(existing, pair.Value)) {
                        target.Warnings.Add($"{component.TypeName}.{pair.Key} 값 충돌({existing} / {pair.Value}) → {existing} 유지");
                        continue;
                    }

                    merged[pair.Key] = pair.Value;
                }
            }

            foreach (var type in order) target.Components.Add(new ComponentRequirement(type, fields[type]));
        }

        private static T Single<T>(MergedTarget target, IEnumerable<T> values, T onConflict, string what) {
            var distinct = values.Distinct().ToList();
            if (distinct.Count == 0) return onConflict;
            if (distinct.Count == 1) return distinct[0];
            target.Warnings.Add($"{what} 규칙 충돌({string.Join(", ", distinct)}) → 건드리지 않음");
            return onConflict;
        }
    }
}
