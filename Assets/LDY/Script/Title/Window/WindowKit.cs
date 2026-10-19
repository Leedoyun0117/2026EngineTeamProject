using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // uGUI 대신 월드 오브젝트로 창을 만듦. 길이는 월드 유닛, 도트 1픽셀 = Pixel(1/60)
    public sealed class WindowKit
    {
        public const float Pixel = 1f / 60f;
        private const float PlatformThickness = 0.1f;

        private static Sprite _pixelSprite;

        private readonly List<Collider2D> _platforms = new List<Collider2D>();
        private readonly TMP_FontAsset _font;
        private readonly int _groundLayer;

        public WindowKit(TitleArtSet art, TMP_FontAsset font)
        {
            Art = art;
            _font = font;
            _groundLayer = LayerMask.NameToLayer("Ground");
            if (_groundLayer < 0)
            {
                Debug.LogWarning("[Window] Ground 레이어가 없어서 발판을 Default 레이어에 만듦");
                _groundLayer = 0;
            }
        }

        public TitleArtSet Art { get; }
        public int GroundLayer => _groundLayer;

        public IReadOnlyList<Collider2D> Platforms => _platforms;

        // 스프라이트 슬롯이 비었을 때 쓰는 1x1 흰 사각형
        private static Sprite PixelSprite
        {
            get
            {
                if (_pixelSprite == null)
                {
                    Texture2D white = Texture2D.whiteTexture;
                    _pixelSprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height),
                        new Vector2(0.5f, 0.5f), white.width);
                }

                return _pixelSprite;
            }
        }

        public Transform Group(string name, Transform parent, Vector2 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        public SpriteRenderer Sliced(string name, Transform parent, Sprite sprite, Rect rect, int order)
        {
            SpriteRenderer sr = NewRenderer(name, parent, rect.center, order);
            sr.sprite = sprite != null ? sprite : PixelSprite;
            if (sprite != null)
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = rect.size;
            }
            else
            {
                SpriteFit.Stretch(sr, rect.size);
            }

            return sr;
        }

        public SpriteRenderer Stretched(string name, Transform parent, Sprite sprite, Rect rect, int order)
        {
            SpriteRenderer sr = NewRenderer(name, parent, rect.center, order);
            sr.sprite = sprite != null ? sprite : PixelSprite;
            SpriteFit.Stretch(sr, rect.size);
            return sr;
        }

        public void Resize(SpriteRenderer sr, Rect rect)
        {
            sr.transform.localPosition = rect.center;
            if (sr.drawMode == SpriteDrawMode.Sliced)
                sr.size = rect.size;
            else
                SpriteFit.Stretch(sr, rect.size);
        }

        public TextMeshPro Text(string name, Transform parent, Vector2 localPosition, string text, float fontSize,
            Color color, int order, TextAlignmentOptions alignment, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var tmp = go.AddComponent<TextMeshPro>();
            if (_font != null)
                tmp.font = _font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.sortingOrder = order;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.rectTransform.sizeDelta = new Vector2(width, fontSize * 0.12f);
            tmp.rectTransform.pivot = new Vector2(PivotX(alignment), 0.5f);
            return tmp;
        }

        // 아래에서 점프하면 통과하는 단방향 발판(PlatformEffector2D)
        // 플레이어 바닥 판정이 쓰는 Ground 레이어에 둬야 함
        public BoxCollider2D Platform(string name, Transform parent, float xMin, float xMax, float top, bool visible)
        {
            var go = new GameObject(name) { layer = _groundLayer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3((xMin + xMax) * 0.5f, top - PlatformThickness * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(xMax - xMin, PlatformThickness);
            box.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
            _platforms.Add(box);

            if (!visible)
                return box;

            SpriteRenderer line = Stretched("Line", go.transform, null,
                new Rect(-(xMax - xMin) * 0.5f, PlatformThickness * 0.5f - 0.06f, xMax - xMin, 0.06f), 11);
            line.color = new Color(0.4f, 0.4f, 0.45f);
            return box;
        }

        private static SpriteRenderer NewRenderer(string name, Transform parent, Vector2 localPosition, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        private static float PivotX(TextAlignmentOptions alignment)
        {
            return alignment == TextAlignmentOptions.Left ? 0f : alignment == TextAlignmentOptions.Right ? 1f : 0.5f;
        }
    }
}
