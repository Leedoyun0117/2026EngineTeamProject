using System.Collections;

namespace LDY.Script
{
    // 이전 씬이 사라지기 전에 Carrier에 먼저 알림(MarkSwitching)
    public class ActivateSceneStep : ITransitionStep
    {
        private readonly LoadingStep _loading;
        private readonly TransitionCarrier _carrier;

        public ActivateSceneStep(LoadingStep loading, TransitionCarrier carrier)
        {
            _loading = loading;
            _carrier = carrier;
        }

        public IEnumerator Run(TransitionContext context)
        {
            ISceneLoadHandle handle = _loading.Handle;
            if (handle == null)
            {
                context.Fail("활성화할 씬 핸들 없음");
                yield break;
            }

            _carrier.MarkSwitching();
            handle.Activate();
            while (!handle.IsDone)
                yield return null;
        }
    }
}
