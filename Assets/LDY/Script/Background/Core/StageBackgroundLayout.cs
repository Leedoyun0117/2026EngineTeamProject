using System.Collections.Generic;
using System.Linq;

namespace LDY.Background.Core {
    public enum TextAlign {
        Left,
        Center,
        Right
    }

    public sealed class TextSlotSpec {
        public string Id;
        public PixelRect Rect;
        public TextAlign Align;
        public Rgba Color;
        public Rgba? SelectedColor;
        public string DefaultText;
        public bool Mask;
    }

    public sealed class SheetAnimSpec {
        public string Id;
        public string File;
        public int Frames;
        public float FrameMs;
        public float CenterX;
        public float CenterY;
    }

    public sealed class SelectionBarSpec {
        public string Id;
        public PixelRect Rect;
        public Rgba Color;
        public float RowStep;
        public int DefaultRow;
        public IReadOnlyList<string> RowTextIds;
    }

    public sealed class CaretSpec {
        public string SheetId;
        public string TextId;
        public float GapPixels;
    }

    public sealed class ProgressSpec {
        public string Id;
        public PixelRect Track;
        public Rgba FillColor;
        public string ThumbFile;
        public float ThumbTop;
        public float ThumbMinX;
        public float ThumbMaxX;
    }

    public sealed class VisualizerSpec {
        public string Id;
        public PixelRect Rect;
        public int Bars;
        public float BarWidth;
        public float BarGap;
        public int Segments;
        public float SegmentHeight;
        public float SegmentStep;
        public string SegmentFile;
        public IReadOnlyList<Rgba> Ramp;
        public Rgba Off;
        public float FrameMs;
    }

    public sealed class NotesSpec {
        public string Id;
        public PixelRect Area;
        public string File;
        public IReadOnlyList<Rgba> Colors;
    }

    public sealed class StageBackgroundLayout {
        public StageBackgroundLayout(PixelSpace space, string staticFile, float staticX, float staticY,
            string referenceFile) {
            Space = space;
            StaticFile = staticFile;
            StaticX = staticX;
            StaticY = staticY;
            ReferenceFile = referenceFile;
        }

        public PixelSpace Space { get; }
        public string StaticFile { get; }
        public float StaticX { get; }
        public float StaticY { get; }
        public string ReferenceFile { get; }
        public List<TextSlotSpec> Texts { get; } = new List<TextSlotSpec>();
        public List<SheetAnimSpec> Sheets { get; } = new List<SheetAnimSpec>();
        public List<SelectionBarSpec> SelectionBars { get; } = new List<SelectionBarSpec>();
        public List<ProgressSpec> ProgressBars { get; } = new List<ProgressSpec>();
        public List<VisualizerSpec> Visualizers { get; } = new List<VisualizerSpec>();
        public List<NotesSpec> Notes { get; } = new List<NotesSpec>();
        public List<CaretSpec> Carets { get; } = new List<CaretSpec>();
        public List<string> UnsupportedWidgets { get; } = new List<string>();

        public IEnumerable<string> SingleSpriteFiles =>
            new[] { StaticFile, ReferenceFile }
                .Concat(ProgressBars.Select(p => p.ThumbFile))
                .Concat(Visualizers.Select(v => v.SegmentFile))
                .Concat(Notes.Select(n => n.File))
                .Where(f => f != null)
                .Distinct();

        public IEnumerable<string> RequiredFiles =>
            SingleSpriteFiles.Where(f => f != ReferenceFile).Concat(Sheets.Select(s => s.File)).Distinct();
    }
}
