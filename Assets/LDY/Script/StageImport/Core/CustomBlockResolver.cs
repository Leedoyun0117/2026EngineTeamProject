namespace LDY.StageImport.Core {
    public enum CustomOutcome {
        MappedByName,
        Fallback,
        Skipped,
        Unknown
    }

    public readonly struct CustomResolution {
        public CustomResolution(CustomOutcome outcome, string key, KeyRule rule) {
            Outcome = outcome;
            Key = key;
            Rule = rule;
        }

        public CustomOutcome Outcome { get; }
        public string Key { get; }
        public KeyRule Rule { get; }
    }

    // c_* 칸: 대응표에 이름(또는 id)이 있으면 그것, 없으면 col 값으로 기본 타일을 고른다.
    public static class CustomBlockResolver {
        public static CustomResolution Resolve(CustomBlock custom, IKeyRuleSource mapped) {
            if (custom == null) return new CustomResolution(CustomOutcome.Unknown, null, default);
            if (!string.IsNullOrEmpty(custom.Name) && mapped.TryGet(custom.Name, out var byName))
                return new CustomResolution(CustomOutcome.MappedByName, custom.Name, byName);
            if (mapped.TryGet(custom.Id, out var byId))
                return new CustomResolution(CustomOutcome.MappedByName, custom.Id, byId);

            switch (custom.Col) {
                case "solid": return Fallback(ToolKeys.Ground);
                case "oneway": return Fallback(ToolKeys.Platform);
                case "hazard": return Fallback(ToolKeys.Spike);
                case "none": return new CustomResolution(CustomOutcome.Skipped, null, default);
                default: return new CustomResolution(CustomOutcome.Unknown, null, default);
            }
        }

        private static CustomResolution Fallback(string key) =>
            new CustomResolution(CustomOutcome.Fallback, key, new KeyRule(Placement.Tile));
    }
}
