using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public sealed class ToolNote {
        public ToolNote(int x, int y, string text) {
            X = x;
            Y = y;
            Text = text;
        }

        public int X { get; }
        public int Y { get; }
        public string Text { get; }
    }

    // 툴 원본 그대로. cells·darkMask는 행 우선, y=0이 맨 위.
    public sealed class ToolStage {
        public ToolStage(string id, string name, int width, int height, IReadOnlyList<string> cells,
            IReadOnlyList<int> darkMask, IReadOnlyList<ToolNote> notes, string memo, bool dark, string status) {
            Id = id;
            Name = name;
            Width = width;
            Height = height;
            Cells = cells;
            DarkMask = darkMask;
            Notes = notes;
            Memo = memo;
            Dark = dark;
            Status = status;
        }

        public string Id { get; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<string> Cells { get; }
        public IReadOnlyList<int> DarkMask { get; }
        public IReadOnlyList<ToolNote> Notes { get; }
        public string Memo { get; }
        public bool Dark { get; }
        public string Status { get; }
    }

    public sealed class CustomBlock {
        public CustomBlock(string id, string name, string color, string col) {
            Id = id;
            Name = name;
            Color = color;
            Col = col;
        }

        public string Id { get; }
        public string Name { get; }
        public string Color { get; }
        public string Col { get; }
    }

    public sealed class ToolSlot {
        public ToolSlot(IReadOnlyList<string> ids, string theme) {
            Ids = ids;
            Theme = theme;
        }

        public IReadOnlyList<string> Ids { get; }
        public string Theme { get; }
    }

    public sealed class CursorStageFile {
        public const string Format = "cursor-stage";
        public const int SupportedVersion = 3;

        public CursorStageFile(int version, IReadOnlyList<ToolStage> stages, IReadOnlyList<CustomBlock> customs,
            IReadOnlyList<ToolSlot> slots) {
            Version = version;
            Stages = stages;
            Customs = customs;
            Slots = slots;
        }

        public int Version { get; }
        public IReadOnlyList<ToolStage> Stages { get; }
        public IReadOnlyList<CustomBlock> Customs { get; }
        public IReadOnlyList<ToolSlot> Slots { get; }

        public ToolStage FindStage(string id) {
            foreach (var stage in Stages)
                if (stage.Id == id) return stage;
            return null;
        }

        public CustomBlock FindCustom(string id) {
            foreach (var custom in Customs)
                if (custom.Id == id) return custom;
            return null;
        }
    }
}
