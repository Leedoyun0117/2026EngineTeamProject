namespace LDY.StageImport.Core {
    public enum CustomOutcome {
        MappedByName,
        Fallback,
        Skipped,
        Unknown,

        // custom 대응표(CustomBlockMap) 결과
        CustomPrefab,
        CustomPlaceholder,
        CustomArea,

        // 대응표에서 타일로 대체(Key = ground/platform/spike)
        CustomTile
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

    // c_* 칸: custom 대응표에서 이름으로 먼저 찾고, 없으면 타일 대응표의 이름(또는 id), 그것도 없으면 col 값으로 기본 타일을 고른다.
    public static class CustomBlockResolver {
        public static CustomResolution Resolve(CustomBlock custom, IKeyRuleSource mapped) =>
            Resolve(custom, NoCustomBlocks.Instance, mapped);

        public static CustomResolution Resolve(CustomBlock custom, ICustomBlockSource customs, IKeyRuleSource mapped) {
            if (custom == null) return new CustomResolution(CustomOutcome.Unknown, null, default);
            if (!string.IsNullOrEmpty(custom.Name) && customs.TryFind(custom.Name, out var entry))
                return FromEntry(custom, entry);
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

        public static Placement PlacementOf(CustomBinding binding) {
            switch (binding) {
                case CustomBinding.Group4: return Placement.Group4;
                case CustomBinding.HorizontalRun: return Placement.HorizontalRun;
                case CustomBinding.Area: return Placement.Area;
                default: return Placement.Single;
            }
        }

        private static CustomResolution FromEntry(CustomBlock custom, CustomRule entry) {
            var tile = CustomRule.TileKey(entry.Tile);
            if (tile != null) return new CustomResolution(CustomOutcome.CustomTile, tile, new KeyRule(Placement.Tile));
            if (!entry.HasPrefab && entry.SkipIfEmpty) return new CustomResolution(CustomOutcome.Skipped, null, default);

            var key = CustomKey.Make(entry.EntryName, custom.Col);
            if (entry.Binding == CustomBinding.Area)
                return new CustomResolution(CustomOutcome.CustomArea, key, new KeyRule(Placement.Area, AreaKind.Named));
            var outcome = entry.HasPrefab ? CustomOutcome.CustomPrefab : CustomOutcome.CustomPlaceholder;
            return new CustomResolution(outcome, key, new KeyRule(PlacementOf(entry.Binding)));
        }

        private static CustomResolution Fallback(string key) =>
            new CustomResolution(CustomOutcome.Fallback, key, new KeyRule(Placement.Tile));
    }
}
