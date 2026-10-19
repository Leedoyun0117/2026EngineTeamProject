using System.Collections;

namespace LDY.Script
{
    public class ZoomStep : ITransitionStep
    {
        private readonly MonitorZoomRig _rig;

        public ZoomStep(MonitorZoomRig rig)
        {
            _rig = rig;
        }

        public IEnumerator Run(TransitionContext context)
        {
            yield return _rig.Play();
        }
    }
}
