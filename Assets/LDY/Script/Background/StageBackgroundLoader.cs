using System;
using System.Linq;
using LDY.Background.Core;

namespace LDY.Background {
    public static class StageBackgroundLoader {
        private const string DefaultStaticFile = "window_base_notext.png";
        private static readonly string[] ReferenceCandidates = {
            "reference_text_positions.png",
            "reference_window_with_text.png"
        };

        public static StageBackgroundLayout Load(string json, string folder, float pixelsPerUnit, string sourceName,
            Func<string, bool> fileExists) {
            var root = JsonNode.Root(json, sourceName);
            var canvas = root.Numbers("canvas", 2);
            var layout = new StageBackgroundLayout(
                new PixelSpace(canvas[0], canvas[1], pixelsPerUnit),
                Combine(folder, root.String("static", DefaultStaticFile)),
                ReferenceCandidates.Select(f => Combine(folder, f)).FirstOrDefault(fileExists));

            foreach (var text in root.Child("texts").Items()) layout.Texts.Add(ReadText(text));
            if (root.Has("widgets")) new WidgetNormalizer(folder, layout).Read(root.Child("widgets"));
            return layout;
        }

        internal static string Combine(string folder, string file) => folder.TrimEnd('/') + "/" + file;

        private static TextSlotSpec ReadText(JsonNode node) {
            return new TextSlotSpec {
                Id = node.String("id"),
                Rect = node.Rect("rect"),
                Align = ReadAlign(node),
                Color = node.Color("color", new Rgba(0f, 0f, 0f, 1f)),
                SelectedColor = node.Has("selected_color") ? node.Color("selected_color") : (Rgba?)null,
                DefaultText = node.String("default", "")
            };
        }

        private static TextAlign ReadAlign(JsonNode node) {
            var align = node.String("align", "left");
            switch (align) {
                case "left": return TextAlign.Left;
                case "center": return TextAlign.Center;
                case "right": return TextAlign.Right;
                default: throw new BackgroundDataException($"{node.Path}.align: 알 수 없는 값 '{align}'");
            }
        }
    }
}
