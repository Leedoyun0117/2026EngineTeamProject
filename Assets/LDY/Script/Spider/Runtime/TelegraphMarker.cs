using UnityEngine;

namespace LDY.Spider {
    // 찍을 자리를 보여 준다: 고정 크기 테두리 원 안에서 채운 원이 커지고, 가득 차는 순간 찍는다. 깜빡이지 않는다.
    public sealed class TelegraphMarker : MonoBehaviour {
        private const int RingResolution = 128;
        private const float RingThickness = 0.07f;

        [SerializeField] private SpriteRenderer markerRenderer;

        private SpriteRenderer _ring;
        private float _startScale = 0.25f;
        private Vector3 _fullScale;

        public void Initialize(float blinkHz, float startScale) {
            _startScale = startScale;
            _fullScale = transform.localScale;
            markerRenderer.enabled = false;
            _ring = CreateRing();
        }

        public void Tick(float deltaTime, bool visible, Vec2 position, float growth) {
            markerRenderer.enabled = visible;
            _ring.enabled = visible;
            if (!visible)
                return;

            var world = new Vector3(position.X, position.Y, transform.position.z);
            transform.position = world;
            transform.localScale = _fullScale * Mathf.Lerp(_startScale, 1f, growth);
            _ring.transform.position = world;
            _ring.transform.localScale = _fullScale;
        }

        private void OnDestroy() {
            if (_ring == null)
                return;

            Destroy(_ring.sprite.texture);
            Destroy(_ring.sprite);
            Destroy(_ring.gameObject);
        }

        private SpriteRenderer CreateRing() {
            var fill = markerRenderer;
            var go = new GameObject("MarkerRing");
            go.transform.SetParent(transform.parent, false);
            var ring = go.AddComponent<SpriteRenderer>();
            ring.sprite = BuildRingSprite(fill.sprite.bounds.size.x);
            ring.color = new Color(fill.color.r, fill.color.g, fill.color.b, 1f);
            ring.sortingLayerID = fill.sortingLayerID;
            ring.sortingOrder = fill.sortingOrder + 1;
            ring.enabled = false;
            return ring;
        }

        private static Sprite BuildRingSprite(float worldSize) {
            var texture = new Texture2D(RingResolution, RingResolution, TextureFormat.RGBA32, false) {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[RingResolution * RingResolution];
            var center = (RingResolution - 1) * 0.5f;
            for (var y = 0; y < RingResolution; y++) {
                for (var x = 0; x < RingResolution; x++) {
                    var distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) / center;
                    var edge = Mathf.Clamp01((1f - distance) * center) * Mathf.Clamp01((distance - (1f - RingThickness)) * center);
                    pixels[y * RingResolution + x] = new Color32(255, 255, 255, (byte)(edge * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, RingResolution, RingResolution), new Vector2(0.5f, 0.5f),
                RingResolution / worldSize);
        }
    }
}
