using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // 클립 시트에 이름과 번호를 찍는 3x5 비트맵 글꼴. 영문 대문자, 숫자, '_', '-'만 지원한다.
    internal static class PixelFont {
        public const int GlyphWidth = 3;
        public const int GlyphHeight = 5;
        public const int Advance = GlyphWidth + 1;

        private static readonly Dictionary<char, string> Glyphs = new() {
            ['A'] = ".#.#.#####.##.#",
            ['B'] = "##.#.###.#.###.",
            ['C'] = ".###..#..#...##",
            ['D'] = "##.#.##.##.###.",
            ['E'] = "####..##.#..###",
            ['F'] = "####..##.#..#..",
            ['G'] = ".###..#.##.#.##",
            ['H'] = "#.##.#####.##.#",
            ['I'] = "###.#..#..#.###",
            ['J'] = "..#..#..##.#.#.",
            ['K'] = "#.##.###.#.##.#",
            ['L'] = "#..#..#..#..###",
            ['M'] = "#.########.##.#",
            ['N'] = "##.#.##.##.##.#",
            ['O'] = ".#.#.##.##.#.#.",
            ['P'] = "##.#.###.#..#..",
            ['Q'] = ".#.#.##.###..##",
            ['R'] = "##.#.###.#.##.#",
            ['S'] = ".###...#...###.",
            ['T'] = "###.#..#..#..#.",
            ['U'] = "#.##.##.##.####",
            ['V'] = "#.##.##.##.#.#.",
            ['W'] = "#.##.########.#",
            ['X'] = "#.##.#.#.#.##.#",
            ['Y'] = "#.##.#.#..#..#.",
            ['Z'] = "###..#.#.#..###",
            ['0'] = "####.##.##.####",
            ['1'] = ".#.##..#..#.###",
            ['2'] = "##...#.#.#..###",
            ['3'] = "##...#.#...###.",
            ['4'] = "#.##.####..#..#",
            ['5'] = "####..##...###.",
            ['6'] = ".###..####.####",
            ['7'] = "###..#.#..#..#.",
            ['8'] = "####.#####.####",
            ['9'] = "####.####..###.",
            ['_'] = "............###",
            ['-'] = "......###......",
        };

        public static int WidthOf(string text, int scale) {
            return text.Length * Advance * scale;
        }

        // x, y는 글자 왼쪽 위 모서리이고 y는 위쪽이 큰 텍스처 좌표다.
        public static void Draw(Color32[] pixels, int width, int height, int x, int y, string text, int scale,
            Color32 color) {
            foreach (var character in text.ToUpperInvariant()) {
                if (Glyphs.TryGetValue(character, out var glyph))
                    DrawGlyph(pixels, width, height, x, y, glyph, scale, color);

                x += Advance * scale;
            }
        }

        private static void DrawGlyph(Color32[] pixels, int width, int height, int x, int y, string glyph, int scale,
            Color32 color) {
            for (var row = 0; row < GlyphHeight; row++)
            for (var column = 0; column < GlyphWidth; column++) {
                if (glyph[row * GlyphWidth + column] != '#')
                    continue;

                Fill(pixels, width, height, x + column * scale, y - row * scale, scale, color);
            }
        }

        private static void Fill(Color32[] pixels, int width, int height, int x, int top, int size, Color32 color) {
            for (var dy = 0; dy < size; dy++)
            for (var dx = 0; dx < size; dx++) {
                var px = x + dx;
                var py = top - dy;
                if (px >= 0 && px < width && py >= 0 && py < height)
                    pixels[py * width + px] = color;
            }
        }
    }
}
