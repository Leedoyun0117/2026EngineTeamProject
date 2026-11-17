namespace LDY.Script {
    // 플레이어가 한 번에 줄 하나만 잡도록 하는 슬롯. 놓은 뒤 재잡기 쿨다운도 관리한다.
    public sealed class HoldSlot {
        // 이 시각 전에는 새로 못 잡음
        private float _blockedUntil;
        // 현재 잡고 있는 소유자(없으면 null)
        private object _owner;

        // 비어 있고 쿨다운이 지났을 때만 획득. 이미 내가 소유자면 true.
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

        // 소유자만 해제 가능. 해제 시 쿨다운 시작.
        public void Release(object owner, float now, float cooldown) {
            if (owner == null || !ReferenceEquals(_owner, owner))
                return;

            _owner = null;
            _blockedUntil = now + cooldown;
        }
    }
}
