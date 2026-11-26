using System.Collections.Generic;
using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Script {
    // 줄 점 목록을 읽기 전용으로 노출. 렌더러/콜라이더가 컨트롤러 전체를 몰라도 된다.
    public interface IRopePose {
        IReadOnlyList<Vector2> Points { get; }
    }

    // 잡기 요청을 받는 쪽. RopeInteractable이 JJB 상호작용에서 호출한다.
    public interface IRopeGrabber {
        bool TryGrab();
    }

    // 줄 매달림의 조립 담당. 입력 -> 진자(플레이어 움직임) -> 줄 시뮬레이션 -> 플레이어 몸 순서만 지휘하고, 계산은 Logic 클래스에 위임한다.
    [DisallowMultipleComponent]
    public class RopeHangController : MonoBehaviour, IRopePose, IRopeGrabber {
        // 이 값보다 세로 입력이 커야 오르내림 포즈로 본다
        private const float ClimbPoseThreshold = 0.1f;

        // 줄이 매달린 고정점(없으면 이 오브젝트 위치)
        [SerializeField] private Transform anchor;
        // 모든 수치가 담긴 데이터 에셋
        [SerializeField] private RopeSettings settings;
        // 매달릴 플레이어(비워두면 Player 태그 본체를 PrimaryPlayerLocator로 찾음)
        [SerializeField] private Rigidbody2D player;

        // 플레이어 몸(위치/속도) 접근 추상화
        private IPlayerBody _body;
        // 잡은 프레임의 놓기 입력을 무시하는 문지기
        private ReleaseGate _gate;
        // 정지 요청 합산 허브(Alt와 공유)
        private SuspendRequestHub _hub;
        // 현재 매달려 있는지
        private bool _hanging;
        // F/Space/WASD 자체 입력
        private RopeInput _input;
        // 오를 때 작은 좌우 반동 생성기
        private ClimbSway _sway;
        // 플레이어 진자 물리
        private Pendulum _pendulum;
        // 매달림 회전각을 받을 플레이어 쪽
        private IRopeAngleSink _angleSink;
        // 손/발 스프라이트 정보 제공자
        private IRopeGripSource _gripSource;
        // 매달림 회전각과 발 고정 위치 계산
        private GripSolver _grip;
        // 애니메이션 포즈 보고 대상
        private IPlayerPoseSink _pose;
        // 줄 모양(Verlet) 시뮬레이션. 시각용이라 플레이어 위치에는 영향 없음
        private RopeSimulation _rope;
        // 한 번에 한 줄만 잡게 하는 슬롯 + 재잡기 쿨다운
        private HoldSlot _slot;
        // 진자 상태(각도, 각속도, anchor로부터의 거리 s)
        private PendulumState _state;

        // IRopePose 구현: 렌더러/콜라이더가 읽는 줄 점들
        public IReadOnlyList<Vector2> Points => _rope?.Points;

        // 줄 고정점 위치
        private Vector2 Anchor => anchor != null ? (Vector2)anchor.position : (Vector2)transform.position;

        // 필수 참조 확인 후 로직 객체들과 플레이어 쪽 컴포넌트를 조립한다.
        private void Awake() {
            if (player == null) {
                var found = PrimaryPlayerLocator.Find(this);
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
            _angleSink = player.GetComponent<IRopeAngleSink>();
            _gripSource = player.GetComponentInChildren<IRopeGripSource>();
            _grip = new GripSolver(settings.Grip);
            _input = new RopeInput();
        }

        private void OnEnable() {
            _input?.Enable();
        }

        // 비활성/파괴 시 매달려 있으면 강제로 놓아 정지 요청이 남지 않게 한다.
        private void OnDisable() {
            ForceRelease();
            _input?.Disable();
        }

        private void OnDestroy() {
            ForceRelease();
            _input?.Dispose();
        }

        // Alt로 timeScale이 0이면 FixedUpdate가 안 돌아 입력이 쌓이므로 비운다. (줄/진자 계산은 FixedUpdate에서 해서 Alt 중 자동 정지)
        private void Update() {
            if (Time.timeScale <= 0f)
                _input.Clear();
        }

        // 잡기 시도. 이미 매달렸거나 슬롯 사용 불가(다른 줄/쿨다운)면 false.
        public bool TryGrab() {
            if (_rope == null || _hanging || !isActiveAndEnabled)
                return false;

            if (!_slot.TryAcquire(this, Time.time))
                return false;

            var velocity = _body.Velocity;
            // 플레이어와 가장 가까운 줄 위 점을 잡는 위치로 정함
            var grabPoint = _rope.PrepareGrab(_body.Position);
            var requested = false;
            try {
                // 플레이어 조작 정지 요청. 실패하면 아래 finally에서 슬롯을 되돌린다.
                _hub.Request(this);
                requested = true;
            }
            finally {
                if (!requested)
                    _slot.Release(this, Time.time, 0f);
            }

            // 잡은 위치/속도에서 진자 시작 상태 생성
            _state = _pendulum.CreateState(Anchor, grabPoint, velocity);
            _sway.Reset();
            _grip.Reset();
            // 잡은 프레임 기록: 같은 프레임의 F 입력이 곧바로 놓기로 처리되는 것을 막는다.
            _gate.MarkGrab(Time.frameCount, Time.realtimeSinceStartup);
            _hanging = true;
            return true;
        }

        // 매 물리 틱: 놓기 입력 처리 후 매달림이면 Hang, 아니면 줄만 자유 시뮬레이션.
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

        // 매달린 중이고 입력이 있고 ReleaseGate를 통과한 놓기만 인정
        private bool ReadRelease(bool pressed, int pressFrame) {
            return _hanging && pressed && _gate.Accepts(pressFrame, Time.realtimeSinceStartup);
        }

        // 매달린 한 틱: 입력 -> 진자 갱신 -> 플레이어 위치 적용 -> 줄 갱신 -> 포즈 보고.
        private void Hang(float deltaTime) {
            var axis = _input.Axis;
            // A/D 흔들기 + 오를 때 생기는 작은 반동
            var swing = axis.x + _sway.Step(axis.y, deltaTime);
            _state = _pendulum.Step(_state, new PendulumInput(swing, axis.y), deltaTime);
            var position = Pendulum.PositionOf(Anchor, _state);
            _body.Position = position;
            // 위치는 진자가 정하므로 물리 속도는 0으로 고정
            _body.Velocity = Vector2.zero;
            _rope.StepHeld(Anchor, _state.Length, position, deltaTime, SolveGrip(position, deltaTime));
            _pose?.ReportRope(true, ToRowMove(axis.y));
        }

        // 손/발 샘플로 매달림 회전각과 발 고정점을 계산하고 각도를 플레이어에 보고. 발이 없으면 null.
        private FootPin? SolveGrip(Vector2 hand, float deltaTime) {
            var sample = _gripSource != null && _gripSource.TryGetSample(out var found) ? found : default;
            var result = _grip.Solve(_state.Angle, hand, sample, deltaTime);
            _angleSink?.ReportRopeAngle(result.Angle);
            return result.HasFoot ? new FootPin(result.Foot, result.FootDistance) : null;
        }

        // 놓기: 이탈 속도 계산 -> 줄 고정 해제 -> 정지 해제 -> 속도 적용(정지 해제를 먼저 해야 속도가 안 지워진다).
        private void Release(bool jump) {
            var velocity = ReleaseVelocity.Compute(_state, jump, settings.JumpReleaseBonus);
            _hanging = false;
            _rope.Release();
            _grip.Reset();
            try {
                _hub.Release(this);
            }
            finally {
                _slot.Release(this, Time.time, settings.RegrabCooldown);
                _pose?.ReportRope(false, RowMoveDirection.None);
                _body.Velocity = velocity;
            }
        }

        // 속도 부여 없이 정리만 하는 강제 해제(비활성/파괴용)
        private void ForceRelease() {
            if (!_hanging)
                return;

            _hanging = false;
            _rope.Release();
            _grip.Reset();
            try {
                _hub.Release(this);
            }
            finally {
                _slot.Release(this, Time.time, settings.RegrabCooldown);
                _pose?.ReportRope(false, RowMoveDirection.None);
            }
        }

        // 세로 입력을 오름/내림/없음 포즈 방향으로 변환
        private static RowMoveDirection ToRowMove(float vertical) {
            if (vertical > ClimbPoseThreshold)
                return RowMoveDirection.Up;

            return vertical < -ClimbPoseThreshold ? RowMoveDirection.Down : RowMoveDirection.None;
        }
    }
}
