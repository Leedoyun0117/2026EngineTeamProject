using UnityEngine;

namespace LDY.Script
{
    public class TimeScaleFreezer : ITimeFreezer
    {
        private float _previousScale = 1f;
        private bool _frozen;

        public void Freeze()
        {
            if (_frozen)
                return;

            _frozen = true;
            _previousScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        public void Release()
        {
            if (!_frozen)
                return;

            _frozen = false;
            Time.timeScale = _previousScale;
        }
    }
}
