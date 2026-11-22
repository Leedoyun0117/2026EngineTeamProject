namespace LDY.Script {
    public enum PoseLayer {
        Normal,
        Alt,
        Trapped,
        Death
    }

    // 사망 연출이 Alt 표시보다 우선한다.
    public static class PosePriority {
        public static PoseLayer Resolve(bool dead, bool trapped, bool alt) {
            if (dead)
                return PoseLayer.Death;

            if (trapped)
                return PoseLayer.Trapped;

            return alt ? PoseLayer.Alt : PoseLayer.Normal;
        }
    }

    public enum DeathStage {
        None,
        Pull,
        Trapped,
        TrappedTail,
        Death,
        Done
    }

    public struct DeathTimings {
        public float PullTime;
        public float TrappedTime;
        public float TailTime;
        public float DeathTime;
        public float EndHold;
    }

    public interface IDeathPose {
        bool IsTrapped { get; }
        bool IsDead { get; }
        DeathStage Stage { get; }
    }

    // 쥐덫: 끌어당김 → trapped → trapped 끝 두 프레임 한 번 더 → death. 바늘: death만.
    public sealed class DeathSequence {
        private DeathTimings _timings;
        private float _elapsed;

        public DeathStage Stage { get; private set; } = DeathStage.None;
        public bool Finished { get; private set; }
        public bool IsActive => Stage != DeathStage.None && Stage != DeathStage.Done;
        public bool IsTrapped => Stage == DeathStage.Trapped || Stage == DeathStage.TrappedTail;
        public bool IsDead => Stage == DeathStage.Death || Stage == DeathStage.Done;

        public float PullProgress {
            get {
                if (Stage == DeathStage.Pull)
                    return _timings.PullTime <= 0f ? 1f : _elapsed / _timings.PullTime;

                return Stage == DeathStage.None ? 0f : 1f;
            }
        }

        public void BeginTrapped(DeathTimings timings) => Begin(DeathStage.Pull, timings);

        public void BeginNeedle(DeathTimings timings) => Begin(DeathStage.Death, timings);

        public void Tick(float deltaTime) {
            Finished = false;
            if (!IsActive)
                return;

            _elapsed += deltaTime;
            while (Stage != DeathStage.Done && _elapsed >= DurationOf(Stage)) {
                _elapsed -= DurationOf(Stage);
                Advance();
            }
        }

        private void Begin(DeathStage first, DeathTimings timings) {
            _timings = timings;
            _elapsed = 0f;
            Finished = false;
            Stage = first;
        }

        private void Advance() {
            switch (Stage) {
                case DeathStage.Pull:
                    Stage = DeathStage.Trapped;
                    break;
                case DeathStage.Trapped:
                    Stage = DeathStage.TrappedTail;
                    break;
                case DeathStage.TrappedTail:
                    Stage = DeathStage.Death;
                    break;
                case DeathStage.Death:
                    Stage = DeathStage.Done;
                    Finished = true;
                    break;
            }
        }

        private float DurationOf(DeathStage stage) {
            switch (stage) {
                case DeathStage.Pull: return _timings.PullTime;
                case DeathStage.Trapped: return _timings.TrappedTime;
                case DeathStage.TrappedTail: return _timings.TailTime;
                case DeathStage.Death: return _timings.DeathTime + _timings.EndHold;
                default: return float.MaxValue;
            }
        }
    }
}
