using System.Collections.Generic;
using UnityEngine;

namespace JJB.Script.MovingBlock
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableBlock : MonoBehaviour
    {
        [Header("Push Settings")]
        [SerializeField] private float push = 40f;
        [SerializeField] private float blockDuration = 0.15f;
        [SerializeField] private float moveSpeed = 0.03f;
        [SerializeField] private float blockSpeedRatio = 0.1f;

        private Rigidbody2D _rb;

        private readonly Dictionary<IPushSource, PushState> _pushers = new();
        private readonly List<IPushSource> _notifyList = new();
        private readonly List<IPushSource> _removeList = new();

        private Vector2 _prevPos;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _prevPos = _rb.position;
        }
        
        public void ReceivePush(IPushSource source, Vector2 dir, float speed)
        {
            if (source == null || dir.sqrMagnitude < 0.0001f)
                return;

            dir.Normalize();

            if (!_pushers.TryGetValue(source, out var state))
            {
                state = new PushState();
                _pushers.Add(source, state);
            }
            else if (Vector2.Dot(state.Direction, dir) < 0.999f)
            {
                state.BlockedTimer = 0f;
                state.Notified = false;
            }

            state.Direction = dir;
            state.Speed = Mathf.Max(0f, speed);
        }
        
        public void ReleasePush(IPushSource source)
        {
            if (source == null)
                return;

            if (!_pushers.Remove(source, out var state))
                return;
            
            if (_pushers.Count == 0)
            {
                var currentSpeed = Vector2.Dot(
                    _rb.linearVelocity,
                    state.Direction
                );

                if (currentSpeed > 0f)
                {
                    _rb.linearVelocity -= state.Direction * currentSpeed;
                }
            }
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            var actualVelocity = (_rb.position - _prevPos) / deltaTime;

            _prevPos = _rb.position;

            if (_pushers.Count == 0)
                return;

            _notifyList.Clear();
            _removeList.Clear();

            var totalPushVelocity = Vector2.zero;
            var count = 0;

            foreach (var pair in _pushers)
            {
                var source = pair.Key;
                var state = pair.Value;

                if (source is Object unityObject &&
                    unityObject == null)
                {
                    _removeList.Add(source);
                    continue;
                }

                totalPushVelocity += state.Direction * state.Speed;
                count++;
                
                var actualSpeed = Vector2.Dot(
                    actualVelocity,
                    state.Direction
                );

                var threshold = Mathf.Max(
                    moveSpeed,
                    state.Speed * blockSpeedRatio
                );

                if (actualSpeed < threshold)
                {
                    state.BlockedTimer += deltaTime;

                    if (state.BlockedTimer >= blockDuration &&
                        !state.Notified)
                    {
                        state.Notified = true;
                        _notifyList.Add(source);
                    }
                }
                else
                {
                    state.BlockedTimer = 0f;
                    state.Notified = false;
                }
            }

            foreach (var source in _removeList) _pushers.Remove(source);

            if (count == 0)
                return;

            var desiredVelocity =
                totalPushVelocity / count;

            ApplyPushVelocity(desiredVelocity, deltaTime);

            foreach (var source in _notifyList)
            {
                if (source != null)
                {
                    source.OnPushBlocked();
                }
            }
        }

        private void ApplyPushVelocity(Vector2 desiredVelocity, float deltaTime)
        {
            if (desiredVelocity.sqrMagnitude < 0.0001f)
            {
                if (_pushers.Count == 0)
                    return;

                var axis = Vector2.zero;

                foreach (var state in _pushers.Values)
                {
                    axis = state.Direction;
                    break;
                }

                ApplyVelocityOnAxis(axis, 0f, deltaTime);
                return;
            }

            var direction = desiredVelocity.normalized;

            ApplyVelocityOnAxis(
                direction,
                desiredVelocity.magnitude,
                deltaTime
            );
        }

        private void ApplyVelocityOnAxis(Vector2 dir, float targetSpeed, float deltaTime)
        {
            var currentSpeed = Vector2.Dot(
                _rb.linearVelocity,
                dir
            );

            var nextSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                push * deltaTime
            );

            _rb.linearVelocity += dir * (nextSpeed - currentSpeed);
        }
    }
}