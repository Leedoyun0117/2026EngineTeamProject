using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LDY.Script {
    internal readonly struct ClipSheetRow {
        public readonly string Label;
        public readonly IReadOnlyList<Sprite> Sprites;

        public ClipSheetRow(string label, IReadOnlyList<Sprite> sprites) {
            Label = label;
            Sprites = sprites;
        }
    }

    // 클립마다 실제로 들어간 스프라이트를 한 줄로 이어 붙인 그림을 만든다. 스프라이트는 피벗이 칸 아래 가운데에 오게 놓는다.
    internal static class ClipSheetRenderer {
        private const int Cell = 64;
        private const int Gap = 4;
        private const int LabelScale = 2;
        private const int NumberScale = 1;

        private static readonly Color32 Background = new(24, 24, 28, 255);
        private static readonly Color32 CellColor = new(56, 56, 64, 255);
        private static readonly Color32 LabelColor = new(255, 255, 255, 255);
        private static readonly Color32 NumberColor = new(255, 210, 80, 255);
        private static readonly Color32 MissingColor = new(220, 40, 40, 255);

        public static Texture2D Render(IReadOnlyList<ClipSheetRow> rows) {
            var labelWidth = rows.Max(r => PixelFont.WidthOf(r.Label, LabelScale)) + Gap * 2;
            var columns = rows.Max(r => r.Sprites.Count);
            var width = labelWidth + columns * (Cell + Gap) + Gap;
            var height = rows.Count * (Cell + Gap) + Gap;
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = Background;

            for (var row = 0; row < rows.Count; row++)
                DrawRow(pixels, width, height, rows[row], row, labelWidth);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static void DrawRow(Color32[] pixels, int width, int height, ClipSheetRow row, int index, int labelWidth) {
            var bottom = height - (index + 1) * (Cell + Gap);
            PixelFont.Draw(pixels, width, height, Gap, bottom + Cell / 2 + PixelFont.GlyphHeight, row.Label, LabelScale,
                LabelColor);

            for (var i = 0; i < row.Sprites.Count; i++) {
                var left = labelWidth + i * (Cell + Gap);
                FillCell(pixels, width, left, bottom, row.Sprites[i] == null ? MissingColor : CellColor);
                if (row.Sprites[i] == null)
                    continue;

                DrawSprite(pixels, width, left, bottom, row.Sprites[i]);
                PixelFont.Draw(pixels, width, height, left + 2, bottom + Cell - 2, FrameNumber(row.Sprites[i]),
                    NumberScale, NumberColor);
            }
        }

        private static string FrameNumber(Sprite sprite) {
            var cut = sprite.name.LastIndexOf('_');
            return cut >= 0 ? sprite.name.Substring(cut + 1) : sprite.name;
        }

        private static void FillCell(Color32[] pixels, int width, int left, int bottom, Color32 color) {
            for (var y = 0; y < Cell; y++)
            for (var x = 0; x < Cell; x++)
                pixels[(bottom + y) * width + left + x] = color;
        }

        private static void DrawSprite(Color32[] pixels, int width, int left, int bottom, Sprite sprite) {
            var rect = sprite.textureRect;
            var source = ReadPixels(sprite);
            var offsetX = Cell / 2 - Mathf.RoundToInt(sprite.pivot.x);
            var offsetY = -Mathf.RoundToInt(sprite.pivot.y);
            var spriteWidth = (int)rect.width;
            for (var y = 0; y < (int)rect.height; y++)
            for (var x = 0; x < spriteWidth; x++) {
                var cellX = x + offsetX;
                var cellY = y + offsetY;
                var color = source[y * spriteWidth + x];
                if (color.a == 0 || cellX < 0 || cellX >= Cell || cellY < 0 || cellY >= Cell)
                    continue;

                pixels[(bottom + cellY) * width + left + cellX] = color;
            }
        }

        // 읽기 불가 텍스처를 RenderTexture로 복사해 스프라이트 영역만 읽는다.
        private static Color32[] ReadPixels(Sprite sprite) {
            var texture = sprite.texture;
            var rect = sprite.textureRect;
            var target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            var copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            try {
                Graphics.Blit(texture, target);
                RenderTexture.active = target;
                copy.ReadPixels(new Rect(rect.x, rect.y, rect.width, rect.height), 0, 0);
                return copy.GetPixels32();
            }
            finally {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(copy);
            }
        }
    }
}
