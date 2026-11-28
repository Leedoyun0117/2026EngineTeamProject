using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 월드 좌표를 따라가는 캔버스 요소. 따라갈 점은 이 RectTransform의 pivot에 놓이고, 간격은 프리팹의 자식 배치가 정한다.
    [RequireComponent(typeof(RectTransform))]
    public sealed class UiWorldAnchor : MonoBehaviour {
        private RectTransform _rect;
        private RectTransform _parent;
        private Camera _camera;

        public void ShowAt(float worldX, float worldY) {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            var camera = WorldCamera();
            if (camera == null || Parent == null)
                return;

            var screen = camera.WorldToScreenPoint(new Vector3(worldX, worldY, 0f));
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Parent, screen, null, out var local))
                return;

            Rect.localPosition = new Vector3(PixelSnap.Snap(local.x), PixelSnap.Snap(local.y), 0f);
        }

        public void ShowAbove(HudRect bounds) => ShowAt(bounds.CenterX, bounds.Top);

        public void Hide() {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;
        private RectTransform Parent => _parent != null ? _parent : _parent = transform.parent as RectTransform;

        private Camera WorldCamera() {
            if (_camera == null || !_camera.isActiveAndEnabled)
                _camera = Camera.main;
            return _camera;
        }
    }
}
