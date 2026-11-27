using System.Collections.Generic;
using System.Text;

namespace LDY.StageImport.Core {
    // 같은 이름으로 볼 때 공백·물음표를 지우고 대소문자를 무시한다. "거미 ?" == "거미?" == "거미".
    public static class CustomNames {
        public static string Normalize(string name) {
            if (string.IsNullOrEmpty(name)) return "";
            var sb = new StringBuilder(name.Length);
            foreach (var c in name) {
                if (char.IsWhiteSpace(c) || c == '?' || c == '？') continue;
                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }
    }

    public enum CustomBinding {
        PerCell,
        Group4,
        HorizontalRun,

        // 4방향 묶음을 트리거 영역 오브젝트로 만든다(프리팹을 쓰지 않는다).
        Area
    }

    public enum CustomSizing {
        Stretch,
        RepeatPerCell,
        OnePerGroup,

        // 묶음당 하나, 가장 왼쪽 칸(같으면 아래 칸)에 원래 크기로.
        OneAtLeft,

        // 묶음당 하나, 맨 위 칸 윗변 가운데를 고정점으로 두고 그 열의 칸 수를 길이로(밧줄).
        HangFromTop
    }

    // 프리팹 대신 타일로 깐다. None이 아니면 묶는 방식·프리팹은 무시한다.
    public enum CustomTileReplace {
        None,
        Ground,
        Platform,
        Spike
    }

    public readonly struct CustomRule {
        public CustomRule(string entryName, CustomBinding binding, CustomSizing sizing, bool hasPrefab,
            CustomTileReplace tile = CustomTileReplace.None, bool skipIfEmpty = false) {
            EntryName = entryName;
            Binding = binding;
            Sizing = sizing;
            HasPrefab = hasPrefab;
            Tile = tile;
            SkipIfEmpty = skipIfEmpty;
        }

        // 대응표 항목의 대표 이름. 별칭으로 찾아도 이 이름이 키가 된다.
        public string EntryName { get; }
        public CustomBinding Binding { get; }
        public CustomSizing Sizing { get; }
        public bool HasPrefab { get; }
        public CustomTileReplace Tile { get; }

        // 프리팹·그림이 없을 때 분홍 블록 대신 건너뛴다(장식).
        public bool SkipIfEmpty { get; }

        public static string TileKey(CustomTileReplace tile) {
            switch (tile) {
                case CustomTileReplace.Ground: return ToolKeys.Ground;
                case CustomTileReplace.Platform: return ToolKeys.Platform;
                case CustomTileReplace.Spike: return ToolKeys.Spike;
                default: return null;
            }
        }
    }

