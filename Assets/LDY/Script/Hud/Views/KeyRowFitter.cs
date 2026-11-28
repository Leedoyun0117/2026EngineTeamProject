using System;
using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 키 버튼들을 after(슬롯 필드) 오른쪽에 gap 간격으로 나란히 두고 세로 가운데에 맞춘다.
    // 버튼 크기는 각 KeyCapFitter가 먼저 정하므로 그 뒤에 돈다. limit(조작 대상 칸)을 넘으면 경고.
    [DefaultExecutionOrder(100)]
    public sealed class KeyRowFitter : MonoBehaviour {
        [SerializeField] private RectTransform after;
        [SerializeField, Min(0)] private int gap = 4;
        [SerializeField] private KeyCapFitter[] keys = Array.Empty<KeyCapFitter>();
        [SerializeField] private RectTransform limit;

        private int[] _widths = Array.Empty<int>();
        private int _start = int.MinValue;

        private void OnEnable() => Fit();

        private void LateUpdate() {
            if (RowStart() != _start || WidthsChanged())
                Fit();
        }

        [ContextMenu("Fit Now")]
        public void Fit() {
            var parent = transform as RectTransform;
            if (parent == null || after == null || keys == null || keys.Length == 0) {
                HudWarnings.Once("keyrow-wiring:" + KeyCapFitter.PathOf(transform), $"{KeyCapFitter.PathOf(transform)}: KeyRowFitter 연결이 비어 있어 버튼을 못 줄 세움", this);
                return;
            }

            _start = RowStart();
            _widths = Widths();
            var xs = KeyCapSizer.Row(_start, gap, _widths);
            var rowHeight = Mathf.RoundToInt(parent.rect.height);
            var right = 0;
            for (var i = 0; i < keys.Length; i++) {
                var rect = Rect(i);
                if (rect == null)
                    continue;

                var height = Mathf.RoundToInt(rect.rect.height);
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(xs[i], KeyCapSizer.Center(rowHeight, height));
                right = xs[i] + _widths[i];
            }

            if (limit != null && right > LeftOf(limit)) {
                var path = KeyCapFitter.PathOf(LastRect());
                HudWarnings.Once("overflow:" + path, $"{path}: 버튼 오른쪽 끝 {right}px가 {limit.name} 칸 왼쪽 {LeftOf(limit)}px를 넘음", this);
            }
        }

        // 부모 왼쪽 아래 기준 x로 맞춘다(버튼 anchor가 (0,0)이므로).
        private int RowStart() {
            var parent = (RectTransform)transform;
            return after == null ? 0 : Mathf.RoundToInt(after.localPosition.x + after.rect.xMax - parent.rect.xMin);
        }

        private int LeftOf(RectTransform rect) {
            var parent = (RectTransform)transform;
            return Mathf.RoundToInt(rect.localPosition.x + rect.rect.xMin - parent.rect.xMin);
        }

        private RectTransform Rect(int i) => keys[i] != null ? keys[i].Target : null;

        private Transform LastRect() {
            var rect = Rect(keys.Length - 1);
            return rect != null ? rect : transform;
        }

        private int[] Widths() {
            var widths = new int[keys.Length];
            for (var i = 0; i < keys.Length; i++) {
                var rect = Rect(i);
                widths[i] = rect != null ? Mathf.RoundToInt(rect.rect.width) : 0;
            }

            return widths;
        }

        private bool WidthsChanged() {
            if (_widths.Length != keys.Length)
                return true;
            for (var i = 0; i < keys.Length; i++) {
                var rect = Rect(i);
                if (rect != null && Mathf.RoundToInt(rect.rect.width) != _widths[i])
                    return true;
            }

            return false;
        }
    }
}
