using UnityEngine;

namespace LDY.Spider {
    public sealed class TelegraphMarker : MonoBehaviour {
        [SerializeField] private SpriteRenderer markerRenderer;

        private float _blinkHz = 6f;
        private float _startScale = 0.25f;
        private Vector3 _fullScale;
        private float _time;

        public void Initialize(float blinkHz, float startScale) {
            _blinkHz = blinkHz;
            _startScale = startScale;
            _fullScale = transform.localScale;
            markerRenderer.enabled = false;
        }

        public void Tick(float deltaTime, bool visible, Vec2 position, float growth) {
            if (!visible) {
                markerRenderer.enabled = false;
                return;
            }

            _time += deltaTime;
            transform.position = new Vector3(position.X, position.Y, transform.position.z);
            transform.localScale = _fullScale * Mathf.Lerp(_startScale, 1f, growth);
            markerRenderer.enabled = Mathf.FloorToInt(_time * _blinkHz * 2f) % 2 == 0;
        }
    }
}
