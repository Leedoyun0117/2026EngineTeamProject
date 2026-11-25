using System.Collections.Generic;

namespace LDY.StageSelect.Core {
    // 한 방향 발판 하나(캔버스 픽셀). 썸네일 윗변이라 두께가 없다.
    public readonly struct PlatformSpan {
        public PlatformSpan(string id, float left, float right, float top) {
            Id = id;
            Left = left;
            Right = right;
            Top = top;
        }

        public string Id { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float CenterX => (Left + Right) * 0.5f;
    }

    // 파일 목록 패널 안 발판 구성. 벽·바닥·천장은 패널 바깥쪽에 두께를 가진 막힌 상자다.
    public sealed class BoardPlan {
        public BoardPlan(IReadOnlyList<PlatformSpan> files, PixelRect floor, PixelRect ceiling, PixelRect leftWall,
            PixelRect rightWall) {
            Files = files;
            Floor = floor;
            Ceiling = ceiling;
            LeftWall = leftWall;
            RightWall = rightWall;
        }

        public IReadOnlyList<PlatformSpan> Files { get; }
        public PixelRect Floor { get; }
        public PixelRect Ceiling { get; }
        public PixelRect LeftWall { get; }
        public PixelRect RightWall { get; }

        public int IndexOf(string id) {
            if (string.IsNullOrEmpty(id)) return -1;
            for (var i = 0; i < Files.Count; i++)
                if (Files[i].Id == id)
                    return i;
            return -1;
        }
    }

    public static class BoardPlanner {
        // panel: 파일 목록 패널(캔버스 픽셀). ids: 그리드에 보이는 순서.
        // 트리·탭·주소창은 넣지 않는다. 패널 밖으로 밀려난 칸(바닥 아래)은 발판을 만들지 않는다.
        public static BoardPlan Plan(PixelRect panel, IReadOnlyList<string> ids, float wallThickness) {
            var files = new List<PlatformSpan>(ids.Count);
            for (var i = 0; i < ids.Count; i++) {
                var thumb = FileGridLayout.ThumbnailOf(i).Offset(panel.X, panel.Y);
                if (thumb.Y >= panel.Bottom) continue;
                files.Add(new PlatformSpan(ids[i], thumb.X, thumb.Right, thumb.Y));
            }

            var t = wallThickness;
            var wide = panel.Width + t * 2f;
            return new BoardPlan(files,
                new PixelRect(panel.X - t, panel.Bottom, wide, t),
                new PixelRect(panel.X - t, panel.Y - t, wide, t),
                new PixelRect(panel.X - t, panel.Y, t, panel.Height),
                new PixelRect(panel.Right, panel.Y, t, panel.Height));
        }
    }

    // 서 있는 발판의 파일이 선택이 된다. 발판을 떠나면(점프·낙하) 마지막 선택을 그대로 둔다.
    public static class StandSelection {
        public static string Next(string current, string stoodOn) => string.IsNullOrEmpty(stoodOn) ? current : stoodOn;
    }
}
