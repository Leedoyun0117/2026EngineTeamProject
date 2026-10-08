namespace LDY.Script
{
    // 지정한 Alt 단계에서만 동작한다. 대상 탐색 방식과 실행 입력을 주입받아 플레이어/커서 방식이 공유한다.
    public class MenuSelector
    {
        private readonly AltPhase _phase;
        private readonly IAltStatus _alt;
        private readonly IMenuTargetFinder _finder;
        private readonly IButtonInput _input;
        private readonly MenuFocus _focus;

        public MenuSelector(AltPhase phase, IAltStatus alt, IMenuTargetFinder finder, IButtonInput input,
            MenuFocus focus)
        {
            _phase = phase;
            _alt = alt;
            _finder = finder;
            _input = input;
            _focus = focus;
        }

        public void Tick()
        {
            if (_alt.Phase != _phase)
            {
                _focus.Release(this);
                return;
            }

            MenuIcon target = _finder.Find();
            _focus.Set(this, target);

            if (target != null && _input.Pressed)
                target.Execute();
        }
    }
}
