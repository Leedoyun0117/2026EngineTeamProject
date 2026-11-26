using System;
using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Flow {
    // 자식 콜라이더로 들어와도 Rigidbody 쪽까지 보고, 태그 또는 레이어로 플레이어를 가린다.
    [Serializable]
    public sealed class PlayerMatch {
        public const string DefaultTag = PrimaryPlayerLocator.Tag;
        private const int Unresolved = -2;

        [SerializeField] private string playerTag = DefaultTag;
        [SerializeField] private string playerLayer = DefaultTag;

        [NonSerialized] private int _layer = Unresolved;

        public bool Matches(Collider2D other) {
            if (other == null) return false;
            if (HasTag(other.gameObject)) return true;
            var body = other.attachedRigidbody;
            return body != null && (HasTag(body.gameObject) || body.gameObject.layer == Layer());
        }

        private bool HasTag(GameObject go) => !string.IsNullOrEmpty(playerTag) && go.CompareTag(playerTag);

        // LayerMask.NameToLayer는 필드 초기화(생성자)에서 부를 수 없어서 처음 쓸 때 찾는다.
        private int Layer() {
            if (_layer == Unresolved) _layer = string.IsNullOrEmpty(playerLayer) ? -1 : LayerMask.NameToLayer(playerLayer);
            return _layer;
        }
    }
}
