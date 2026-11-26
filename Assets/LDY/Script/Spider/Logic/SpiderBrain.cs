using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.Spider {
    public enum SpiderState {
        Idle,
        Aim,
        Reposition,
        Telegraph,
        Stab,
        Stuck,
        Retract,
        Hurt,
        Flee,
        Gone
    }

    public struct SpiderBrainConfig {
        public int LegCount;
        public float AimDuration;
        public float TelegraphTime;
        public float LockTime;
        public float StuckTime;
        public float RetractTime;
        public float Cooldown;
        public float TargetError;
        public int SeversToDefeat;
        public float FleeTime;
        public float HurtTime;
        public float HurtCooldown;
        public IReadOnlyList<LegReach> Reach;
        public float Standoff;
        public float ArriveTolerance;
        public CombatZone Zone;
    }

    public sealed class SpiderBrain {
        private readonly SpiderBrainConfig _config;
        private readonly Func<float> _signedRandom;
        private readonly bool[] _alive;
        private float _idleLength;
        private float _nextIdleLength;
        private float _targetOffset;

        public SpiderBrain(SpiderBrainConfig config, Func<float> signedRandom) {
            _config = config;
            _signedRandom = signedRandom;
            _alive = new bool[config.LegCount];
            Array.Fill(_alive, true);
            ActiveLeg = -1;
            _idleLength = config.Cooldown;
            _nextIdleLength = config.Cooldown;
        }

        public SpiderState State { get; private set; } = SpiderState.Idle;
        public float Elapsed { get; private set; }
        public int ActiveLeg { get; private set; }
        public float TargetX { get; private set; }
        public float BodyGoalX { get; private set; }
        public int SeveredCount { get; private set; }
        public IReadOnlyList<bool> LegAlive => _alive;

        // 텔레그래프 앞부분은 플레이어를 따라가고, LockTime 이후와 그 뒤 상태에서는 목표가 굳는다.
        public bool IsTargetLocked => State != SpiderState.Telegraph || Elapsed >= _config.LockTime;

        public bool IsStriking => State == SpiderState.Stab || State == SpiderState.Stuck;
        public bool IsFleeing => State == SpiderState.Flee || State == SpiderState.Gone;
        public bool HasActiveLeg => State >= SpiderState.Telegraph && State <= SpiderState.Retract;

        public float Progress => Fraction(Elapsed, DurationOf(State));

        public event Action<SpiderState> StateChanged;
        public event Action Defeated;

        public bool IsLegAlive(int leg) => leg >= 0 && leg < _alive.Length && _alive[leg];

        public void Tick(float deltaTime, float playerX, IReadOnlyList<float> legX) {
            Tick(deltaTime, playerX, legX.Count == 0 ? 0f : legX.Average(), legX);
        }

        public void Tick(float deltaTime, float playerX, float bodyX, IReadOnlyList<float> legX) {
            var before = Elapsed;
            Elapsed += deltaTime;
            switch (State) {
                case SpiderState.Idle when Elapsed >= _idleLength:
                    Enter(SpiderState.Aim);
                    break;
                case SpiderState.Aim:
                    TickAim(playerX, bodyX, legX);
                    break;
                case SpiderState.Reposition:
                    TickReposition(playerX, bodyX, legX);
                    break;
                case SpiderState.Telegraph:
                    TickTelegraph(playerX, before);
                    break;
                case SpiderState.Stuck when Elapsed >= _config.StuckTime:
                    Enter(SpiderState.Retract);
                    break;
                case SpiderState.Retract when Elapsed >= _config.RetractTime:
                    Enter(SpiderState.Idle);
                    break;
                case SpiderState.Hurt when Elapsed >= _config.HurtTime:
                    _nextIdleLength = _config.HurtCooldown;
                    Enter(SpiderState.Idle);
                    break;
                case SpiderState.Flee when Elapsed >= _config.FleeTime:
                    Enter(SpiderState.Gone);
                    Defeated?.Invoke();
                    break;
            }
        }

        public void TipArrived() {
            if (State == SpiderState.Stab)
                Enter(SpiderState.Stuck);
        }

        public bool Sever(int leg) {
            if (!IsLegAlive(leg) || IsFleeing)
                return false;

            _alive[leg] = false;
            SeveredCount++;
            if (SeveredCount >= _config.SeversToDefeat)
                Enter(SpiderState.Flee);
            else if (leg == ActiveLeg && IsAttacking(State))
                Enter(SpiderState.Hurt);

            return true;
        }

        // LockTime을 넘기는 틱까지는 플레이어 위치를 한 번 더 받고, 그다음 틱부터 굳는다.
        // 직전 Elapsed를 그대로 비교한다(뺄셈으로 되돌리면 float 오차로 경계가 흔들린다).
        private void TickTelegraph(float playerX, float elapsedBefore) {
            if (elapsedBefore < _config.LockTime)
                TargetX = playerX + _targetOffset;

            if (Elapsed >= _config.TelegraphTime)
                Enter(SpiderState.Stab);
        }

        private void TickAim(float playerX, float bodyX, IReadOnlyList<float> legX) {
            if (!Reaches(playerX, bodyX, legX))
                Enter(SpiderState.Reposition);
            else if (Elapsed >= _config.AimDuration)
                BeginTelegraph(playerX, bodyX, legX);
        }

        private void TickReposition(float playerX, float bodyX, IReadOnlyList<float> legX) {
            BodyGoalX = AttackRangeRule.StandoffBodyX(playerX, bodyX, legX, _alive, _config.Standoff, _config.Zone);
            if (Math.Abs(bodyX - BodyGoalX) > _config.ArriveTolerance)
                return;

            Enter(Reaches(playerX, bodyX, legX) ? SpiderState.Aim : SpiderState.Idle);
        }

        private void BeginTelegraph(float playerX, float bodyX, IReadOnlyList<float> legX) {
            _targetOffset = _signedRandom() * _config.TargetError;
            TargetX = playerX + _targetOffset;
            var leg = PickLeg(TargetX, bodyX, legX);
            if (leg < 0) {
                _targetOffset = 0f;
                TargetX = playerX;
                leg = PickLeg(TargetX, bodyX, legX);
            }

            if (leg < 0) {
                Enter(SpiderState.Idle);
                return;
            }

            ActiveLeg = leg;
            Enter(SpiderState.Telegraph);
        }

        private bool Reaches(float playerX, float bodyX, IReadOnlyList<float> legX) {
            return _config.Reach == null || PickLeg(playerX, bodyX, legX) >= 0;
        }

        private int PickLeg(float targetX, float bodyX, IReadOnlyList<float> legX) {
            return _config.Reach == null
                ? LegPicker.Pick(targetX, legX, _alive)
                : AttackRangeRule.Pick(targetX, bodyX, legX, _alive, _config.Reach);
        }

        private void Enter(SpiderState next) {
            State = next;
            Elapsed = 0f;
            if (next == SpiderState.Idle) {
                ActiveLeg = -1;
                _idleLength = _nextIdleLength;
                _nextIdleLength = _config.Cooldown;
            }

            StateChanged?.Invoke(next);
        }

        private static bool IsAttacking(SpiderState state) {
            return state == SpiderState.Telegraph || state == SpiderState.Stab || state == SpiderState.Stuck;
        }

        private float DurationOf(SpiderState state) {
            switch (state) {
                case SpiderState.Telegraph: return _config.TelegraphTime;
                case SpiderState.Stuck: return _config.StuckTime;
                case SpiderState.Retract: return _config.RetractTime;
                case SpiderState.Hurt: return _config.HurtTime;
                case SpiderState.Flee: return _config.FleeTime;
                default: return 0f;
            }
        }

        private static float Fraction(float elapsed, float duration) {
            return duration <= 0f ? 0f : Math.Clamp(elapsed / duration, 0f, 1f);
        }
    }
}
