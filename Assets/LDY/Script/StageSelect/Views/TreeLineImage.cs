using LDY.Flow;
using LDY.StageSelect.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.StageSelect {
    // 트리 연결선과 +/- 상자를 트리 패널 크기의 텍스처 한 장에 찍는다. 텍스처 1px = 캔버스 1px.
    // 행·아이콘보다 뒤(첫 자식)에 깔려서 상자가 아이콘·글자를 가리지 않는다.
    public sealed class TreeLineImage : MonoBehaviour {
        private static readonly Color32 Line = new Color32(0x80, 0x80, 0x80, 0xFF);
        private static readonly Color32 BoxBorder = new Color32(0x80, 0x80, 0x80, 0xFF);
        private static readonly Color32 BoxFill = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        private static readonly Color32 Sign = new Color32(0x00, 0x00, 0x00, 0xFF);
        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        private RawImage _image;
        private Texture2D _texture;

        public static TreeLineImage Create(Transform panel) {
            var rect = UiFactory.Rect("TreeLines", panel);
            rect.SetAsFirstSibling();
            var image = rect.gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            var lines = rect.gameObject.AddComponent<TreeLineImage>();
            lines._image = image;
            return lines;
        }

        public void Draw(TreeDrawing drawing) {
            var size = ((RectTransform)transform.parent).rect.size;
            var width = Mathf.Max(1, Mathf.CeilToInt(size.x));
            var height = Mathf.Max(1, Mathf.CeilToInt(size.y));
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Clear;

            var canvas = new Canvas32(pixels, width, height);
            foreach (var segment in drawing.Segments) DrawSegment(canvas, segment);
            foreach (var box in drawing.Boxes) DrawBox(canvas, box);

            Release();
            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
                name = "TreeLines",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            _texture.SetPixels32(pixels);
            _texture.Apply(false, true);
            _image.texture = _texture;
            UiFactory.Place(_image.rectTransform, 0f, 0f, width, height);
        }

        private void OnDestroy() => Release();

        private void Release() {
            if (_texture == null) return;
            Destroy(_texture);
            _texture = null;
        }

        private static void DrawSegment(Canvas32 canvas, TreeSegment segment) {
            for (var i = 0; i < segment.Length; i++) {
                var x = segment.Axis == LineAxis.Horizontal ? segment.X + i : segment.X;
                var y = segment.Axis == LineAxis.Vertical ? segment.Y + i : segment.Y;
                if (TreeLines.IsLit(x, y)) canvas.Set(x, y, Line);
            }
        }

        // 흰 바탕 + 회색 테두리 + 검정 '−'(펼침 고정, 장식).
        private static void DrawBox(Canvas32 canvas, TreeBox box) {
            const int size = TreeLines.BoxSize;
            for (var dy = 0; dy < size; dy++)
            for (var dx = 0; dx < size; dx++) {
                var edge = dx == 0 || dy == 0 || dx == size - 1 || dy == size - 1;
                canvas.Set(box.X + dx, box.Y + dy, edge ? BoxBorder : BoxFill);
            }

            for (var dx = 2; dx <= size - 3; dx++) canvas.Set(box.X + dx, box.Y + size / 2, Sign);
        }

        // 위쪽 원점 픽셀 좌표로 쓰고, 텍스처(아래쪽 원점)로 뒤집어 넣는다.
        private readonly struct Canvas32 {
            private readonly Color32[] _pixels;
            private readonly int _width;
            private readonly int _height;

            public Canvas32(Color32[] pixels, int width, int height) {
                _pixels = pixels;
                _width = width;
                _height = height;
            }

            public void Set(int x, int y, Color32 color) {
                if (x < 0 || y < 0 || x >= _width || y >= _height) return;
                _pixels[(_height - 1 - y) * _width + x] = color;
            }
        }
    }
}
