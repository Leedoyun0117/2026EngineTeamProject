using UnityEngine;

namespace LDY.Script
{
    // 대상 위에 씌우는 테두리. 창에 묶이지 않으므로 타이틀 메뉴나 HUD에서도 부모에 만들어 쓴다
    // 사각형은 부모 로컬(Show), 월드(ShowWorld), Renderer.bounds를 매 프레임 따라가는 방식(Follow) 중 고른다
    public sealed class FocusFrame : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Renderer _followed;
        private Rect _shown;
        private bool _visible;
        private float _padding;

        public static FocusFrame Create(Transform parent, FocusFrameStyle style)
        {
            var go = new GameObject("FocusFrame");
            go.transform.SetParent(parent, false);
            var frame = go.AddComponent<FocusFrame>();
            frame.Apply(style);
            go.SetActive(false);
            return frame;
        }

        // 에셋 없이 스프라이트와 정렬 순서만 아는 호출자용. 값은 만들 때 복사하므로 임시 스타일은 바로 지운다
        public static FocusFrame Create(Transform parent, Sprite sprite, int sortingOrder)
        {
            FocusFrameStyle style = FocusFrameStyle.CreateDefault(sprite, sortingOrder);
            FocusFrame frame = Create(parent, style);
            Destroy(style);
            return frame;
        }

        // 부모 로컬 좌표
        public void Show(Rect local)
        {
            _followed = null;
            ShowLocal(Pad(local));
        }

        // 월드 좌표. 부모에 스케일이 있어도 월드에서 같은 크기로 보이게 변환한다
        public void ShowWorld(Rect world)
        {
            _followed = null;
            ShowLocal(ToLocal(Pad(world)));
        }

        // 대상 Renderer의 bounds를 LateUpdate마다 따라간다. 대상이 사라지면 숨긴다
        public void Follow(Renderer target)
        {
            _followed = target;
            if (target == null)
            {
                Hide();
                return;
            }

            ShowLocal(ToLocal(Pad(BoundsRect(target))));
        }

        // 파괴하지 않고 숨김. 다시 Show 계열을 부르면 재사용
        public void Hide()
        {
            _followed = null;
            if (!_visible)
                return;

            _visible = false;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_followed == null)
                return;

            ShowLocal(ToLocal(Pad(BoundsRect(_followed))));
        }

        private void Apply(FocusFrameStyle style)
        {
            _padding = style.padding;
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = style.sortingOrder;
            _renderer.color = style.tint;
            // 스프라이트 슬롯이 비었으면 WindowKit과 같은 1x1 흰 사각형을 늘려 씀
            _renderer.sprite = style.sprite != null ? style.sprite : WindowKit.PixelSprite;
            if (style.sprite != null)
            {
                _renderer.drawMode = SpriteDrawMode.Sliced;
                _renderer.tileMode = style.tileMode;
            }
        }

        private void ShowLocal(Rect local)
        {
            if (_visible && _shown == local)
                return;

            _visible = true;
            _shown = local;
            gameObject.SetActive(true);
            transform.localPosition = local.center;
            if (_renderer.drawMode == SpriteDrawMode.Sliced)
                _renderer.size = local.size;
            else
                SpriteFit.Stretch(_renderer, local.size);
        }

        private Rect Pad(Rect rect)
        {
            return new Rect(rect.xMin - _padding, rect.yMin - _padding, rect.width + _padding * 2f,
                rect.height + _padding * 2f);
        }

        private Rect ToLocal(Rect world)
        {
            Transform parent = transform.parent;
            if (parent == null)
                return world;

            Vector3 center = parent.InverseTransformPoint(new Vector3(world.center.x, world.center.y, parent.position.z));
            Vector3 scale = parent.lossyScale;
            var size = new Vector2(world.width / Mathf.Max(TitleTuning.MinScale, Mathf.Abs(scale.x)),
                world.height / Mathf.Max(TitleTuning.MinScale, Mathf.Abs(scale.y)));
            return new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
        }

        private static Rect BoundsRect(Renderer target)
        {
            Bounds bounds = target.bounds;
            return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }
    }
}
