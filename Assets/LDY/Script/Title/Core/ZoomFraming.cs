using System.Collections.Generic;
using UnityEngine;

namespace LDY.Title.Core {
    // 시작 확대가 끝났을 때의 프레이밍
    public enum ZoomEndFit {
        Pillarbox, // 맞출 영역이 다 보이게 멈추고, 화면 쿼드 바깥은 가림막으로 가린다
        Cover // 맞출 영역이 화면을 꽉 채우게 더 확대한다(넘치는 쪽은 잘림)
    }

    public readonly struct ZoomEndFrame {
        public ZoomEndFrame(float orthoSize, Rect view, IReadOnlyList<Rect> masks) {
            OrthoSize = orthoSize;
            View = view;
            Masks = masks;
        }

        public float OrthoSize { get; }
        public Rect View { get; }
        public IReadOnlyList<Rect> Masks { get; }
    }

    // 직교 카메라 확대 끝 크기와 가림막 사각형 계산. 좌표는 모두 월드, aspect = 가로 / 세로
    public static class ZoomFraming {
        private static readonly Rect[] NoMasks = new Rect[0];

        // fitHalf: 화면에 맞출 영역의 반크기(CRT 곡률 모서리를 뺀 안전 영역일 수 있음)
        public static float EndOrthoSize(Vector2 fitHalf, float aspect, ZoomEndFit fit) {
            var byHeight = fitHalf.y;
            var byWidth = fitHalf.x / aspect;
            return fit == ZoomEndFit.Pillarbox ? Mathf.Max(byHeight, byWidth) : Mathf.Min(byHeight, byWidth);
        }

        public static Rect View(Vector2 center, float orthoSize, float aspect) {
            var half = new Vector2(orthoSize * aspect, orthoSize);
            return new Rect(center - half, half * 2f);
        }

        // 시야에서 쿼드를 뺀 부분을 서로 겹치지 않는 사각형으로 나눈다(빈 것은 뺌).
        // 좌우는 시야 전체 높이, 상하는 좌우 사이(쿼드 폭과 시야의 교집합) 구간만 덮는다.
        public static List<Rect> Masks(Rect view, Rect quad) {
            var masks = new List<Rect>(4);
            var innerLeft = Mathf.Clamp(quad.xMin, view.xMin, view.xMax);
            var innerRight = Mathf.Clamp(quad.xMax, view.xMin, view.xMax);
            var innerBottom = Mathf.Clamp(quad.yMin, view.yMin, view.yMax);
            var innerTop = Mathf.Clamp(quad.yMax, view.yMin, view.yMax);

            AddIfNotEmpty(masks, Rect.MinMaxRect(view.xMin, view.yMin, innerLeft, view.yMax));
            AddIfNotEmpty(masks, Rect.MinMaxRect(innerRight, view.yMin, view.xMax, view.yMax));
            AddIfNotEmpty(masks, Rect.MinMaxRect(innerLeft, innerTop, innerRight, view.yMax));
            AddIfNotEmpty(masks, Rect.MinMaxRect(innerLeft, view.yMin, innerRight, innerBottom));
            return masks;
        }

        // 확대 끝 상태. Cover는 화면이 쿼드 안쪽이라 가림막이 없다
        public static ZoomEndFrame End(Vector2 center, Vector2 fitHalf, Rect quad, float aspect, ZoomEndFit fit) {
            var size = EndOrthoSize(fitHalf, aspect, fit);
            var view = View(center, size, aspect);
            IReadOnlyList<Rect> masks = fit == ZoomEndFit.Pillarbox ? Masks(view, quad) : NoMasks;
            return new ZoomEndFrame(size, view, masks);
        }

        // 확대 진행률 fadeStart~1 구간에서 0→1
        public static float MaskAlpha(float progress, float fadeStart) {
            if (fadeStart >= 1f)
                return progress >= 1f ? 1f : 0f;

            return Mathf.Clamp01((progress - fadeStart) / (1f - fadeStart));
        }

        // 경계가 맞닿을 때 부동소수점 오차로 생기는 실처럼 얇은 사각형은 버림(월드 단위)
        private const float MinMaskSize = 1e-4f;

        private static void AddIfNotEmpty(List<Rect> masks, Rect rect) {
            if (rect.width > MinMaskSize && rect.height > MinMaskSize)
                masks.Add(rect);
        }
    }
}
