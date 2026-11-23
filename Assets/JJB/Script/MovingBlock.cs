using DG.Tweening;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private Vector2 moveDirection = Vector2.up;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveDuration = 2f;
    
    private Vector2 _currentDirection;
    private Tween _moveTween;
    
    private void OnEnable()
    {
        _currentDirection = moveDirection.normalized;
        Move();
    }

    private void Move()
    {
        if (_currentDirection == Vector2.zero)
            return;

        Vector3 targetPosition =
            transform.position + (Vector3)(_currentDirection * moveDistance);

        _moveTween = transform.DOMove(targetPosition, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(Move);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Reverse"))
        {
            ReverseDirection();
        }
    }

    private void ReverseDirection()
    {
        _currentDirection *= -1f;
        _moveTween?.Kill();
        Move();
    }
    
    private void OnDisable()
    {
        _moveTween?.Kill();
    }
}
