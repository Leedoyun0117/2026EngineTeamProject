using System.Collections.Generic;

namespace LDY.SkillTargets.Core {
    // 합친 규칙과 현재 상태를 비교해 할 일을 계산한다. 있는 컴포넌트는 다시 붙이지 않고 다른 값만 맞춘다.
    public static class PlanCalculator {
        public static TargetPlan Calculate(MergedTarget target, TargetSnapshot snapshot) {
            var steps = new List<PlanStep>();
            foreach (var warning in target.Warnings) steps.Add(PlanStep.Warning(warning));

            if (!snapshot.Found) {
                steps.Add(PlanStep.Warning($"대상 오브젝트 '{target.Child}' 없음"));
                return new TargetPlan(target, steps);
            }

            AddRigidbodyStep(target, snapshot, steps);
            foreach (var component in target.Components) AddComponentSteps(component, snapshot, steps);
            AddMyCanSteps(target, snapshot, steps);

            if (target.MaterialPath != null && target.MaterialPath != snapshot.MaterialPath)
                steps.Add(PlanStep.SetMaterial(snapshot.MaterialPath, target.MaterialPath));
            if (target.Layer >= 0 && target.Layer != snapshot.Layer)
                steps.Add(PlanStep.SetLayer(snapshot.Layer, target.Layer));

            return new TargetPlan(target, steps);
        }

        private static void AddRigidbodyStep(MergedTarget target, TargetSnapshot snapshot, List<PlanStep> steps) {
            if (!target.NeedsRigidbody || snapshot.HasRigidbody) return;
            steps.Add(target.Body == BodyPolicy.Skip
                ? PlanStep.Warning("Rigidbody2D 없음, 규칙이 Skip이라 추가 안 함 (정지 효과 없음)")
                : PlanStep.AddRigidbody(target.Body));
        }

        private static void AddMyCanSteps(MergedTarget target, TargetSnapshot snapshot, List<PlanStep> steps) {
            if (target.HasMyCanConflict) {
                steps.Add(PlanStep.Conflict($"MyCan {string.Join(" + ", target.MyCanConflicts)} 동시 지정 불가 → Object_HTY 비워 둠"));
                return;
            }

            if (target.MyCan == null) return;
            var fields = new Dictionary<string, string> { [HtyContract.MyCanField] = target.MyCan };
            AddComponentSteps(new ComponentRequirement(HtyContract.ObjectType, fields), snapshot, steps);

            if (!snapshot.HasCollider) steps.Add(PlanStep.Warning("Collider2D 없음 → 스킬 탐색에 안 잡힘"));
            if (!snapshot.HasSpriteRenderer) steps.Add(PlanStep.Warning("SpriteRenderer 없음 → 선택 표시 때 오류"));
        }

        private static void AddComponentSteps(ComponentRequirement component, TargetSnapshot snapshot, List<PlanStep> steps) {
            if (!snapshot.Components.TryGetValue(component.TypeName, out var current)) {
                steps.Add(PlanStep.AddComponent(component.TypeName));
                foreach (var pair in component.Fields)
                    steps.Add(PlanStep.SetField(component.TypeName, pair.Key, null, pair.Value));
                return;
            }

            foreach (var pair in component.Fields) {
                var from = current.TryGetValue(pair.Key, out var value) ? value : FieldValues.Missing;
                if (!FieldValues.Same(from, pair.Value))
                    steps.Add(PlanStep.SetField(component.TypeName, pair.Key, from, pair.Value));
            }
        }
    }
}
