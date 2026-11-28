namespace LDY.SkillTargets.Core {
    public enum StepKind { AddRigidbody, AddComponent, SetField, SetMaterial, SetLayer, Conflict, Warning }

    public sealed class PlanStep {
        private PlanStep(StepKind kind, string typeName, string field, string from, string to, string message) {
            Kind = kind;
            TypeName = typeName;
            Field = field;
            From = from;
            To = to;
            Message = message;
        }

        public StepKind Kind { get; }
        public string TypeName { get; }
        public string Field { get; }
        public string From { get; }
        public string To { get; }
        public string Message { get; }
        public bool IsChange => Kind <= StepKind.SetLayer;

        public static PlanStep AddRigidbody(BodyPolicy body) =>
            new PlanStep(StepKind.AddRigidbody, HtyContract.Rigidbody, null, null, body.ToString(), null);

        public static PlanStep AddComponent(string type) => new PlanStep(StepKind.AddComponent, type, null, null, null, null);

        public static PlanStep SetField(string type, string field, string from, string to) =>
            new PlanStep(StepKind.SetField, type, field, from, to, null);

        public static PlanStep SetMaterial(string from, string to) =>
            new PlanStep(StepKind.SetMaterial, null, null, from, to, null);

        public static PlanStep SetLayer(int from, int to) =>
            new PlanStep(StepKind.SetLayer, null, null, from.ToString(), to.ToString(), null);

        public static PlanStep Conflict(string message) => new PlanStep(StepKind.Conflict, null, null, null, null, message);
        public static PlanStep Warning(string message) => new PlanStep(StepKind.Warning, null, null, null, null, message);

        public string Describe() => Kind switch {
            StepKind.AddRigidbody => $"+ Rigidbody2D ({To})",
            StepKind.AddComponent => $"+ {TypeName}",
            StepKind.SetField => $"{TypeName}.{Field}: {From ?? "(새로)"} → {To}",
            StepKind.SetMaterial => $"머티리얼: {From ?? "(없음)"} → {To}",
            StepKind.SetLayer => $"레이어: {From} → {To}",
            StepKind.Conflict => $"겹침: {Message}",
            _ => $"주의: {Message}"
        };
    }
}
