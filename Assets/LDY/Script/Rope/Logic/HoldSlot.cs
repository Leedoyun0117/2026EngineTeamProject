namespace LDY.Script {
    public sealed class HoldSlot {
        private float _blockedUntil;
        private object _owner;

        public bool TryAcquire(object owner, float now) {
            if (owner == null)
                return false;

            if (_owner != null)
                return ReferenceEquals(_owner, owner);

            if (now < _blockedUntil)
                return false;

            _owner = owner;
            return true;
        }

        public void Release(object owner, float now, float cooldown) {
            if (owner == null || !ReferenceEquals(_owner, owner))
                return;

            _owner = null;
            _blockedUntil = now + cooldown;
        }
    }
}
