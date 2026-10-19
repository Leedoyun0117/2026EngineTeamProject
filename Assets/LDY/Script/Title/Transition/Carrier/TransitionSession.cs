namespace LDY.Script
{
    public enum TransitionPhase
    {
        Leaving,   // 이전 씬이 아직 활성(실패하면 되돌릴 수 있음)
        Switching, // 새 씬 활성화 요청됨
        Entering   // 새 씬의 SceneEntryDirector가 넘겨받음
    }

    public class TransitionSession
    {
        public TransitionPhase Phase { get; internal set; } = TransitionPhase.Leaving;
    }
}
