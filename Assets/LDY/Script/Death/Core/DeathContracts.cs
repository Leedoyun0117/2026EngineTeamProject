namespace LDY.Death {
    public enum DeathCause {
        Hazard,
        Fall,
        Enemy,
        Trap,
        Unknown
    }

    public interface IPlayerDeath {
        bool IsDying { get; }

        // 이미 죽는 중이면 무시하고 false. pullToX는 덫 사망에서만 쓴다.
        bool Die(DeathCause cause, float? pullToX = null);
    }

    public interface IStageRestarter {
        bool IsBusy { get; }
        void RequestRestart();
    }
}
