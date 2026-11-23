using UnityEngine;
using UnityEngine.InputSystem;

public class StopObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private TargetSelector_HTY _targetSelector;
    private Camera _mainCam;

    private Rigidbody2D _otherRb;
    private bool _isStop = false;
    private GameObject _stopObj;

    private RigidbodyType2D _saveType;
    private Vector2 _saveDir;
    private float _saveAng;

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
        if (_stopObj == null)//다거치고 멈춰진 개체가 없다면
            return true;

        return _stopObj == currentTarget.gameObject;//멈췄던 개체라면
    }

    public void UseSkill()
    {
        if (!CanUse()) return;

        GameObject target =_targetSelector._currentTarget.gameObject;

        if (_isStop)
        {
            ReSetObject();
            return;
        }

        StopObject(target);
    }

    private void StopObject(GameObject target)//멈추기 용
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();

        if (rb == null) return;

        _otherRb = rb;
        _stopObj = target;
        _saveDir = rb.linearVelocity;
        _saveAng = rb.angularVelocity;
        _saveType = rb.bodyType;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        _isStop = true;
    }

    private void ReSetObject()//돌아오기용
    {
        if (_otherRb != null)
        {
            _otherRb.bodyType = _saveType;
            _otherRb.linearVelocity = _saveDir;
            _otherRb.angularVelocity = _saveAng;
        }

        _isStop = false;
        _stopObj = null;
        _otherRb = null;
    }

    private bool IsVisible(GameObject target)//화면 처리
    {
        if (target == null) return false;

        Vector3 screenPos = _mainCam.WorldToViewportPoint(target.transform.position);

        return screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    public void Enter()
    {
        _isStop = false;
        _stopObj = null;
        _saveType = RigidbodyType2D.Dynamic;
        _saveDir = Vector2.zero;
        _saveAng = 0;
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
            _targetSelector.TargetDetectAndSelect();
    }
}