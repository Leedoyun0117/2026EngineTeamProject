using UnityEngine;

public class ReScaleObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    [SerializeField] private TargetSelector _targetSelector;

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
}