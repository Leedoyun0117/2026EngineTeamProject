namespace LDY.Spider {
    public interface IGroundProbe {
        bool TryGetGroundY(float x, float fromY, out float y);
    }

    public struct AttackMotionConfig {
        public float RaiseHeight;
        public float RaiseTime;
        public float TremorAmplitude;
        public float TremorHertz;
        public float StabTime;
        public float HoldShake;
        public float HoldShakeHertz;
        public float RetractLiftShare;
        public float RetractRise;
        public float RetractArcHeight;
    }

    // 공격하는 다리 한 개의 바늘 끝 경로: 위로 올림(ease-out) → 떨림 → 수직 낙하(ease-in) → 박힘 → 수직으로 뽑음 → 호를 그리며 복귀.
    public sealed class LegChoreographer {
        private readonly SpiderBrain _brain;
        private readonly IGroundProbe _probe;
        private readonly AttackMotionConfig _config;

        private SpiderState _previous = SpiderState.Idle;
        private Vec2 _from;
        private Vec2 _raised;
        private float _stabElapsed;

        public LegChoreographer(SpiderBrain brain, IGroundProbe probe, AttackMotionConfig config) {
            _brain = brain;
            _probe = probe;
            _config = config;
        }

        public Vec2 TargetPoint { get; private set; }

        public bool ShowsMarker => _brain.State == SpiderState.Telegraph || _brain.State == SpiderState.Stab ||
                                   _brain.State == SpiderState.Stuck;

        public float MarkerGrowth => _brain.State == SpiderState.Telegraph ? _brain.Progress : 1f;

        // 공격 중인 다리의 끝점을 계산한다. 이 다리를 직접 움직일 필요가 없으면 false.
        public bool TryStep(float deltaTime, Vec2 currentTip, Vec2 plantedFoot, out Vec2 tip) {
            var state = _brain.State;
            if (state != _previous) {
                OnEnter(state, currentTip, plantedFoot);
                _previous = state;
            }

            tip = currentTip;
            if (!_brain.IsLegAlive(_brain.ActiveLeg))
                return false;

            switch (state) {
                case SpiderState.Telegraph:
                    tip = Raise();
                    break;
                case SpiderState.Stab:
                    tip = Drop(deltaTime);
                    break;
                case SpiderState.Stuck:
                    tip = TargetPoint + LegMotion.Tremor(_brain.Elapsed, _config.HoldShake, _config.HoldShakeHertz);
                    break;
                case SpiderState.Retract:
                    tip = Pull(plantedFoot);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private Vec2 Raise() {
            var elapsed = _brain.Elapsed;
            var rising = _config.RaiseTime <= 0f ? 1f : elapsed / _config.RaiseTime;
            if (rising < 1f)
                return Vec2.Lerp(_from, _raised, Easing.EaseOut(rising));

            return _raised + LegMotion.Tremor(elapsed - _config.RaiseTime, _config.TremorAmplitude, _config.TremorHertz);
        }

        private Vec2 Drop(float deltaTime) {
            _stabElapsed += deltaTime;
            var t = _config.StabTime <= 0f ? 1f : _stabElapsed / _config.StabTime;
            var y = _raised.Y + (TargetPoint.Y - _raised.Y) * Easing.EaseIn(t);
            if (t >= 1f)
                _brain.TipArrived();

            return new Vec2(TargetPoint.X, t >= 1f ? TargetPoint.Y : y);
        }

        private Vec2 Pull(Vec2 plantedFoot) {
            var progress = _brain.Progress;
            var share = _config.RetractLiftShare;
            var top = new Vec2(TargetPoint.X, TargetPoint.Y + _config.RetractRise);
            if (progress < share)
                return Vec2.Lerp(TargetPoint, top, Easing.EaseOut(progress / share));

            return LegMotion.Arc(top, plantedFoot, (progress - share) / (1f - share), _config.RetractArcHeight);
        }

        private void OnEnter(SpiderState state, Vec2 currentTip, Vec2 plantedFoot) {
            if (state == SpiderState.Telegraph) {
                _from = currentTip;
                var x = _brain.TargetX;
                var found = _probe.TryGetGroundY(x, currentTip.Y + _config.RaiseHeight, out var y);
                var groundY = found ? y : plantedFoot.Y;
                TargetPoint = new Vec2(x, groundY);
                _raised = new Vec2(x, groundY + _config.RaiseHeight);
                _stabElapsed = 0f;
            }
        }
    }
}
