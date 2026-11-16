using UnityEngine;

namespace LDY.Script {
    [RequireComponent(typeof(EdgeCollider2D))]
    public class RopeColliderFollower : MonoBehaviour {
        private EdgeCollider2D _edge;
        private Vector2[] _local;
        private IRopePose _pose;

        private void Awake() {
            _edge = GetComponent<EdgeCollider2D>();
            _edge.isTrigger = true;
            _pose = GetComponent<IRopePose>();
        }

        private void FixedUpdate() {
            var points = _pose?.Points;
            if (points == null || points.Count < 2)
                return;

            if (_local == null || _local.Length != points.Count)
                _local = new Vector2[points.Count];

            for (var i = 0; i < _local.Length; i++)
                _local[i] = transform.InverseTransformPoint(points[i]);

            _edge.points = _local;
        }
    }
}
