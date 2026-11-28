using System;
using DG.Tweening;
using UnityEngine;

namespace JJB.Script.MovingBlock
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingBlockMotor : MonoBehaviour
    {
        [Header("Move Settings")]
        [SerializeField] private Vector2 moveDirection = Vector2.right;
        [SerializeField] private float moveDistance = 3f;
        [SerializeField] private float moveDuration = 2f;

        [Header("Collision Settings")]
        [SerializeField] private LayerMask blockLayer = ~0;
        [SerializeField] private float skinWidth = 0.02f;

        private Rigidbody2D _rb;
        private Vector2 _dir;
        private Tween _moveTween;
        private bool _isMoving;

        private ContactFilter2D _filter;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
        
        
        public Vector2 Dir => _dir;
        public float MoveSpeed => moveDuration > 0f ? moveDistance / moveDuration : 0f;

        public event Action<Collider2D> OnObstacleDetected;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            _filter = new ContactFilter2D();
            _filter.useTriggers = false;
            _filter.SetLayerMask(blockLayer);
        }

        public void StartMoving()
        {
            _moveTween?.Kill();
            _dir = moveDirection.normalized;
            _isMoving = true;

            Move();
        }

        private void Move()
        {
            if (!_isMoving ||
                _dir == Vector2.zero ||
                moveDistance <= 0f ||
                moveDuration <= 0f)
                return;

            Vector2 startPos = _rb.position;
            Vector2 targetPos = startPos + _dir * moveDistance;

            float progress = 0f;

            _moveTween = DOTween.To(
                    () => progress,
                    value =>
                    {
                        progress = value;

                        Vector2 desiredPosition = Vector2.Lerp(
                            startPos,
                            targetPos,
                            progress
                        );

                        MoveSafely(desiredPosition);
                    },
                    1f,
                    moveDuration
                )
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Fixed)
                .OnComplete(Move);
        }

        private void MoveSafely(Vector2 desiredPos)
        {
            Vector2 curPos = _rb.position;
            Vector2 delta = desiredPos - curPos;

            float distance = delta.magnitude;

            if (distance <= 0.0001f)
                return;

            Vector2 dir = delta.normalized;
            int hitCount = _rb.Cast(dir, _filter, _hits, distance + skinWidth);
            float nearestDistance = distance + skinWidth;
            Collider2D nearestCol = null;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _hits[i];

                if (hit.collider == null || hit.rigidbody == _rb)
                    continue;

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestCol = hit.collider;
                }
            }

            float allowedDistance = distance;

            if (nearestCol != null)
            {
                allowedDistance = Mathf.Clamp(
                    nearestDistance - skinWidth,
                    0f,
                    distance
                );
            }

            OnObstacleDetected?.Invoke(nearestCol);

            _rb.MovePosition(
                curPos + dir * allowedDistance
            );
        }

        public void ReverseDirection()
        {
            if (!_isMoving)
                return;

            _dir *= -1f;

            _moveTween?.Kill();

            Move();
        }

        public void StopMoving()
        {
            _isMoving = false;

            _moveTween?.Kill();
            _moveTween = null;

            OnObstacleDetected?.Invoke(null);
        }

        private void OnDisable()
        {
            StopMoving();
        }
    }
}