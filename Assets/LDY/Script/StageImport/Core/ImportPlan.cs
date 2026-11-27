using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public readonly struct KeyedCell {
        public KeyedCell(string key, GridPoint cell) {
            Key = key;
            Cell = cell;
        }

        public string Key { get; }
        public GridPoint Cell { get; }
    }

    public readonly struct KeyedRect {
        public KeyedRect(string key, GridRect rect) {
            Key = key;
            Rect = rect;
        }

        public string Key { get; }
        public GridRect Rect { get; }
    }

    public sealed class KeyedGroup {
        public KeyedGroup(string key, IReadOnlyList<GridPoint> cells) {
            Key = key;
            Cells = cells;
        }

        public string Key { get; }
        public IReadOnlyList<GridPoint> Cells { get; }
    }

    public sealed class AreaRegion {
        public AreaRegion(AreaKind kind, IReadOnlyList<GridPoint> cells, IReadOnlyList<GridRect> rects,
            string key = null) {
            Kind = kind;
            Cells = cells;
            Rects = rects;
            Key = key;
        }

        public AreaKind Kind { get; }

        // Named 영역의 계획 키(CustomKey). 어둠·탭 영역은 null.
        public string Key { get; }
        public IReadOnlyList<GridPoint> Cells { get; }
        public IReadOnlyList<GridRect> Rects { get; }
    }

    public readonly struct PlacedNote {
        public PlacedNote(GridPoint cell, string text) {
            Cell = cell;
            Text = text;
        }

        public GridPoint Cell { get; }
        public string Text { get; }
    }

    public sealed class CustomUse {
        public CustomUse(string id, string name, string col, CustomOutcome outcome, string key,
            KeyRule rule = default) {
            Id = id;
            Name = name;
            Col = col;
            Outcome = outcome;
            Key = key;
            Rule = rule;
        }

        public KeyRule Rule { get; }

        public string Id { get; }
        public string Name { get; }
        public string Col { get; }
        public CustomOutcome Outcome { get; }
        public string Key { get; }
        public int Count { get; set; }
    }

    // 한 맵을 Unity 좌표의 배치 목록으로 바꾼 결과. 에셋은 모른다.
    public sealed class ImportPlan {
        public ImportPlan(ToolStage source) {
            Source = source;
        }

        public ToolStage Source { get; }
        public int Width => Source.Width;
        public int Height => Source.Height;

        public List<KeyedCell> Tiles { get; } = new List<KeyedCell>();
        public List<KeyedCell> Decorations { get; } = new List<KeyedCell>();
        public List<KeyedRect> Prefabs { get; } = new List<KeyedRect>();
        public List<KeyedGroup> Groups { get; } = new List<KeyedGroup>();
        public List<GridPoint> Starts { get; } = new List<GridPoint>();
        public List<GridPoint> Goals { get; } = new List<GridPoint>();
        public List<AreaRegion> Areas { get; } = new List<AreaRegion>();
        public List<PlacedNote> Notes { get; } = new List<PlacedNote>();
        public SortedDictionary<string, int> KeyCounts { get; } = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> Unmapped { get; } = new SortedDictionary<string, int>();
        public List<CustomUse> Customs { get; } = new List<CustomUse>();

        public int BlockCount {
            get {
                var total = 0;
                foreach (var count in KeyCounts.Values) total += count;
                return total;
            }
        }
    }
}
