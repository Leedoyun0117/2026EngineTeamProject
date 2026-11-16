using UnityEngine;

public class DragSkill_HTY : MonoBehaviour, ISkill_HTY
{
    private FixedJoint2D _joint;
    private bool _canDrag;
    [SerializeField] private float _maxDistance = 2.0f;
    private GameObject _dragObj;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _joint = GetComponentInParent<FixedJoint2D>();
        _joint.enabled = false;
        _skillState = SkillState.Drag;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
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