using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // A(지빈) 프리팹을 읽어 JJBLevel 기준 월드 값을 만든다. GroundCheck는 JJBLevel 오버라이드 값을 쓴다.
    internal sealed class JibinReference {
        private JibinReference() { }

        public Float2 ColliderOffset { get; private set; }
        public Float2 ColliderSize { get; private set; }
        public Float2 GroundCheck { get; private set; }
        public Float2 GroundCheckSize { get; private set; }
        public float VisualHeight { get; private set; }

        public static JibinReference Load(SpriteRows rows) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Jibin);
            if (asset == null)
                return null;

            var probe = new PlayerProbe(asset);
            var scale = asset.transform.localScale.y;
            var reference = new JibinReference();
            if (probe.TryCollider(out var offset, out var size)) {
                reference.ColliderOffset = offset;
                reference.ColliderSize = size;
            }

            var point = asset.transform.Find(PlayerParts.GroundCheck);
            var local = new Float2(point != null ? point.localPosition.x : 0f, PlayerParts.JibinGroundCheckLocalY);
            reference.GroundCheck = RootScaleConversion.ToWorld(local, scale);
            if (probe.TryGroundCheck(out _, out var boxSize))
                reference.GroundCheckSize = boxSize;

            var sprite = asset.transform.Find(PlayerParts.LegacyVisual)?.GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
                reference.VisualHeight = VisualScale.WorldHeight(rows.Of(sprite.sprite).Height, sprite.sprite.pixelsPerUnit,
                    sprite.transform.lossyScale.y);

            return reference;
        }
    }

    internal sealed class SpriteRows {
        private readonly System.Collections.Generic.Dictionary<Sprite, OpaqueRows> _cache = new();

        public OpaqueRows Of(Sprite sprite) {
            if (!_cache.TryGetValue(sprite, out var rows))
                _cache[sprite] = rows = SpriteFeetProbe.Measure(sprite);

            return rows;
        }
    }
}
