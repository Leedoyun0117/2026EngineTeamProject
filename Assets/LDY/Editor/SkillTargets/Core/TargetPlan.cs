using System.Collections.Generic;
using System.Linq;

namespace LDY.SkillTargets.Core {
    public sealed class TargetPlan {
        public TargetPlan(MergedTarget target, IReadOnlyList<PlanStep> steps) {
            Target = target;
            Steps = steps;
        }

        public MergedTarget Target { get; }
        public IReadOnlyList<PlanStep> Steps { get; }
        public bool HasChanges => Steps.Any(s => s.IsChange);
        public bool HasIssues => Steps.Any(s => !s.IsChange);
    }
}
