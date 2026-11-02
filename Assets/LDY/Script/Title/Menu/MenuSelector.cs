namespace LDY.Script {
    public class MenuSelector {
        private readonly IAltStatus _alt;
        private readonly IMenuTargetFinder _finder;
        private readonly MenuFocus _focus;
        private readonly IButtonInput _input;
        private readonly AltPhase _phase;

        public MenuSelector(AltPhase phase, IAltStatus alt, IMenuTargetFinder finder, IButtonInput input,
            MenuFocus focus) {
            _phase = phase;
            _alt = alt;
            _finder = finder;
            _input = input;
            _focus = focus;
        }

        public void Tick() {
            if (_alt.Phase != _phase) {
                _focus.Release(this);
                return;
            }

            var target = _finder.Find();
            _focus.Set(this, target);

            if (target != null && _input.Pressed)
                target.Execute();
        }
    }
}