namespace LDY.Script
{
    // Jump와 Fall은 클립이 아직 없다. PlayerAnimSet에 클립을 넣으면 코드 수정 없이 쓰인다.
    public enum PlayerAnimId
    {
        Idle,
        Walk,
        Run,
        SitDown,
        SitLoop,
        StandUp,
        Jump,
        Fall
    }
}
