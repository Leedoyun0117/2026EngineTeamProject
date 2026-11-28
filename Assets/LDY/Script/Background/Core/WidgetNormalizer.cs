using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.Background.Core {
    internal sealed class WidgetNormalizer {
        private const float DefaultFrameMs = 120f;
        private const float DefaultCaretGap = 4f;
        private const string SelectionBarSuffix = "_selection_bar";
        private const string CaretSuffix = "_caret";
        private const string ProgressTrack = "progress_track";
        private const string ProgressThumb = "progress_thumb";

        private static readonly Dictionary<string, int> NoteDefaultRows = new Dictionary<string, int> {
            { "tree_selection_bar", 4 }
        };

        private readonly string _folder;
        private readonly StageBackgroundLayout _layout;
        private readonly JsonNode _sheets;
        private readonly int? _frames;
        private readonly float _frameMs;
        private readonly Dictionary<string, Action<string, JsonNode, JsonNode>> _byKey;

        public WidgetNormalizer(string folder, StageBackgroundLayout layout, JsonNode root) {
            _folder = folder;
            _layout = layout;
            _sheets = root.ChildOrNull("sheets");
            _frames = root.Has("frames") ? (int)root.Number("frames") : (int?)null;
            _frameMs = root.Number("frame_ms", DefaultFrameMs);
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
            var prefix = key.Substring(0, key.Length - SelectionBarSuffix.Length) + "_";
            var rows = _layout.Texts.Where(t => t.Id.StartsWith(prefix)).ToList();
            if (rows.Count == 0) throw new BackgroundDataException($"{node.Path}: '{prefix}*' 텍스트 행이 없다");
            if (node.Has("text_color")) {
                var selected = node.Color("text_color");
                foreach (var slot in rows.Where(r => !r.SelectedColor.HasValue)) slot.SelectedColor = selected;
            }

            PixelRect rect;
            float step;
            if (node.Has("rect")) {
                rect = node.Rect("rect");
                step = node.Number("row_step");
            }
            else {
                var height = node.Number("row_h");
                var x = node.Numbers("x", 2);
                var top = (float)Math.Floor(rows[0].Rect.CenterY - height * 0.5f);
                rect = new PixelRect(x[0], top, x[1] - x[0], height);
                step = node.Number("row_step", height);
            }

            _layout.SelectionBars.Add(new SelectionBarSpec {
                Id = key,
                Rect = rect,
                Color = node.Color("color"),
                RowStep = step,
                DefaultRow = node.Has("default_row")
                    ? (int)node.Number("default_row")
                    : NoteDefaultRows.TryGetValue(key, out var row) ? row : 0,
                RowTextIds = rows.Select(r => r.Id).ToList()
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
            var file = node.String("sheet");
            var placement = FindPlacement(key, file);
            float centerX, centerY;
            if (node.Has("center")) {
                var center = node.Numbers("center", 2);
                centerX = center[0];
                centerY = center[1];
            }
            else if (placement != null) {
                centerX = placement.Number("x") + placement.Number("w") * 0.5f;
                centerY = placement.Number("y") + placement.Number("h") * 0.5f;
            }
            else {
                throw new BackgroundDataException($"{node.Path}: 필수 키 'center'가 없다 (sheets 배치도 없음)");
            }

            var spec = new SheetAnimSpec {
                Id = key,
                File = AssetPath(file),
                Frames = ReadFrames(node, placement),
                FrameMs = node.Number("frame_ms", _frameMs),
                CenterX = centerX,
                CenterY = centerY
            };
            _layout.Sheets.Add(spec);
            if (key.EndsWith(CaretSuffix)) ReadCaret(spec, node);
        }

        private int ReadFrames(JsonNode node, JsonNode placement) {
            if (node.Has("frames")) return (int)node.Number("frames");
            if (placement != null && placement.Has("frames")) return (int)placement.Number("frames");
            if (_frames.HasValue) return _frames.Value;
            throw new BackgroundDataException($"{node.Path}: 필수 키 'frames'가 없다 (sheets, 최상위에도 없음)");
        }

        private JsonNode FindPlacement(string key, string file) {
            if (_sheets == null) return null;
            if (_sheets.Has(key)) return _sheets.Child(key);
            foreach (var name in _sheets.Keys) {
                var entry = _sheets.Child(name);
                if (entry.Has("file") && entry.String("file") == file) return entry;
            }

            var suffix = _sheets.Keys.FirstOrDefault(name => key.EndsWith("_" + name));
            return suffix != null ? _sheets.Child(suffix) : null;
        }

        private void ReadCaret(SheetAnimSpec sheet, JsonNode node) {
            var text = _layout.Texts
                .Where(t => t.Rect.Y <= sheet.CenterY && sheet.CenterY <= t.Rect.Y + t.Rect.Height && t.Rect.X <= sheet.CenterX)
                .OrderByDescending(t => t.Rect.X)
                .FirstOrDefault();
            if (text == null) throw new BackgroundDataException($"{node.Path}: 캐럿 왼쪽 같은 줄에 텍스트가 없다");
            _layout.Carets.Add(new CaretSpec {
                SheetId = sheet.Id,
                TextId = text.Id,
                GapPixels = node.Number("gap", DefaultCaretGap)
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
                FrameMs = node.Number("frame_ms", _frameMs)
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
