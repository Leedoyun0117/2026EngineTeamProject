public interface ISkill_HTY
{
    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public void Init();

    void Enter();
    void UpdateState();
    void Exit();

    bool CanUse();
    void UseSkill();
}
