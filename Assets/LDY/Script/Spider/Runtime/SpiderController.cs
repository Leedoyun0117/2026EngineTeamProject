using System;
using UnityEngine;
using UnityEngine.Events;

namespace LDY.Spider {
    // 조립과 틱 전달만 한다. 상태는 SpiderBrain, 바늘 경로는 LegChoreographer, 발 걸음은 FootPlanter가 정한다.
    public sealed class SpiderController : MonoBehaviour {
        private const float FootSnapRise = 2f;

        [SerializeField] private SpiderSettings settings;
        [SerializeField] private SpiderLeg[] legs;
        [SerializeField] private SpiderBodyView bodyView;
        [SerializeField] private TelegraphMarker marker;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private BoxCollider2D combatZone;
        [SerializeField] private UnityEvent onDefeated;
        [SerializeField] private UnityEvent onStabImpact;
        [SerializeField] private UnityEvent onHurt;

        private SpiderBrain _brain;
        private LegChoreographer _choreographer;
        private FootPlanter _planter;
        private BodyTracker _tracker;
        private ImpactBounce _impact;
        private readonly SmoothedValue _lean = new SmoothedValue();
        private IPlayerLocator _locator;
        private Vec2[] _desired;
        private Vec2[] _tips;
        private float[] _originalRestX;
        private float[] _restOffsetX;
        private float[] _socketX;
        private float _baseY;
        private float _playerX;
        private float _recoilDirection;

        public event Action Defeated;
        public event Action StabImpact;
        public event Action Hurt;

        private void Awake() {
            var probe = new Physics2DGroundProbe(groundMask);
            _locator = new TagPlayerLocator();
            _brain = new SpiderBrain(settings.ToBrainConfig(legs.Length, CurrentZone()),
                () => UnityEngine.Random.value * 2f - 1f);
            _choreographer = new LegChoreographer(_brain, probe, settings.ToAttackMotion());
            _planter = new FootPlanter(legs.Length, settings.StepThreshold, settings.StepTime, settings.StepHeight);
            _tracker = new BodyTracker(transform.position.x, settings.FollowMaxSpeed, settings.FollowAcceleration);
            _impact = new ImpactBounce(settings.ImpactAmplitude, settings.ImpactDuration);
            _baseY = transform.position.y;
            _playerX = transform.position.x;

            foreach (var leg in legs) {
                leg.Initialize(settings.TipRadius);
                leg.SeverRequested += OnSeverRequested;
            }

            bodyView.Initialize(settings);
            marker.Initialize(settings.MarkerBlinkHz, settings.MarkerStartScale);
            CaptureFeet(probe);
            _brain.StateChanged += OnStateChanged;
            _brain.Defeated += OnBrainDefeated;
        }

        private void FixedUpdate() {
            var deltaTime = Time.fixedDeltaTime;
            if (_locator.TryGetX(out var x))
                _playerX = x;

            _tracker.Step(deltaTime, BodyGoalX(), _brain.IsStriking || _brain.IsFleeing);
            transform.position = new Vector3(_tracker.X, _baseY, transform.position.z);

            for (var i = 0; i < legs.Length; i++)
                _socketX[i] = legs[i].Socket.X;

            _brain.Tick(deltaTime, _playerX, _tracker.X, _socketX);
            var rise = RiseOffset();
            var offset = new Vector2(RecoilOffset(), rise + _impact.Step(deltaTime));
            bodyView.Tick(deltaTime, offset, _lean.Step(LeanTarget(), settings.LeanRate, deltaTime), _brain.State,
                _brain.Elapsed);

            PlaceFeet(deltaTime);
            DriveAttackLeg(deltaTime);
            FlailRemainingLegs();
            DriveLegs();
            marker.Tick(deltaTime, _choreographer.ShowsMarker, _choreographer.TargetPoint, _choreographer.MarkerGrowth);
        }

        private CombatZone CurrentZone() {
            if (combatZone == null)
                return default;

            var center = combatZone.transform.TransformPoint(combatZone.offset).x;
            var halfWidth = combatZone.size.x * 0.5f * Mathf.Abs(combatZone.transform.lossyScale.x);
            return new CombatZone(center - halfWidth, center + halfWidth);
        }

        private float BodyGoalX() {
            return _brain.State == SpiderState.Reposition ? _brain.BodyGoalX : _tracker.X;
        }

