using System;
using System.Collections.Generic;
using System.Linq;
using LDY.Background.Core;

namespace LDY.Background {
    internal sealed class WidgetNormalizer {
        private const float DefaultFrameMs = 120f;
        private const string SelectionBarSuffix = "_selection_bar";
        private const string ProgressTrack = "progress_track";
        private const string ProgressThumb = "progress_thumb";

        private readonly string _folder;
        private readonly StageBackgroundLayout _layout;
        private readonly Dictionary<string, Action<string, JsonNode, JsonNode>> _byKey;

        public WidgetNormalizer(string folder, StageBackgroundLayout layout) {
            _folder = folder;
            _layout = layout;
            _byKey = new Dictionary<string, Action<string, JsonNode, JsonNode>> {
                { ProgressTrack, ReadProgress },
                { ProgressThumb, (key, node, all) => RequireSibling(all, ProgressTrack, node) },
                { "visualizer", ReadVisualizer },
                { "notes_spawn", ReadNotes }
            };
        }

        public void Read(JsonNode widgets) {
            foreach (var key in widgets.Keys) {
                var node = widgets.Child(key);
                if (_byKey.TryGetValue(key, out var read)) read(key, node, widgets);
                else if (key.EndsWith(SelectionBarSuffix)) ReadSelectionBar(key, node);
                else if (node.Has("sheet")) ReadSheet(key, node);
                else _layout.UnsupportedWidgets.Add(key);
            }
        }

        private void ReadSelectionBar(string key, JsonNode node) {
            _layout.SelectionBars.Add(new SelectionBarSpec {
                Id = key,
                Rect = node.Rect("rect"),
                Color = node.Color("color"),
                RowStep = node.Number("row_step"),
                DefaultRow = (int)node.Number("default_row", 0f),
                RowTextIds = _layout.Texts.Where(t => t.SelectedColor.HasValue).Select(t => t.Id).ToList()
            });
        }

        private void ReadProgress(string key, JsonNode track, JsonNode all) {
            var thumb = all.Child(ProgressThumb);
            var range = thumb.Numbers("x_range", 2);
            _layout.ProgressBars.Add(new ProgressSpec {
                Id = "progress",
                Track = track.Rect("rect"),
                FillColor = track.Color("fill_color"),
                ThumbFile = AssetPath(thumb.String("file")),
                ThumbTop = thumb.Number("y"),
                ThumbMinX = range[0],
                ThumbMaxX = range[1]
            });
        }

        private void ReadSheet(string key, JsonNode node) {
            var center = node.Numbers("center", 2);
            _layout.Sheets.Add(new SheetAnimSpec {
                Id = key,
                File = AssetPath(node.String("sheet")),
                Frames = (int)node.Number("frames"),
                FrameMs = node.Number("frame_ms", DefaultFrameMs),
                CenterX = center[0],
                CenterY = center[1]
            });
        }

        private void ReadVisualizer(string key, JsonNode node, JsonNode all) {
            var spec = new VisualizerSpec {
                Id = key,
                Rect = node.Rect("rect"),
                Bars = (int)node.Number("bars"),
                BarWidth = node.Number("bar_w"),
                BarGap = node.Number("bar_gap"),
                Segments = (int)node.Number("segments"),
                SegmentHeight = node.Number("seg_h"),
                SegmentStep = node.Number("seg_step"),
                SegmentFile = AssetPath(node.String("segment_sprite")),
                Ramp = node.Colors("ramp"),
                Off = node.Color("off"),
                FrameMs = node.Number("frame_ms", DefaultFrameMs)
            };
            if (spec.Ramp.Count != spec.Segments)
                throw new BackgroundDataException($"{node.Path}.ramp: segments({spec.Segments})와 개수가 다르다({spec.Ramp.Count})");
            _layout.Visualizers.Add(spec);
        }

        private void ReadNotes(string key, JsonNode node, JsonNode all) {
            _layout.Notes.Add(new NotesSpec {
                Id = key,
                Area = node.Rect("area"),
                File = AssetPath(node.String("sprite")),
                Colors = node.Colors("colors")
            });
        }

        private static void RequireSibling(JsonNode all, string sibling, JsonNode node) {
            if (!all.Has(sibling)) throw new BackgroundDataException($"{node.Path}: '{sibling}' 위젯이 함께 있어야 한다");
        }

        private string AssetPath(string name) => StageBackgroundLoader.Combine(_folder, name);
    }
}
