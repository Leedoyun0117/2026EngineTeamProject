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
        [Tooltip("세로는 가로보다 약하게 둔다. 점프할 때 마우스가 위로 따라가지 않게 한다")]
        [SerializeField] private Vector2 gain = new Vector2(0.03f, 0.008f);

        [Header("Range (시작 위치 기준 직사각형)")]
        [SerializeField, Min(0f)] private float horizontalRange = 0.35f;
        [SerializeField, Min(0f)] private float upRange = 0.05f;
        [SerializeField, Min(0f)] private float downRange = 0.4f;

        [Header("Obstacles (좌우 범위를 이 사이로 제한)")]
        [Tooltip("마우스 왼쪽에 있는 물체(키보드)")]
        [SerializeField] private Renderer leftObstacle;
        [Tooltip("마우스 오른쪽에 있는 물체(본체)")]
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

#if UNITY_EDITOR
        // 기준점(플레이 중에는 시작 위치) 기준의 이동 범위를 사각형으로 보여 준다.
        private void OnDrawGizmosSelected()
        {
            Vector2 origin = Application.isPlaying ? _mouseOrigin : (Vector2)transform.position;
            HorizontalLimits(origin, out float left, out float right);
            var size = new Vector3(left + right, upRange + downRange, 0f);
            var center = new Vector3(origin.x + (right - left) * 0.5f, origin.y + (upRange - downRange) * 0.5f,
                transform.position.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