    // 묶음당 하나 놓을 자리. Rect는 놓을 범위(가운데에 맞춘다).
    // HangFromTop이면 Rect는 맨 위 칸, 고정점은 그 칸 윗변 가운데(AnchorX, AnchorY), Length는 그 열의 칸 수(맨 위 칸 포함).
    public readonly struct GroupSpot {
        public GroupSpot(GridRect rect, int length) {
            Rect = rect;
            Length = length;
        }

        public GridRect Rect { get; }
        public int Length { get; }
        public float AnchorX => Rect.CenterX;
        public float AnchorY => Rect.Y + Rect.Height;

        public static GroupSpot Pick(CustomSizing sizing, IReadOnlyList<GridPoint> cells) {
            var bounds = Bounds(cells);
            switch (sizing) {
                case CustomSizing.OneAtLeft: {
                    var best = cells[0];
                    foreach (var p in cells)
                        if (p.X < best.X || (p.X == best.X && p.Y < best.Y))
                            best = p;
                    return new GroupSpot(GridRect.Cell(best), 0);
                }
                case CustomSizing.HangFromTop: {
                    var top = cells[0];
                    foreach (var p in cells)
                        if (p.Y > top.Y || (p.Y == top.Y && p.X < top.X))
                            top = p;
                    var length = 0;
                    foreach (var p in cells)
                        if (p.X == top.X && p.Y <= top.Y)
                            length++;
                    return new GroupSpot(GridRect.Cell(top), length);
                }
                default:
                    return new GroupSpot(bounds, 0);
            }
        }

        public static GridRect Bounds(IReadOnlyList<GridPoint> cells) {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in cells) {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            return new GridRect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }

    // custom 블록 이름으로 대응표 항목을 찾는 쪽.
    public interface ICustomBlockSource {
        bool TryFind(string customName, out CustomRule rule);
    }

    public sealed class NoCustomBlocks : ICustomBlockSource {
        public static readonly NoCustomBlocks Instance = new NoCustomBlocks();

        public bool TryFind(string customName, out CustomRule rule) {
            rule = default;
            return false;
        }
    }

    // 대표 이름·별칭을 정규화해 찾는다. 같은 정규화 이름이 두 항목에 있으면 먼저 넣은 쪽을 쓰고 Collisions에 남긴다.
    public sealed class CustomNameIndex : ICustomBlockSource {
        private readonly Dictionary<string, CustomRule> _byName = new Dictionary<string, CustomRule>();

        public List<string> Collisions { get; } = new List<string>();

        public void Add(CustomRule rule, IEnumerable<string> aliases) {
            Register(rule.EntryName, rule);
            if (aliases == null) return;
            foreach (var alias in aliases) Register(alias, rule);
        }

        public bool TryFind(string customName, out CustomRule rule) =>
            _byName.TryGetValue(CustomNames.Normalize(customName), out rule);

        private void Register(string name, CustomRule rule) {
            var key = CustomNames.Normalize(name);
            if (key.Length == 0) return;
            if (!_byName.TryGetValue(key, out var existing)) {
                _byName.Add(key, rule);
                return;
            }

            if (existing.EntryName != rule.EntryName)
                Collisions.Add($"'{name}': '{existing.EntryName}'와 '{rule.EntryName}'에 모두 있음 → '{existing.EntryName}' 사용");
        }
    }

    // 계획에서 custom 블록을 가리키는 키. 같은 이름이라도 col이 다르면 충돌 처리가 달라서 col까지 넣는다.
    public static class CustomKey {
        public const string Prefix = "custom:";
        private const char Separator = '|';

        public static string Make(string entryName, string col) => Prefix + entryName + Separator + (col ?? "");

        public static bool Is(string key) => key != null && key.StartsWith(Prefix);

        public static bool TryParse(string key, out string entryName, out string col) {
            entryName = null;
            col = null;
            if (!Is(key)) return false;
            var bar = key.LastIndexOf(Separator);
            if (bar < Prefix.Length) return false;
            entryName = key.Substring(Prefix.Length, bar - Prefix.Length);
            col = key.Substring(bar + 1);
            return true;
        }
    }

    public sealed class ColConflict {
        public ColConflict(string entryName, IReadOnlyList<string> cols, IReadOnlyList<string> customIds) {
            EntryName = entryName;
            Cols = cols;
            CustomIds = customIds;
        }

        public string EntryName { get; }
        public IReadOnlyList<string> Cols { get; }
        public IReadOnlyList<string> CustomIds { get; }
    }

    // 같은 이름(대응표 항목)인데 col이 다른 custom 정의를 찾는다. 둘 다 같은 프리팹으로 가고, 임시 블록이면 col대로 충돌한다.
    public static class CustomColConflicts {
        public static List<ColConflict> Find(IEnumerable<CustomBlock> customs, ICustomBlockSource source) {
            var cols = new Dictionary<string, List<string>>();
            var ids = new Dictionary<string, List<string>>();
            var order = new List<string>();
            foreach (var custom in customs) {
                var name = source.TryFind(custom.Name, out var rule) ? rule.EntryName : custom.Name;
                if (!cols.TryGetValue(name, out var list)) {
                    list = new List<string>();
                    cols.Add(name, list);
                    ids.Add(name, new List<string>());
                    order.Add(name);
                }

                if (!list.Contains(custom.Col)) list.Add(custom.Col);
                ids[name].Add(custom.Id);
            }

            var result = new List<ColConflict>();
            foreach (var name in order)
                if (cols[name].Count > 1)
                    result.Add(new ColConflict(name, cols[name], ids[name]));
            return result;
        }
    }
}
