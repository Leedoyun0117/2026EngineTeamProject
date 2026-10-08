using UnityEngine;

namespace LDY.Script
{
    public class SpriteAfterimageSpawner : IAfterimageSpawner
    {
        private readonly SpriteRenderer _source;
        private readonly Color _tint;

        public SpriteAfterimageSpawner(SpriteRenderer source, Color tint)
        {
            _source = source;
            _tint = tint;
        }

        // 스프라이트만 복제하므로 콜라이더와 스크립트는 포함되지 않는다.
        public IAfterimage Spawn()
        {
            Transform src = _source.transform;
            var go = new GameObject("Afterimage");
            go.transform.SetPositionAndRotation(src.position, src.rotation);
            go.transform.localScale = src.lossyScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _source.sprite;
            sr.flipX = _source.flipX;
            sr.flipY = _source.flipY;
            sr.sharedMaterial = _source.sharedMaterial;
            sr.sortingLayerID = _source.sortingLayerID;
            sr.sortingOrder = _source.sortingOrder - 1;
            sr.color = _tint;

            return new Handle(go, sr, _tint);
        }

        private sealed class Handle : IAfterimage
        {
            private GameObject _go;
            private readonly SpriteRenderer _renderer;
            private readonly Color _baseColor;

            public Handle(GameObject go, SpriteRenderer renderer, Color baseColor)
            {
                _go = go;
                _renderer = renderer;
                _baseColor = baseColor;
            }

            public void SetFade(float visibility)
            {
                if (_renderer == null)
                    return;

                Color c = _baseColor;
                c.a *= visibility;
                _renderer.color = c;
            }

            public void Dispose()
            {
                if (_go == null)
                    return;

                Object.Destroy(_go);
                _go = null;
            }
        }
    }
}
