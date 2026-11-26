using System.Collections.Generic;

namespace LDY.StageSelect.Core {
    // 트리 한 줄 배치(트리 패널 왼쪽 위 기준 정수 픽셀). 아이콘은 16px 폭.
    public readonly struct TreeMetrics {
        public TreeMetrics(int left, int top, int rowHeight, int indent, int iconWidth) {
            Left = left;
            Top = top;
            RowHeight = rowHeight;
            Indent = indent;
            IconWidth = iconWidth;
        }

        public int Left { get; }
        public int Top { get; }
        public int RowHeight { get; }
        public int Indent { get; }
        public int IconWidth { get; }

        public int IconX(int depth) => Left + depth * Indent;
        public int IconCenterX(int depth) => IconX(depth) + IconWidth / 2;
        public int RowTop(int row) => Top + row * RowHeight;
        public int RowMid(int row) => RowTop(row) + RowHeight / 2;

        // 부모 세로선이 지나는 x(= 부모 아이콘 가로 중앙). 깊이 0은 화면 밖에 있다고 치고 같은 규칙으로 계산한다.
        public int ConnectorX(int depth) => IconX(depth) - Indent + IconWidth / 2;
    }

    public enum LineAxis {
        Horizontal,
        Vertical
    }

    // 1px 점선 한 줄. (X, Y)에서 오른쪽(가로) 또는 아래(세로)로 Length 픽셀.
    public readonly struct TreeSegment {
        public TreeSegment(LineAxis axis, int x, int y, int length) {
            Axis = axis;
            X = x;
            Y = y;
            Length = length;
        }

        public LineAxis Axis { get; }
        public int X { get; }
        public int Y { get; }
        public int Length { get; }
        public int EndX => Axis == LineAxis.Horizontal ? X + Length - 1 : X;
        public int EndY => Axis == LineAxis.Vertical ? Y + Length - 1 : Y;
    }

    public sealed class TreeDrawing {
        public TreeDrawing(IReadOnlyList<TreeSegment> segments, IReadOnlyList<TreeBox> boxes) {
            Segments = segments;
            Boxes = boxes;
        }

        public IReadOnlyList<TreeSegment> Segments { get; }
        public IReadOnlyList<TreeBox> Boxes { get; }
    }

    // +/- 상자의 왼쪽 위. 크기는 TreeLines.BoxSize.
    public readonly struct TreeBox {
        public TreeBox(int x, int y, int row) {
            X = x;
            Y = y;
            Row = row;
        }

        public int X { get; }
        public int Y { get; }
        public int Row { get; }
    }

    // Win98 탐색기 트리 연결선.
    // 자식이 있는 노드: 아이콘 아래에서 마지막 자식 행 가운데까지 세로선(마지막 자식에서 └로 끝남) + 자기 연결점에 9x9 상자.
    // 깊이 1 이상 노드: 부모 세로선에서 자기 아이콘 바로 앞까지 가로선.
    public static class TreeLines {
        public const int BoxSize = 9;

        // 1px 켜고 1px 끄기. 위상을 좌표 합으로 정해서 가로·세로선이 만나는 곳에서도 무늬가 이어진다.
        public static bool IsLit(int x, int y) => ((x + y) & 1) == 0;

        // depths: 위에서부터 행 순서의 깊이(앞 행보다 최대 1 깊어진다).
        public static TreeDrawing Build(IReadOnlyList<int> depths, TreeMetrics metrics) {
            var segments = new List<TreeSegment>();
            var boxes = new List<TreeBox>();
            for (var row = 0; row < depths.Count; row++) {
                var depth = depths[row];
                if (depth > 0) {
                    var from = metrics.ConnectorX(depth);
                    var length = metrics.IconX(depth) - from;
                    if (length > 0)
                        segments.Add(new TreeSegment(LineAxis.Horizontal, from, metrics.RowMid(row), length));
                }

                var lastChild = LastChild(depths, row);
                if (lastChild < 0) continue;

                var x = metrics.IconCenterX(depth);
                var top = metrics.RowTop(row) + metrics.RowHeight;
                segments.Add(new TreeSegment(LineAxis.Vertical, x, top, metrics.RowMid(lastChild) - top + 1));
                boxes.Add(new TreeBox(metrics.ConnectorX(depth) - BoxSize / 2, metrics.RowMid(row) - BoxSize / 2, row));
            }

            return new TreeDrawing(segments, boxes);
        }

        // 바로 아래 깊이의 마지막 자식 행. 자식이 없으면 -1.
        public static int LastChild(IReadOnlyList<int> depths, int row) {
            var last = -1;
            for (var i = row + 1; i < depths.Count; i++) {
                if (depths[i] <= depths[row]) break;
                if (depths[i] == depths[row] + 1) last = i;
            }

            return last;
        }
    }
}
