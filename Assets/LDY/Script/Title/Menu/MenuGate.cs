using System.Collections.Generic;

namespace LDY.Script
{
    public class MenuGate : IMenuGate
    {
        private readonly IReadOnlyList<IMenuBlocker> _blockers;

        public MenuGate(IReadOnlyList<IMenuBlocker> blockers)
        {
            _blockers = blockers;
        }

        public bool CanExecute
        {
            get
            {
                for (int i = 0; i < _blockers.Count; i++)
                {
                    if (_blockers[i].Blocking)
                        return false;
                }

                return true;
            }
        }
    }
}
