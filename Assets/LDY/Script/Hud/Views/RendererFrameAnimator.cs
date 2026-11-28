using UnityEngine;

namespace LDY.Hud {
    public sealed class RendererFrameAnimator : FrameAnimator {
        [SerializeField] private SpriteRenderer target;

        protected override void SetSprite(Sprite sprite) {
            if (target != null)
                target.sprite = sprite;
        }
    }
}
