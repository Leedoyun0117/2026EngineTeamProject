using UnityEngine;
using UnityEngine.InputSystem;

public class DragSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private FixedJoint2D _joint;
    private bool _canDrag;
    private float _detectRange = 2.0f;
    private GameObject _dragObj;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public void Init()
    {
        _joint = GetComponentInParent<FixedJoint2D>();
        _joint.enabled = false;
        _skillState = SkillState.Drag;
        _skillSO = SkillManager_HTY.Instance._stateToSoDict[_skillState];
    }

    public bool CanUse() => _canDrag;

    public void UseSkill()
    {
        Debug.Log(_canDrag);


        if (CanUse())
        {
            Collider2D[] _rbColl = Physics2D.OverlapCircleAll(transform.position, _detectRange);

            foreach (Collider2D coll in _rbColl)
            {
                if (coll.TryGetComponent(out DragObj Obj))
                {

                    Debug.Log(0101);
                    _joint.connectedBody = Obj._rb;
                    _joint.enabled = true;
                    _dragObj = Obj.gameObject;
                    _canDrag = false;
                    Obj._rb.mass = 10;
                }
            }
        }
        else
        {
            if (_dragObj == null) return;

            if (_dragObj.TryGetComponent(out DragObj Obj))
            {
                _joint.connectedBody = null;
                _joint.enabled = false;
                _dragObj = null;
                _canDrag = true;
                Obj._rb.mass = 1000;
            }
        }
    }

    public void Enter()
    {
        _joint.connectedBody = null;
        _joint.enabled = false;
        _dragObj = null;
        _canDrag = true;
    }

    public void Exit()
    {
        if (_dragObj == null) return;

        if (_dragObj.TryGetComponent(out DragObj Obj))
        {
            _joint.connectedBody = null;
            _joint.enabled = false;
            _dragObj = null;
            _canDrag = false;
            Obj._rb.mass = 1000;
        }
    }

    public void UpdateState()
    {
        if(Keyboard.current.yKey.wasPressedThisFrame)
            UseSkill();


        if(_joint.connectedBody != null)
        {
            float distance = Vector2.Distance(transform.position, _joint.connectedBody.transform.position);

            if (distance > _detectRange)
            {
                if (_dragObj.TryGetComponent(out DragObj Obj))
                {
                    _joint.connectedBody = null;
                    _joint.enabled = false;
                    _dragObj = null;
                    _canDrag = false;
                    Obj._rb.mass = 1000;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectRange);
    }
}