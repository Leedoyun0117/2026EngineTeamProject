namespace LDY.Spider {
    public enum TrapPhase {
        Armed,
        Snapped
    }

    public enum TrapOutcome {
        None,
        KillPlayer,
        SeverLeg
    }

    public sealed class TrapState {
        public TrapPhase Phase { get; private set; } = TrapPhase.Armed;

        public TrapOutcome PlayerTouched() => Trigger(TrapOutcome.KillPlayer);

        public TrapOutcome NeedleTouched() => Trigger(TrapOutcome.SeverLeg);

        private TrapOutcome Trigger(TrapOutcome outcome) {
            if (Phase == TrapPhase.Snapped)
                return TrapOutcome.None;

            Phase = TrapPhase.Snapped;
            return outcome;
        }
    }
}
