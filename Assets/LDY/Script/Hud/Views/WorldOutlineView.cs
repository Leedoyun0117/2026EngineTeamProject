using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 대상 경계를 감싸는 점선(월드 SpriteRenderer, Tiled). 위치와 크기를 스프라이트 픽셀 격자에 맞춘다.
    public sealed class WorldOutlineView : MonoBehaviour {
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] [Min(0f)] private float padding;

        public void Show(HudRect bounds) {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            if (outline == null || outline.sprite == null)
                return;

            var pixel = 1f / outline.sprite.pixelsPerUnit;
            outline.size = new Vector2(PixelSnap.Snap(bounds.Width + padding * 2f, pixel),
                PixelSnap.Snap(bounds.Height + padding * 2f, pixel));
            transform.position = new Vector3(PixelSnap.Snap(bounds.CenterX, pixel),
                PixelSnap.Snap(bounds.CenterY, pixel), transform.position.z);
        }

        public void Hide() {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
