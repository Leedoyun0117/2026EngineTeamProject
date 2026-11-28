using UnityEngine;
using UnityEngine.InputSystem;

public class ReScaleObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private TargetSelector_HTY _targetSelector;
    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }


    public void Init()
    {
        _targetSelector = GetComponentInParent<TargetSelector_HTY>();
        _skillState = SkillState.ReSizeObj;
        _skillSO = SkillManager_HTY.Instance._stateToSoDict[_skillState];
    }

    public bool CanUse()
    {
        if (_targetSelector == null)
            return false;
        Transform currentTarget = _targetSelector._currentTarget;
        if (currentTarget == null)
            return false;
        return currentTarget.TryGetComponent<ReScaleObject_HTY>(out _);//_는 그냥 가져올수있는지만 확인할거라 썼음
    }

    public void UseSkill()
    {
        if (!CanUse())
            return;

        Transform target = _targetSelector._currentTarget;

        if (target.TryGetComponent(out ReScaleObject_HTY reScale))
        {
            reScale.ChangeScale();
        }
    }

    public void Enter()
    {
    }

    public void UpdateState()
    {
        if(Keyboard.current.yKey.wasPressedThisFrame)
            UseSkill();
        if (Keyboard.current.rKey.wasPressedThisFrame)
            _targetSelector.TargetDetectAndSelect(CanIf.CanResize);
    }

    public void Exit()
    {
        
    }
}