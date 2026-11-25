using System.Collections.Generic;
using LDY.Title.Core;
using NUnit.Framework;
using UnityEngine;

namespace LDY.Tests.Title {
    // 실제 씬 값: 모니터 쿼드 중심 (-2, 2), 크기 7.48 x 5.61(4:3)
    public class ZoomFramingTests {
        private const float Eps = 1e-4f;
        private static readonly Vector2 Center = new(-2f, 2f);
        private static readonly Vector2 QuadHalf = new(3.74f, 2.805f);
        private static readonly Rect Quad = new(Center - QuadHalf, QuadHalf * 2f);

        private static readonly float[] Aspects = {
            16f / 9f, 4f / 3f, 16f / 10f, 21f / 9f, 9f / 16f, 3f / 4f
        };

        private static IEnumerable<float> AllAspects() => Aspects;

        // ---- Pillarbox 끝 크기 ----

        [TestCase(16f / 9f, 2.805f)]
        [TestCase(4f / 3f, 2.805f)]
        [TestCase(16f / 10f, 2.805f)]
        [TestCase(21f / 9f, 2.805f)]
        [TestCase(9f / 16f, 3.74f * 16f / 9f)]
        public void PillarboxSizeShowsWholeQuad(float aspect, float expected) {
            var size = ZoomFraming.EndOrthoSize(QuadHalf, aspect, ZoomEndFit.Pillarbox);
            Assert.AreEqual(expected, size, Eps);
        }

        [TestCaseSource(nameof(AllAspects))]
        public void PillarboxViewContainsQuadAndTouchesOneAxis(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Pillarbox);
            var view = end.View;
            Assert.LessOrEqual(view.xMin, Quad.xMin + Eps);
            Assert.GreaterOrEqual(view.xMax, Quad.xMax - Eps);
            Assert.LessOrEqual(view.yMin, Quad.yMin + Eps);
            Assert.GreaterOrEqual(view.yMax, Quad.yMax - Eps);

            var touchesHeight = Mathf.Abs(view.height - Quad.height) < Eps;
            var touchesWidth = Mathf.Abs(view.width - Quad.width) < Eps;
            Assert.IsTrue(touchesHeight || touchesWidth);
        }

        // ---- 가림막 ----

