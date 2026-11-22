using UnityEngine;

namespace LDY.Script {
    // 줄 점 목록을 LineRenderer로 그린다(픽셀 스냅 적용).
    [RequireComponent(typeof(LineRenderer))]
    public class RopeLineRenderer : MonoBehaviour {
        [SerializeField] [Min(0.01f)] private float width = 0.12f;
        [SerializeField] private Color color = new(0.55f, 0.38f, 0.2f, 1f);
        [SerializeField] [Range(0, 16)] private int cornerVertices = 4;
        [SerializeField] [Range(0, 16)] private int capVertices = 2;
        // 점 좌표를 이 해상도 격자에 맞춰 도트 느낌 유지
        [SerializeField] [Min(1f)] private float pixelsPerUnit = 16f;

        private Vector3[] _buffer;
        private LineRenderer _line;
        private IRopePose _pose;

        private void Awake() {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            // 부모의 IRopePose(RopeHangController)에서 점을 읽는다
            _pose = GetComponentInParent<IRopePose>();
            Apply();
        }

        private void OnValidate() {
            if (_line != null)
                Apply();
        }

        // 물리 갱신이 끝난 뒤 점을 읽어 선 위치 갱신
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

        // 굵기/색/모서리 설정 적용
        private void Apply() {
            _line.startWidth = width;
            _line.endWidth = width;
            _line.startColor = color;
            _line.endColor = color;
            _line.numCornerVertices = cornerVertices;
            _line.numCapVertices = capVertices;
        }

        // 좌표를 픽셀 격자에 반올림
        private Vector3 Snap(Vector2 point) {
            return new Vector3(
                Mathf.Round(point.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(point.y * pixelsPerUnit) / pixelsPerUnit,
                transform.position.z);
        }
    }
}
