using UnityEngine;

namespace LDY.Script {
    [RequireComponent(typeof(LineRenderer))]
    public class RopeLineRenderer : MonoBehaviour {
        [SerializeField] [Min(0.01f)] private float width = 0.12f;
        [SerializeField] private Color color = new(0.55f, 0.38f, 0.2f, 1f);
        [SerializeField] [Range(0, 16)] private int cornerVertices = 4;
        [SerializeField] [Range(0, 16)] private int capVertices = 2;
        [SerializeField] [Min(1f)] private float pixelsPerUnit = 16f;

        private Vector3[] _buffer;
        private LineRenderer _line;
        private IRopePose _pose;

        private void Awake() {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _pose = GetComponentInParent<IRopePose>();
            Apply();
        }

        private void OnValidate() {
            if (_line != null)
                Apply();
        }

        private void LateUpdate() {
            var points = _pose?.Points;
            if (points == null || points.Count == 0)
                return;

            if (_buffer == null || _buffer.Length != points.Count) {
                _buffer = new Vector3[points.Count];
                _line.positionCount = points.Count;
            }

            for (var i = 0; i < points.Count; i++)
                _buffer[i] = Snap(points[i]);

            _line.SetPositions(_buffer);
        }

        private void Apply() {
            _line.startWidth = width;
            _line.endWidth = width;
            _line.startColor = color;
            _line.endColor = color;
            _line.numCornerVertices = cornerVertices;
            _line.numCapVertices = capVertices;
        }

        private Vector3 Snap(Vector2 point) {
            return new Vector3(
                Mathf.Round(point.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(point.y * pixelsPerUnit) / pixelsPerUnit,
                transform.position.z);
        }
    }
}
