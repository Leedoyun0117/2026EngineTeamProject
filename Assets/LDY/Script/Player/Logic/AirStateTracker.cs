namespace LDY.Script {
    public sealed class AirStateTracker {
        private readonly PlayerAnimRules _rules;
        private float _airTime;
        private PlayerAnimId _last = PlayerAnimId.Fall;
        private bool _hasLast;

        public AirStateTracker(PlayerAnimRules rules) {
            _rules = rules;
        }

        public void Reset() {
            _airTime = 0f;
            _hasLast = false;
        }

        public bool TryResolve(in PlayerAnimInput input, out PlayerAnimId state) {
            if (input.RowMove != RowMoveDirection.None) {
                _airTime = 0f;
                _hasLast = false;
                state = input.RowMove == RowMoveDirection.Up ? PlayerAnimId.Jump : PlayerAnimId.Fall;
                return true;
            }

            if (input.Grounded) {
                Reset();
                state = default;
                return false;
            }

            _airTime += input.DeltaTime;
            var vy = input.Velocity.y;

            if (vy > _rules.airVelocityMargin) {
                state = Remember(PlayerAnimId.Jump);
                return true;
            }

            if (!_hasLast && _airTime < _rules.groundLeaveGrace) {
                state = default;
                return false;
            }

            if (vy < -_rules.airVelocityMargin || !_hasLast)
                Remember(PlayerAnimId.Fall);

            state = _last;
            return true;
        }

        private PlayerAnimId Remember(PlayerAnimId id) {
            _last = id;
            _hasLast = true;
            return id;
        }
    }
}
