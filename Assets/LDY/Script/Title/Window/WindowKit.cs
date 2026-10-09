using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 창 UI를 월드 오브젝트(SpriteRenderer, TextMeshPro, Collider2D)로 만드는 도우미. uGUI Canvas는 쓰지 않는다.
    // 모든 길이는 월드 유닛이고, 도트 키트 1픽셀은 Pixel(1/60)이다.
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
                Debug.LogWarning("[Window] Ground 레이어가 없어 발판이 Default 레이어에 만들어집니다.");
                _groundLayer = 0;
            }
        }

        public TitleArtSet Art { get; }

        // 이 키트로 만든 모든 발판. S로 내려갈 수 있는 대상이다.
        public IReadOnlyList<Collider2D> Platforms => _platforms;

        // 크기 1x1유닛의 흰 사각형. 스프라이트 슬롯이 비어 있을 때와 단색 선에 쓴다.
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

        // 9-slice 스프라이트를 rect 크기로 그린다. rect는 부모 로컬 좌표.
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

        // 일반 스프라이트를 rect 크기로 늘려 그린다. sprite가 null이면 color로 칠한 사각형이 된다.
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

        // 위쪽 면이 top에 오는 얇은 발판. 아래에서 점프하면 통과하고 위에서는 선다 (PlatformEffector2D 단방향).
        // 플레이어의 바닥 판정이 쓰는 Ground 레이어에 둔다.
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
