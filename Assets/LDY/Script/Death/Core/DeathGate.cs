namespace LDY.Death {
    public enum DeathPhase {
        Alive,
        Dying,
        RestartRequested
    }

    // 죽음은 한 번만 시작하고, 재시작 요청도 한 번만 보낸다.
    public sealed class DeathGate {
        public DeathPhase Phase { get; private set; } = DeathPhase.Alive;
        public bool IsDying => Phase != DeathPhase.Alive;

        public bool TryBegin() {
            if (Phase != DeathPhase.Alive)
                return false;

            Phase = DeathPhase.Dying;
            return true;
        }

        public bool TryRequestRestart() {
            if (Phase != DeathPhase.Dying)
                return false;

            Phase = DeathPhase.RestartRequested;
            return true;
        }
    }
}
