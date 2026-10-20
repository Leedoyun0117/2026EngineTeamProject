using System;
using UnityEngine;

namespace LDY.Script
{
    // 모니터 안 플레이어의 이동에 비례해 책상 위 마우스를 조금씩 움직인다.
    public class DeskMouseFollower : MonoBehaviour, IDeskMouse
    {
        [SerializeField] private TitleClock clock;
        [SerializeField] private Transform source;
        [Header("Gain (플레이어 이동량에 곱함)")]
        [SerializeField] private Vector2 gain = new Vector2(0.03f, 0.008f);

        [Header("Range (시작 위치 기준 직사각형)")]
        [SerializeField, Min(0f)] private float horizontalRange = 0.35f;
        [SerializeField, Min(0f)] private float upRange = 0.05f;
        [SerializeField, Min(0f)] private float downRange = 0.4f;

        [Header("Obstacles (좌우 범위를 이 사이로 제한)")]
        [SerializeField] private Renderer leftObstacle;
        [SerializeField] private Renderer rightObstacle;
        [SerializeField, Min(0f)] private float obstacleMargin = 0.1f;

        [SerializeField, Min(0.1f)] private float smoothing = 10f;

        private Vector2 _sourceOrigin;
        private Vector2 _mouseOrigin;
        private float _left;
        private float _right;

        public Vector2 Position => transform.position;
        public event Action<Vector2> Moved;

        private void Start()
        {
            _sourceOrigin = source.position;
            _mouseOrigin = transform.position;
            HorizontalLimits(_mouseOrigin, out _left, out _right);
        }

        // 기준점에서 왼쪽/오른쪽으로 움직일 수 있는 거리. 설정 범위와 장애물까지의 여유 중 작은 값이다.
        private void HorizontalLimits(Vector2 origin, out float left, out float right)
        {
            left = horizontalRange;
            right = horizontalRange;
            float half = GetComponent<Renderer>().bounds.extents.x;
            if (leftObstacle != null)
                left = Mathf.Clamp(origin.x - half - leftObstacle.bounds.max.x - obstacleMargin, 0f, left);
            if (rightObstacle != null)
                right = Mathf.Clamp(rightObstacle.bounds.min.x - (origin.x + half) - obstacleMargin, 0f, right);
        }

        private void Update()
        {
            Vector2 delta = (Vector2)source.position - _sourceOrigin;
            Vector2 offset = new Vector2(
                Mathf.Clamp(delta.x * gain.x, -_left, _right),
                Mathf.Clamp(delta.y * gain.y, -downRange, upRange));

            float blend = 1f - Mathf.Exp(-smoothing * clock.Delta);
            Vector2 next = Vector2.Lerp(transform.position, _mouseOrigin + offset, blend);
            if ((next - (Vector2)transform.position).sqrMagnitude < 1e-10f)
                return;

            transform.position = new Vector3(next.x, next.y, transform.position.z);
            Moved?.Invoke(next);
        }
    }
}
