using UnityEngine;

namespace LDY.Script
{
    public class FocusFrameView
    {
        private readonly SpriteRenderer _renderer;

        public FocusFrameView(Transform parent, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject("TransitionFocusFrame");
            go.transform.SetParent(parent, false);
            _renderer = go.AddComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.drawMode = SpriteDrawMode.Sliced;
            _renderer.tileMode = SpriteTileMode.Continuous;
            _renderer.sortingOrder = sortingOrder;
            _renderer.enabled = false;
        }

        public void SetRect(Rect world)
        {
            _renderer.enabled = true;
            _renderer.transform.position = new Vector3(world.center.x, world.center.y, 0f);
            _renderer.size = world.size;
        }

        public void Hide()
        {
            if (_renderer != null)
                Object.Destroy(_renderer.gameObject);
        }
    }
}