        [TestCaseSource(nameof(AllAspects))]
        public void MasksNeverOverlapQuad(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Pillarbox);
            foreach (var mask in end.Masks)
                Assert.AreEqual(0f, OverlapArea(mask, Quad), Eps, $"mask {mask} overlaps quad");
        }

        [TestCaseSource(nameof(AllAspects))]
        public void MasksAndQuadCoverWholeView(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Pillarbox);
            var covered = OverlapArea(end.View, Quad);
            foreach (var mask in end.Masks)
                covered += OverlapArea(mask, end.View);

            Assert.AreEqual(end.View.width * end.View.height, covered, 1e-3f);
            AssertMasksDisjoint(end.Masks);
        }

        [TestCase(16f / 9f)]
        [TestCase(16f / 10f)]
        [TestCase(21f / 9f)]
        public void WideScreenGetsLeftAndRightMasksOnly(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Pillarbox);
            Assert.AreEqual(2, end.Masks.Count);

            var left = end.Masks[0];
            var right = end.Masks[1];
            Assert.AreEqual(end.View.xMin, left.xMin, Eps);
            Assert.AreEqual(Quad.xMin, left.xMax, Eps);
            Assert.AreEqual(Quad.xMax, right.xMin, Eps);
            Assert.AreEqual(end.View.xMax, right.xMax, Eps);
            Assert.AreEqual(end.View.height, left.height, Eps);
            Assert.AreEqual(end.View.height, right.height, Eps);
        }

        [Test]
        public void SixteenByNineMaskWidthMatchesMeasuredMargin() {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, 16f / 9f, ZoomEndFit.Pillarbox);
            // 시야 폭 9.9733 중 쿼드 7.48 → 한쪽 1.2467 (12.5%)
            Assert.AreEqual(end.View.width * 0.125f, end.Masks[0].width, Eps);
        }

        [Test]
        public void FourByThreeHasNoMasks() {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, 4f / 3f, ZoomEndFit.Pillarbox);
            Assert.AreEqual(0, end.Masks.Count);
        }

        [TestCase(9f / 16f)]
        [TestCase(3f / 4f)]
        public void PortraitGetsTopAndBottomMasksOnly(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Pillarbox);
            Assert.AreEqual(2, end.Masks.Count);

            var top = end.Masks[0];
            var bottom = end.Masks[1];
            Assert.AreEqual(Quad.yMax, top.yMin, Eps);
            Assert.AreEqual(end.View.yMax, top.yMax, Eps);
            Assert.AreEqual(end.View.yMin, bottom.yMin, Eps);
            Assert.AreEqual(Quad.yMin, bottom.yMax, Eps);
            Assert.AreEqual(end.View.width, top.width, Eps);
        }

        // 확대 도중(시야가 쿼드보다 큼)엔 네 장이 쿼드를 둘러쌈
        [Test]
        public void MidZoomViewGetsFourMasksHuggingQuad() {
            var view = ZoomFraming.View(Center, 5f, 16f / 9f);
            var masks = ZoomFraming.Masks(view, Quad);
            Assert.AreEqual(4, masks.Count);
            AssertMasksDisjoint(masks);
            foreach (var mask in masks)
                Assert.AreEqual(0f, OverlapArea(mask, Quad), Eps);
        }

        // ---- Cover ----

        [TestCaseSource(nameof(AllAspects))]
        public void CoverViewIsInsideQuad(float aspect) {
            var end = ZoomFraming.End(Center, QuadHalf, Quad, aspect, ZoomEndFit.Cover);
            var view = end.View;
            Assert.GreaterOrEqual(view.xMin, Quad.xMin - Eps);
            Assert.LessOrEqual(view.xMax, Quad.xMax + Eps);
            Assert.GreaterOrEqual(view.yMin, Quad.yMin - Eps);
            Assert.LessOrEqual(view.yMax, Quad.yMax + Eps);
            Assert.AreEqual(0, end.Masks.Count);
        }

        [TestCase(16f / 9f, 3.74f * 9f / 16f)]
        [TestCase(4f / 3f, 2.805f)]
        [TestCase(9f / 16f, 2.805f)]
        public void CoverSize(float aspect, float expected) {
            Assert.AreEqual(expected, ZoomFraming.EndOrthoSize(QuadHalf, aspect, ZoomEndFit.Cover), Eps);
        }

        // ---- 페이드 ----

        [TestCase(0f, 0f)]
        [TestCase(0.7f, 0f)]
        [TestCase(0.85f, 0.5f)]
        [TestCase(1f, 1f)]
        public void MaskFadesFrom70To100Percent(float progress, float expected) {
            Assert.AreEqual(expected, ZoomFraming.MaskAlpha(progress, 0.7f), Eps);
        }

        [Test]
        public void FadeStartAtOneJumpsAtEnd() {
            Assert.AreEqual(0f, ZoomFraming.MaskAlpha(0.99f, 1f));
            Assert.AreEqual(1f, ZoomFraming.MaskAlpha(1f, 1f));
        }

        private static float OverlapArea(Rect a, Rect b) {
            var w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
            var h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
            return w > 0f && h > 0f ? w * h : 0f;
        }

        private static void AssertMasksDisjoint(IReadOnlyList<Rect> masks) {
            for (var i = 0; i < masks.Count; i++)
            for (var j = i + 1; j < masks.Count; j++)
                Assert.AreEqual(0f, OverlapArea(masks[i], masks[j]), Eps, $"mask {i} overlaps mask {j}");
        }
    }
}
