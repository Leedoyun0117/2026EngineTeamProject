using UnityEngine;

namespace LDY.Spider {
    public sealed class MouseTrapView : MonoBehaviour {
        [SerializeField] private SpriteRenderer trapRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite snapSprite;

        private void Awake() {
            trapRenderer.sprite = openSprite;
        }

        public void PlaySnap() {
            trapRenderer.sprite = snapSprite;
        }
    }
}
