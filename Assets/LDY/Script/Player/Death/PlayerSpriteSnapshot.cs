using UnityEngine;

namespace LDY.Script {
    // 플레이어가 파괴된 뒤에도 대역을 같은 자리·방향·정렬로 그릴 수 있게 보이던 스프라이트를 값으로 남긴다.
    internal readonly struct PlayerSpriteSnapshot {
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Vector3 _scale;
        private readonly Sprite _sprite;
        private readonly bool _flipX;
        private readonly bool _flipY;
        private readonly Color _color;
        private readonly Material _material;
        private readonly int _sortingLayerId;
        private readonly int _sortingOrder;

        private PlayerSpriteSnapshot(SpriteRenderer source) {
            var t = source.transform;
            _position = t.position;
            _rotation = t.rotation;
            _scale = t.lossyScale;
            _sprite = source.sprite;
            _flipX = source.flipX;
            _flipY = source.flipY;
            _color = source.color;
            _material = source.sharedMaterial;
            _sortingLayerId = source.sortingLayerID;
            _sortingOrder = source.sortingOrder;
        }

        private PlayerSpriteSnapshot(Transform fallback) : this() {
            _position = fallback.position;
            _rotation = fallback.rotation;
            _scale = Vector3.one;
            _color = Color.white;
        }

        public static PlayerSpriteSnapshot Capture(SpriteRenderer source, Transform fallback) =>
            source != null ? new PlayerSpriteSnapshot(source) : new PlayerSpriteSnapshot(fallback);

        public SpriteRenderer CreateRenderer(string name) {
            var go = new GameObject(name);
            go.transform.SetPositionAndRotation(_position, _rotation);
            go.transform.localScale = _scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _sprite;
            renderer.flipX = _flipX;
            renderer.flipY = _flipY;
            renderer.color = _color;
            if (_material != null)
                renderer.sharedMaterial = _material;
            renderer.sortingLayerID = _sortingLayerId;
            renderer.sortingOrder = _sortingOrder;
            return renderer;
        }
    }
}
