using Unity.VisualScripting;
using UnityEngine;

public class DragSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private FixedJoint2D _joint;
    private bool _canDrag;
    [SerializeField] private float _maxDistance = 2.0f;
    private GameObject _dragObj;

    private void Awake()
    {
        _joint = GetComponent<FixedJoint2D>();
        _joint.enabled = false;
    }

    public bool CanUse() => _canDrag;

    public void UseSkill()
    {
        _canDrag = !_canDrag;
        if (_canDrag == false)
        {
            _joint.connectedBody = null;
            _joint.enabled = false;
        }
    }


    private void Update()
    {
        if (_joint.connectedBody == null)
        {
            if (CanUse())
            {
                Collider2D _rbColl = Physics2D.OverlapCircle(transform.position, _maxDistance);
                if (_rbColl.gameObject.TryGetComponent(out DragObj Obj))
                {
                    _joint.connectedBody = Obj._rb;
                    _joint.enabled = true;
                    _dragObj = Obj.gameObject;
                    _canDrag = true;
                    Obj._rb.mass = 10;
                }
            }
        }
        else
        {
            float distance = Vector2.Distance(transform.position, _joint.connectedBody.transform.position);

            if (distance > _maxDistance)
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
}