using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public enum Placement {
        Tile,
        Single,
        HorizontalRun,
        Group4,
        Start,
        Goal,
        Area,
        Decoration
    }

    public enum AreaKind {
        Dark,
        Tab,

        // custom 대응표에서 영역으로 지정한 이름. AreaRegion.Key에 어떤 이름인지 들어 있다.
        Named
    }

    public readonly struct KeyRule {
        public KeyRule(Placement placement, AreaKind area = AreaKind.Dark) {
            Placement = placement;
            Area = area;
        }

        public Placement Placement { get; }
        public AreaKind Area { get; }
    }

    // 툴에 없는 키를 대응표에 추가했을 때 묶는 방식을 알려 주는 쪽.
    public interface IKeyRuleSource {
        bool TryGet(string key, out KeyRule rule);
    }

    public sealed class NoExtraRules : IKeyRuleSource {
        public static readonly NoExtraRules Instance = new NoExtraRules();

        public bool TryGet(string key, out KeyRule rule) {
            rule = default;
            return false;
        }
    }

    public static class ToolKeys {
        public const string Empty = "";
        public const string Ground = "ground";
        public const string Platform = "platform";
        public const string Spike = "spike";
        public const string Start = "start";
        public const string Goal = "goal";
        public const string GhostHidden = "ghost_on";
        public const string CustomPrefix = "c_";
        public const string AbsorbPrefix = "abs_";
        public const string DecorationPrefix = "deco_";

        public static readonly IReadOnlyList<string> Abilities = new[] {
            "resize", "copy", "tab", "drag", "pause", "alpha", "zoom", "light", "cut"
        };

        public static readonly IReadOnlyList<string> Decorations = new[] {
            "deco_back", "deco_cloud", "deco_bush", "deco_star"
        };

        public static bool IsCustom(string key) => key.StartsWith(CustomPrefix);
        public static string AbilityOf(string key) => key.StartsWith(AbsorbPrefix) ? key.Substring(AbsorbPrefix.Length) : null;
    }

    public static class DefaultKeyRules {
        private static readonly Dictionary<string, KeyRule> Rules = Build();

        public static IEnumerable<string> Keys => Rules.Keys;

        public static bool TryGet(string key, out KeyRule rule) => Rules.TryGetValue(key, out rule);

        private static Dictionary<string, KeyRule> Build() {
            var rules = new Dictionary<string, KeyRule> {
                { ToolKeys.Ground, new KeyRule(Placement.Tile) },
                { ToolKeys.Platform, new KeyRule(Placement.Tile) },
                { ToolKeys.Spike, new KeyRule(Placement.Tile) },
                { ToolKeys.Start, new KeyRule(Placement.Start) },
                { ToolKeys.Goal, new KeyRule(Placement.Goal) },
                { "box", new KeyRule(Placement.Single) },
                { "cut", new KeyRule(Placement.Single) },
                { "mov_h", new KeyRule(Placement.HorizontalRun) },
                { "mov_v", new KeyRule(Placement.HorizontalRun) },
                { "ghost", new KeyRule(Placement.Group4) },
                { ToolKeys.GhostHidden, new KeyRule(Placement.Group4) },
                { "tabzone", new KeyRule(Placement.Area, AreaKind.Tab) }
            };
            foreach (var ability in ToolKeys.Abilities)
                rules.Add(ToolKeys.AbsorbPrefix + ability, new KeyRule(Placement.Single));
            foreach (var deco in ToolKeys.Decorations) rules.Add(deco, new KeyRule(Placement.Decoration));
            return rules;
        }
    }
}
