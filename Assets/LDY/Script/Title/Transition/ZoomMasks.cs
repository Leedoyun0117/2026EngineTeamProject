using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // 확대 중 화면 쿼드 바깥을 가리는 월드 사각형들. 카메라 오버레이가 아니라 월드 좌표에 놓아서
    // 쿼드 경계에 딱 붙는다. 쿼드와 겹치지 않으므로 쿼드 위 CRT 연출(꺼짐 포함)은 가리지 않는다.
    public sealed class ZoomMasks {
        private const int MaxMasks = 4;

        private readonly Color _color;
        private readonly int _layer;
        private readonly int _sortingLayerId;
        private readonly int _sortingOrder;
        private readonly List<SpriteRenderer> _renderers = new(MaxMasks);
        private GameObject _root;

        public ZoomMasks(int layer, int sortingLayerId, int sortingOrder, Color color) {
            _layer = layer;
            _sortingLayerId = sortingLayerId;
            _sortingOrder = sortingOrder;
            _color = color;
        }

        // 월드 사각형들을 alpha로 보여 줌. alpha가 0이면 숨김
        public void Show(IReadOnlyList<Rect> rects, float alpha) {
            if (alpha <= 0f || rects.Count == 0) {
                Hide();
                return;
            }

            EnsureCreated();
            _root.SetActive(true);
            var color = new Color(_color.r, _color.g, _color.b, _color.a * alpha);
            for (var i = 0; i < _renderers.Count; i++) {
                var sr = _renderers[i];
                var visible = i < rects.Count;
                sr.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                var rect = rects[i];
                sr.color = color;
                sr.transform.position = new Vector3(rect.center.x, rect.center.y, 0f);
                SpriteFit.Stretch(sr, rect.size);
            }
        }

        public void Hide() {
            if (_root != null)
                _root.SetActive(false);
        }

        public void Release() {
            if (_root != null)
                Object.Destroy(_root);
            _root = null;
            _renderers.Clear();
        }

        // 부모 스케일 영향을 받지 않게 씬 루트에 둠
        private void EnsureCreated() {
            if (_root != null)
                return;

            _renderers.Clear();
            _root = new GameObject("ZoomMasks") { layer = _layer };
            for (var i = 0; i < MaxMasks; i++) {
                var go = new GameObject("Mask" + i) { layer = _layer };
                go.transform.SetParent(_root.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = WindowKit.PixelSprite;
                sr.sortingLayerID = _sortingLayerId;
                sr.sortingOrder = _sortingOrder;
                _renderers.Add(sr);
            }
        }
    }
}
