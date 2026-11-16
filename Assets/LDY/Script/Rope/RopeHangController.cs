using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    public interface IRopePose {
        IReadOnlyList<Vector2> Points { get; }
    }

    public interface IRopeGrabber {
        bool TryGrab();
    }

    [DisallowMultipleComponent]
    public class RopeHangController : MonoBehaviour, IRopePose, IRopeGrabber {
        private const float ClimbPoseThreshold = 0.1f;

        [SerializeField] private Transform anchor;
        [SerializeField] private RopeSettings settings;
        [SerializeField] private Rigidbody2D player;

        private IPlayerBody _body;
        private ReleaseGate _gate;
        private SuspendRequestHub _hub;
        private bool _hanging;
        private RopeInput _input;
        private ClimbSway _sway;
        private Pendulum _pendulum;
        private IPlayerPoseSink _pose;
        private RopeSimulation _rope;
        private HoldSlot _slot;
        private PendulumState _state;

        public IReadOnlyList<Vector2> Points => _rope?.Points;

        private Vector2 Anchor => anchor != null ? (Vector2)anchor.position : (Vector2)transform.position;

        private void Awake() {
            if (player == null) {
                var found = GameObject.FindWithTag("Player");
                player = found != null ? found.GetComponent<Rigidbody2D>() : null;
            }

            if (player == null || settings == null) {
                Debug.LogError($"{nameof(RopeHangController)}: player 또는 settings가 없습니다.", this);
                enabled = false;
                return;
            }

            _pendulum = new Pendulum(settings.Tuning);
            _sway = new ClimbSway(settings.ClimbSway);
            _gate = new ReleaseGate(settings.ReleaseInputDelay);
            _rope = new RopeSimulation(settings.Curve, settings.Damping);
            _rope.Initialize(Anchor);
            _body = new Rigidbody2DPlayerBody(player);
            _hub = PlayerSuspendHub.GetOrAdd(player.gameObject).Hub;
            _slot = PlayerRopeSlot.GetOrAdd(player.gameObject).Slot;
            _pose = player.GetComponent<IPlayerPoseSink>();
            _input = new RopeInput();
        }

        private void OnEnable() {
            _input?.Enable();
        }

        private void OnDisable() {
            ForceRelease();
            _input?.Disable();
        }

        private void OnDestroy() {
            ForceRelease();
            _input?.Dispose();
        }

        private void Update() {
            if (Time.timeScale <= 0f)
                _input.Clear();
        }

        public bool TryGrab() {
            if (_rope == null || _hanging || !isActiveAndEnabled)
                return false;

            if (!_slot.TryAcquire(this, Time.time))
                return false;

            var velocity = _body.Velocity;
            var grabPoint = _rope.PrepareGrab(_body.Position);
            var requested = false;
            try {
                _hub.Request(this);
                requested = true;
            }
            finally {
                if (!requested)
                    _slot.Release(this, Time.time, 0f);
            }

            _state = _pendulum.CreateState(Anchor, grabPoint, velocity);
            _sway.Reset();
            _gate.MarkGrab(Time.frameCount, Time.realtimeSinceStartup);
            _hanging = true;
            return true;
        }

        private void FixedUpdate() {
            var deltaTime = Time.fixedDeltaTime;
            var released = ReadRelease(_input.TryConsumeRelease(out var releaseFrame), releaseFrame);
            var jumped = ReadRelease(_input.TryConsumeJump(out var jumpFrame), jumpFrame);

            if (_hanging && (released || jumped))
                Release(jumped);

            if (_hanging)
                Hang(deltaTime);
            else
                _rope.Step(Anchor, deltaTime);
        }

        private bool ReadRelease(bool pressed, int pressFrame) {
            return _hanging && pressed && _gate.Accepts(pressFrame, Time.realtimeSinceStartup);
        }

        private void Hang(float deltaTime) {
            var axis = _input.Axis;
            var swing = axis.x + _sway.Step(axis.y, deltaTime);
            _state = _pendulum.Step(_state, new PendulumInput(swing, axis.y), deltaTime);
            var position = Pendulum.PositionOf(Anchor, _state);
            _body.Position = position;
            _body.Velocity = Vector2.zero;
            _rope.StepHeld(Anchor, _state.Length, position, deltaTime);
            _pose?.ReportRope(true, ToRowMove(axis.y));
        }

        private void Release(bool jump) {
            var velocity = ReleaseVelocity.Compute(_state, jump, settings.JumpReleaseBonus);
            _hanging = false;
            _rope.Release();
            try {
                _hub.Release(this);
            }
            finally {
                _slot.Release(this, Time.time, settings.RegrabCooldown);
                _pose?.ReportRope(false, RowMoveDirection.None);
                _body.Velocity = velocity;
            }
        }

        private void ForceRelease() {
            if (!_hanging)
                return;

            _hanging = false;
            _rope.Release();
            try {
                _hub.Release(this);
            }
            finally {
                _slot.Release(this, Time.time, settings.RegrabCooldown);
                _pose?.ReportRope(false, RowMoveDirection.None);
            }
        }

        private static RowMoveDirection ToRowMove(float vertical) {
            if (vertical > ClimbPoseThreshold)
                return RowMoveDirection.Up;

            return vertical < -ClimbPoseThreshold ? RowMoveDirection.Down : RowMoveDirection.None;
        }
    }
}
