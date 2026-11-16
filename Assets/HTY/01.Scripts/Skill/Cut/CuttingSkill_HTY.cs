using UnityEngine;
using UnityEngine.InputSystem;

public class CuttingSkill_HTY : MonoBehaviour, ISkill_HTY
{
    public TargetSelector_HTY _targetSelector;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.Cutting;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public bool CanUse() => true;//나중에 연결할떄 타겟이 null이 아니면 true


    public void UseSkill()
    {
        if (_targetSelector._currentTarget != null)//나중에 canskill로 이전하기
        {
            Destroy(_targetSelector._currentTarget.gameObject);
        }
    }

    public void Enter()
    {
        //공란
    }

    public void UpdateState()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            UseSkill();
        }
    }

    public void Exit()
    {
        //공란
    }
}
