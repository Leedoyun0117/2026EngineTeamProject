using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class StopObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private TargetSelector_HTY _targetSelector;
    private Camera _mainCam;

    private bool _isStop = false;
    private GameObject _stopObj;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }


    public void Init()
    {
        _mainCam = Camera.main;
        _targetSelector = GetComponentInParent<TargetSelector_HTY>();
        _skillState = SkillState.TimeStop;
        _skillSO = SkillManager_HTY.Instance._skillSoList._skillDict[_skillState];
    }

    public bool CanUse()
    {
        if (_targetSelector == null)
            return false;
        Transform currentTarget = _targetSelector._currentTarget;

        if (currentTarget == null)
            return false;
        if (_stopObj == null)
            return true;

        return _stopObj == currentTarget.gameObject;
    }

    public void UseSkill()
    {
        if (!CanUse()) return;

        GameObject target = _targetSelector._currentTarget.gameObject;

        if (_isStop)
        {
            ReSetObject();
            return;
        }

        StopObject(target);
    }

    private void StopObject(GameObject target)
    {
        _stopObj = target;
        target.transform.DOPause();
        ICanMove canMove = target.GetComponent<ICanMove>();
        if (canMove != null)
        {
            canMove.IsStop = true;
        }

        _isStop = true;
    }

    private void ReSetObject()
    {
        if (_stopObj != null)
        {
            _stopObj.transform.DOPlay();
            ICanMove canMove = _stopObj.GetComponent<ICanMove>();
            if (canMove != null)
            {
                canMove.IsStop = false;
            }
        }

        _isStop = false;
        _stopObj = null;
    }

    private bool IsVisible(GameObject target)
    {
        if (target == null) return false;

        Vector3 screenPos = _mainCam.WorldToViewportPoint(target.transform.position);

        return screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    public void Enter()
    {
        _isStop = false;
        _stopObj = null;
    }

    public void Exit()
    {

    }

    public void UpdateState()
    {
        if (_isStop && _stopObj != null)
        {
            if (!IsVisible(_stopObj))
            {
                ReSetObject();
            }
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            _targetSelector.TargetDetectAndSelect(CanIf.CanStop);
        }
    }
}