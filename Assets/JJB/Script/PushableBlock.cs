
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableBlock : MonoBehaviour
{
    private struct PushRequest
    {
        public Vector2 Direction;
        public float Speed;
    }

    [Header("Push Settings")]
    [SerializeField] private float push = 40f;
    [SerializeField] private float blockedDuration = 0.15f;
    [SerializeField] private float moveSpeed = 0.03f;
    [SerializeField] private float blockedSpeedRatio = 0.1f;

    private Rigidbody2D _rb;

    // 여러 발판의 요청을 동시에 저장
    private readonly Dictionary<MovingBlock, PushRequest> _push = new();
    private readonly Dictionary<MovingBlock, float> _blockedTimers = new();
    private readonly HashSet<MovingBlock> _blockConflict = new();

    private Vector2 _prevPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _prevPos = _rb.position;
    }

    public void ReceivePush(MovingBlock source, Vector2 direction, float speed)
    {
        if (source == null || direction.sqrMagnitude < 0.0001f)
            return;

        _push[source] = new PushRequest
        {
            Direction = direction.normalized,
            Speed = speed
        };

        if (!_blockedTimers.ContainsKey(source))
            _blockedTimers.Add(source, 0f);
    }

    public bool IsBlockedFor(MovingBlock source)
    {
        if (source == null || !_push.ContainsKey(source))
            return false;

        if (_blockConflict.Contains(source))
            return true;

        return _blockedTimers.TryGetValue(source, out float timer)
               && timer >= blockedDuration;
    }

    public void ReleasePush(MovingBlock source)
    {
        if (source == null)
            return;

        _push.Remove(source);
        _blockedTimers.Remove(source);
        _blockConflict.Remove(source);
    }

    private void FixedUpdate()
    {
        Vector2 currentPos = _rb.position;
        Vector2 velocity = (currentPos - _prevPos) / Time.fixedDeltaTime;

        _prevPos = currentPos;

        if (_push.Count == 0)
            return;

        Vector2 totalPush = Vector2.zero;

        foreach (var request in _push.Values)
        {
            totalPush += request.Direction * request.Speed;
        }

        foreach (var pair in _push)
        {
            MovingBlock source = pair.Key;
            PushRequest request = pair.Value;

            float netSpeed = Vector2.Dot(totalPush, request.Direction);

            if (netSpeed <= 0.001f)
            {
                _blockConflict.Add(source);
            }

            float actualSpeed = Vector2.Dot(velocity, request.Direction);
            float threshold = Mathf.Max(moveSpeed, request.Speed * blockedSpeedRatio);

            if (actualSpeed < threshold)
            {
                _blockedTimers[source] += Time.fixedDeltaTime;
            }
            else
            {
                _blockedTimers[source] = 0f;
            }
        }
        
        if (totalPush.sqrMagnitude < 0.0001f)
        {
            Vector2 pushAxis = default;

            foreach (var request in _push.Values)
            {
                pushAxis = request.Direction;
                break;
            }

            float axisSpeed = Vector2.Dot(_rb.linearVelocity, pushAxis);
            _rb.linearVelocity -= pushAxis * axisSpeed;
            return;
        }

        Vector2 desiredVelocity = totalPush / _push.Count;
        Vector2 pushDirection = desiredVelocity.normalized;

        float currentSpeed = Vector2.Dot(_rb.linearVelocity, pushDirection);

        Vector2 otherVelocity = _rb.linearVelocity - pushDirection * currentSpeed;

        float nextSpeed = Mathf.MoveTowards(
            currentSpeed,
            desiredVelocity.magnitude,
            push * Time.fixedDeltaTime
        );

        _rb.linearVelocity = otherVelocity + pushDirection * nextSpeed;
    }
}