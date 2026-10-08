namespace LDY.Script
{
    public class NormalState : IAltState
    {
        private readonly IAltInput _input;
        private readonly IAltStateSwitcher _switcher;

        public NormalState(IAltInput input, IAltStateSwitcher switcher)
        {
            _input = input;
            _switcher = switcher;
        }

        public AltPhase Phase => AltPhase.Normal;

        public void Enter() { }
        public void Exit() { }

        public void Tick(float unscaledDeltaTime)
        {
            if (_input.Pressed)
                _switcher.Switch<AltState>();
        }
    }
}
