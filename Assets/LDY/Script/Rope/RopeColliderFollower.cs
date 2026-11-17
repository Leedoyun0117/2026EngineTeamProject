using UnityEngine;

namespace LDY.Script {
    // 줄 점 목록을 EdgeCollider2D에 복사해 콜라이더가 줄 모양을 따라가게 한다. JJB 상호작용이 줄을 '찾는' 용도(트리거)이고 플레이어 위치 계산에는 쓰이지 않는다.
    [RequireComponent(typeof(EdgeCollider2D))]
    public class RopeColliderFollower : MonoBehaviour {
        private EdgeCollider2D _edge;
        private Vector2[] _local;
        private IRopePose _pose;

        private void Awake() {
            _edge = GetComponent<EdgeCollider2D>();
            // 물리 충돌 없이 감지만 하도록 트리거로 둔다.
            _edge.isTrigger = true;
            _pose = GetComponent<IRopePose>();
        }

        private void FixedUpdate() {
            // 점이 2개 미만이면 선분이 안 만들어지므로 건너뜀
            var points = _pose?.Points;
            if (points == null || points.Count < 2)
                return;

            if (_local == null || _local.Length != points.Count)
                _local = new Vector2[points.Count];

            for (var i = 0; i < _local.Length; i++)
                // 줄 점은 월드 좌표이고 콜라이더 points는 로컬 좌표라 변환이 필요하다.
                _local[i] = transform.InverseTransformPoint(points[i]);

            _edge.points = _local;
        }
    }
}
