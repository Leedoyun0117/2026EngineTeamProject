using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud {
    public sealed class ImageFrameAnimator : FrameAnimator {
        [SerializeField] private Image image;

        protected override void SetSprite(Sprite sprite) {
            if (image != null)
                image.sprite = sprite;
        }
    }
}