        private void CaptureFeet(IGroundProbe probe) {
            var feet = new Vec2[legs.Length];
            _desired = new Vec2[legs.Length];
            _tips = new Vec2[legs.Length];
            _originalRestX = new float[legs.Length];
            _restOffsetX = new float[legs.Length];
            _socketX = new float[legs.Length];
            for (var i = 0; i < legs.Length; i++) {
                var foot = legs[i].RestFoot();
                if (probe.TryGetGroundY(foot.X, foot.Y + FootSnapRise, out var groundY))
                    foot = new Vec2(foot.X, groundY);

                feet[i] = foot;
                _originalRestX[i] = foot.X - transform.position.x;
                _restOffsetX[i] = _originalRestX[i];
                _socketX[i] = legs[i].Socket.X;
            }

            _planter.Initialize(feet);
            for (var i = 0; i < legs.Length; i++)
                _tips[i] = feet[i];
        }

        private void PlaceFeet(float deltaTime) {
            if (_brain.IsFleeing) {
                TuckLegs();
                return;
            }

            for (var i = 0; i < legs.Length; i++)
                _desired[i] = new Vec2(_tracker.X + _restOffsetX[i], _planter.Planted(i).Y);

            _planter.SetBusy(_brain.HasActiveLeg ? _brain.ActiveLeg : -1);
            _planter.Step(deltaTime, _desired);
            for (var i = 0; i < legs.Length; i++)
                _tips[i] = _planter.Current(i);
        }

        private void TuckLegs() {
            var tuck = LegTuck.Progress(_brain.Progress, settings.FleeTuckShare);
            for (var i = 0; i < legs.Length; i++) {
                var hang = legs[i].Socket - new Vec2(0f, settings.FleeTuckHang);
                _tips[i] = LegTuck.Tip(_planter.Planted(i), hang, tuck);
            }
        }

        private void DriveAttackLeg(float deltaTime) {
            var leg = _brain.ActiveLeg;
            if (!_brain.HasActiveLeg || !_brain.IsLegAlive(leg))
                return;

            if (_choreographer.TryStep(deltaTime, _tips[leg], _planter.Planted(leg), out var tip))
                _tips[leg] = tip;
        }

        private void FlailRemainingLegs() {
            if (_brain.State != SpiderState.Hurt)
                return;

            for (var i = 0; i < legs.Length; i++) {
                if (!_brain.IsLegAlive(i))
                    continue;

                var degrees = LegFlail.Degrees(_brain.Progress, _brain.Elapsed, settings.FlailDegrees,
                    settings.FlailHertz, i * 1.3f);
                _tips[i] = LegFlail.Rotate(_tips[i], legs[i].Socket, degrees);
            }
        }

        private void DriveLegs() {
            for (var i = 0; i < legs.Length; i++) {
                if (legs[i].IsCut)
                    continue;

                legs[i].SetNeedleActive(_brain.IsStriking && i == _brain.ActiveLeg);
                legs[i].Apply(_tips[i]);
            }
        }

        private float RiseOffset() {
            switch (_brain.State) {
                case SpiderState.Flee: return settings.FleeDistance * Easing.EaseIn(_brain.Progress);
                case SpiderState.Gone: return settings.FleeDistance;
                default: return 0f;
            }
        }

        private float RecoilOffset() {
            if (_brain.State != SpiderState.Hurt)
                return 0f;

            var progress = _brain.Elapsed / settings.HurtRecoilTime;
            return _recoilDirection * settings.HurtRecoil * Easing.Pulse(progress);
        }

        private float LeanTarget() {
            var leg = _brain.ActiveLeg;
            if (!_brain.IsLegAlive(leg))
                return 0f;

            return BodyPose.LeanTarget(_brain.State, legs[leg].Socket.X - _tracker.X, settings.LeanDegrees);
        }

        private void OnStateChanged(SpiderState state) {
            if (state == SpiderState.Stuck) {
                _impact.Trigger();
                StabImpact?.Invoke();
                onStabImpact?.Invoke();
            }
            else if (state == SpiderState.Hurt) {
                BeginHurt();
            }
        }

        private void BeginHurt() {
            var leg = _brain.ActiveLeg;
            _recoilDirection = leg >= 0 && legs[leg].Socket.X >= _tracker.X ? -1f : 1f;
            Hurt?.Invoke();
            onHurt?.Invoke();
        }

        private void OnSeverRequested(SpiderLeg leg) {
            var index = Array.IndexOf(legs, leg);
            if (!_brain.Sever(index))
                return;

            _planter.Cut(index);
            leg.Cut(settings.PieceLifetime);
            StanceLayout.Spread(_originalRestX, _brain.LegAlive, _restOffsetX);
        }

        private void OnBrainDefeated() {
            Defeated?.Invoke();
            onDefeated?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
