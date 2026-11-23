using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PixelStyle
{
    /// <summary>Stage1 벽 타일(tileset_stage1.png)에서 가져온 팔레트. 눈 4색만 기존 gear 디자인 유지.</summary>
    public static class GearPalette
    {
        public static readonly int Outline = Argb(0x00, 0x00, 0x80);
        public static readonly int Bright = Argb(0xA6, 0xCA, 0xF0);
        public static readonly int Highlight = Argb(0x5E, 0x9A, 0xE0);
        public static readonly int Fill = Argb(0x0D, 0x6E, 0xC2);
        public static readonly int Shade = Argb(0x08, 0x42, 0xA8);
        public static readonly int ShadeRim = Argb(0x05, 0x2C, 0x9A);
        public static readonly int Groove = Argb(0x02, 0x16, 0x8D);

        public static readonly int EyeLight = Argb(0xE0, 0x45, 0x3A);
        public static readonly int EyeMid = Argb(0xA8, 0x24, 0x3A);
        public static readonly int EyeDark = Argb(0x5A, 0x0E, 0x1E);
        public static readonly int EyeSpark = Argb(0xFF, 0xD0, 0xC8);

        public static int Argb(int r, int g, int b)
        {
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }
    }

    /// <summary>톱니 실루엣. 이 N개, 팁/루트 반지름, 이 옆면 각도로 정의한다.</summary>
    public sealed class GearShape
    {
        public int Teeth = 8;
        public double CenterX = 20, CenterY = 20;
        // 원본 gear.aseprite 실루엣(4프레임)에 맞춰 피팅한 값. 불일치 76/3548px.
        public double TipRadius = 18.95, RootRadius = 14.73;
        public double TipHalfAngle = 7.88, BaseHalfAngle = 12.5;
        public double PhaseDeg = 34.13;
        public double StepDeg = 11.25;

        public bool Contains(int px, int py, int frame)
        {
            double dx = px + 0.5 - CenterX;
            double dy = -(py + 0.5 - CenterY);
            double r = Math.Sqrt(dx * dx + dy * dy);
            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            double pitch = 360.0 / Teeth;
            double offset = angle - (PhaseDeg - frame * StepDeg);
            offset = offset - Math.Floor(offset / pitch + 0.5) * pitch;
            double a = Math.Abs(offset);

            double limit;
            if (a <= TipHalfAngle) limit = TipRadius;
            else if (a < BaseHalfAngle)
                limit = TipRadius - (TipRadius - RootRadius) * (a - TipHalfAngle) / (BaseHalfAngle - TipHalfAngle);
            else limit = RootRadius;
            return r <= limit;
        }
    }

    /// <summary>눈(14x14). 기존 gear 디자인을 그대로 유지한다. 원점은 캔버스 (13,13).</summary>
    public static class GearEye
    {
        public const int OriginX = 13, OriginY = 13;

        public static readonly string[] Rows =
        {
            "...kkkkkkkk...",
            "..kkrRRRrrkk..",
            ".krRRRRRRRrrk.",
            "kkRRRRRRRRRrkk",
            "krRRpppRRRRrrk",
            "kRRRpppRRRRRrk",
            "kRRRppkkkRRRrk",
            "kRRRRRkkkRRRrk",
            "krRRRRkkkRRrrk",
            "krRRRRRRRRRrrk",
            "kkrRRRRRRRrrkk",
            ".krrrRRRrrrrk.",
            "..kkrrrrrrkk..",
            "...kkkkkkkk...",
        };

        public static int ColorAt(int px, int py)
        {
            int x = px - OriginX, y = py - OriginY;
            if (y < 0 || y >= Rows.Length || x < 0 || x >= Rows[y].Length) return 0;
            switch (Rows[y][x])
            {
                case 'R': return GearPalette.EyeLight;
                case 'r': return GearPalette.EyeMid;
                case 'k': return GearPalette.EyeDark;
                case 'p': return GearPalette.EyeSpark;
                default: return 0;
            }
        }
    }

    /// <summary>벽과 같은 규칙: 1px 어두운 외곽 + 좌상단 하이라이트 베벨 + 우하단 음영.</summary>
    public sealed class GearFrameRenderer
    {
        public const int Size = 40;
        public double GrooveInner = 8.4, GrooveOuter = 9.6;
        public double LightX = 0.8, LightY = 0.6, LightThreshold = -1.5;

        private readonly GearShape shape;

        public GearFrameRenderer(GearShape shape) { this.shape = shape; }

        public int[,] Render(int frame)
        {
            bool[,] mask = new bool[Size, Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    mask[x, y] = shape.Contains(x, y, frame);

            int[,] layer = EdgeLayers(mask, 3);
            int[,] result = new int[Size, Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    if (!mask[x, y]) continue;
                    int eye = GearEye.ColorAt(x, y);
                    result[x, y] = eye != 0 ? eye : BodyColor(x, y, layer[x, y]);
                }
            return result;
        }

        private int BodyColor(int x, int y, int layer)
        {
            if (layer == 1) return GearPalette.Outline;

            double dx = x + 0.5 - shape.CenterX, dy = y + 0.5 - shape.CenterY;
            double r = Math.Sqrt(dx * dx + dy * dy);
            bool lit = LightX * dx + LightY * dy < LightThreshold;
            bool groove = r >= GrooveInner && r < GrooveOuter;

            if (lit)
            {
                if (layer == 2) return GearPalette.Bright;
                if (layer == 3) return GearPalette.Highlight;
                return groove ? GearPalette.ShadeRim : GearPalette.Fill;
            }
            if (layer == 2) return GearPalette.ShadeRim;
            return groove ? GearPalette.Groove : GearPalette.Shade;
        }

        // 1 = 실루엣 가장자리 픽셀, 2 = 그 안쪽 한 겹 ... maxLayer 이상은 0
        private static int[,] EdgeLayers(bool[,] mask, int maxLayer)
        {
            int[,] layer = new int[Size, Size];
            for (int l = 1; l <= maxLayer; l++)
                for (int y = 0; y < Size; y++)
                    for (int x = 0; x < Size; x++)
                    {
                        if (!mask[x, y] || layer[x, y] != 0) continue;
                        if (TouchesOutside(mask, layer, x, y, l)) layer[x, y] = l;
                    }
            return layer;
        }

        private static bool TouchesOutside(bool[,] mask, int[,] layer, int x, int y, int l)
        {
            int[] dxs = { 1, -1, 0, 0 };
            int[] dys = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dxs[i], ny = y + dys[i];
                bool outside = nx < 0 || ny < 0 || nx >= Size || ny >= Size || !mask[nx, ny];
                if (l == 1 && outside) return true;
                if (l > 1 && !outside && layer[nx, ny] == l - 1) return true;
            }
            return false;
        }
    }

    /// <summary>프레임별 40x40 PNG를 쓴다. 파일명: gear_pixel_{n}.png</summary>
    public static class GearSheetWriter
    {
        public static void Write(string folder, GearShape shape, int frames)
        {
            Directory.CreateDirectory(folder);
            GearFrameRenderer renderer = new GearFrameRenderer(shape);
            for (int f = 0; f < frames; f++)
            {
                int[,] px = renderer.Render(f);
                using (Bitmap bmp = new Bitmap(GearFrameRenderer.Size, GearFrameRenderer.Size, PixelFormat.Format32bppArgb))
                {
                    for (int y = 0; y < GearFrameRenderer.Size; y++)
                        for (int x = 0; x < GearFrameRenderer.Size; x++)
                            bmp.SetPixel(x, y, px[x, y] == 0 ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(px[x, y]));
                    bmp.Save(Path.Combine(folder, "gear_pixel_" + f + ".png"), ImageFormat.Png);
                }
            }
        }
    }
}
