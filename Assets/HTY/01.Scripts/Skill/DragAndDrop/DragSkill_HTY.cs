using UnityEngine;

public class DragSkill_HTY : MonoBehaviour, ISkill_HTY {
    [SerializeField] private float _maxDistance = 2.0f;
    private bool _canDrag;
    private GameObject _dragObj;
    private FixedJoint2D _joint;

    private void Awake() {
        _joint = GetComponent<FixedJoint2D>();
        _joint.enabled = false;
    }


    private void Update() {
        if (_joint.connectedBody == null) {
            if (CanUse()) {
                var _rbColl = Physics2D.OverlapCircle(transform.position, _maxDistance);
                if (_rbColl.gameObject.TryGetComponent(out DragObj Obj)) {
                    _joint.connectedBody = Obj._rb;
                    _joint.enabled = true;
                    _dragObj = Obj.gameObject;
                    _canDrag = true;
                    Obj._rb.mass = 10;
                }
            }
        }
        else {
            var distance = Vector2.Distance(transform.position, _joint.connectedBody.transform.position);

            if (distance > _maxDistance)
                if (_dragObj.TryGetComponent(out DragObj Obj)) {
                    _joint.connectedBody = null;
                    _joint.enabled = false;
                    _dragObj = null;
                    _canDrag = false;
                    Obj._rb.mass = 1000;
                }
        }
    }

    public bool CanUse() {
        return _canDrag;
    }

    public void UseSkill() {
        _canDrag = !_canDrag;
        if (!_canDrag) {
            _joint.connectedBody = null;
            _joint.enabled = false;
        }
    }
}