using UnityEngine;

public class ReScaleObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    [SerializeField] private TargetSelector_HTY _targetSelector;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.ReSizeObj;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
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
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}