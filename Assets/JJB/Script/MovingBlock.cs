using System;
using DG.Tweening;
using JJB.Script;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private Vector2 moveDir = Vector2.up;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveDuration = 2f;
    
    [Header("Collision Settings")]
    [SerializeField] private float skinWidth = 0.02f;
    
    private Rigidbody2D _rb;
    private PushableBlock _pushBlock;
    
    private Vector2 _currentDirection;
    private Tween _moveTween;
    
    private ContactFilter2D _castFilter;
    private readonly RaycastHit2D[] _castHits = new RaycastHit2D[16];
    
    private float MoveSpeed => moveDuration > 0f ? moveDistance / moveDuration : 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _castFilter = new ContactFilter2D();
        _castFilter.useTriggers = false;
        _castFilter.useLayerMask = false;
    }

    private void OnEnable()
    {
        _currentDirection = moveDir.normalized;
        Move();
    }

    private void Move()
    {
        if (_currentDirection == Vector2.zero ||
            moveDistance <= 0f ||
            moveDuration <= 0f)
            return;

        Vector2 startPos = _rb.position;
        Vector2 targetPos = startPos + _currentDirection * moveDistance;
        
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Reverse"))
        {
            ReverseDirection();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPushBlock(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckPushBlock(collision);
    }
    
    private void CheckPushBlock(Collision2D collision)
    {
        PushableBlock block =
            collision.collider.GetComponentInParent<PushableBlock>();
        
        if (block == null || !block.CompareTag("PushBlock"))
            return;

        Vector2 toBlock = block.Rb.worldCenterOfMass - _rb.worldCenterOfMass;
        float dot = Vector2.Dot(toBlock, _currentDirection);
        
        if (dot <= 0f)
            return;
        
        if (Vector2.Dot(toBlock, _currentDirection) <= 0f)
            return;

        if (_pushBlock != null && _pushBlock != block)
        {
            _pushBlock.ReleasePush(this);
        }

        _pushBlock = block;
        block.ReceivePush(this, _currentDirection, MoveSpeed);
        
        if (block.IsBlocked)
        {
            ReverseDirection();
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        PushableBlock block = collision.collider.GetComponentInParent<PushableBlock>();

        if (block == null || block != _pushBlock)
            return;

        block.ReleasePush(this);
        _pushBlock = null;
    }

    private void ReverseDirection()
    {
        ReleaseCurrentBlock();
        _currentDirection *= -1f;
        _moveTween?.Kill();
        Move();
    }
    
    private void ReleaseCurrentBlock()
    {
        if (_pushBlock == null)
            return;

        _pushBlock.ReleasePush(this);
        _pushBlock = null;
    }
    
    private void OnDisable()
    {
        _moveTween?.Kill();
        ReleaseCurrentBlock();
    }
}
