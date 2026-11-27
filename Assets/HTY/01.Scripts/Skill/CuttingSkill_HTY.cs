using UnityEngine;
using UnityEngine.InputSystem;

public class CuttingSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private TargetSelector_HTY _targetSelector;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public void Init()
    {
        _targetSelector = GetComponentInParent<TargetSelector_HTY>();
        _skillState = SkillState.Cutting;
        _skillSO = SkillManager_HTY.Instance._skillSoList._skillDict[_skillState];
    }

    public bool CanUse() => true;//나중에 연결할떄 타겟이 null이 아니면 true

    public void UseSkill()
    {
        if (_targetSelector._currentTarget != null)//나중에 canskill로 이전하기
        {
            Destroy(_targetSelector._currentTarget.gameObject);
            _targetSelector.TargetDetectAndSelect(CanIf.CanCut);
        }
    }

    public void Enter()
    {
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
            _targetSelector.TargetDetectAndSelect(CanIf.CanCut);
    }

    public void Exit()
    {
        //공란
    }
}
