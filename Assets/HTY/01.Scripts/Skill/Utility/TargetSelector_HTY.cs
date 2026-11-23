using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetSelector_HTY : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private LayerMask _targetLayer;

    private readonly List<Transform> _targets = new();

    private int _currentIndex = -1;
    public Transform _currentTarget;

    public Material _originMat;
    public Material _outLineMat;

    public void TargetDetectAndSelect()
    {
        DetectTargets();
        SelectNextTarget();
    }

    private void DetectTargets()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,_detectRange,_targetLayer);

        foreach (Collider2D col in colliders)//탐색
        {
            Transform target = col.transform;

            if (target == transform) continue;

            if (!_targets.Contains(target))//타겟이 리스트미리 있는지 확인
                _targets.Add(target);
        }


        List<Collider2D> collList=colliders.ToList();
        List<Transform> _nulltargets = new List<Transform>();

        foreach (var tran in _targets)//탐색
        {
            Collider2D collT = tran.GetComponent<Collider2D>();
            if (!collList.Contains(collT))
            {
                _nulltargets.Add(tran);
            }//타겟이 리스트미리 있는지 확인
        }

        foreach (var tran in _nulltargets)
        {
            if (_currentTarget != null && _currentTarget == tran)
            {
                _currentTarget.GetComponent<SpriteRenderer>().material = _originMat;
                _currentTarget = null;
                _targets.Remove(tran);
            }
        }

        _targets.Sort((a, b) =>//정렬
        {
            float distanceA = (a.position - transform.position).sqrMagnitude;

            float distanceB = (b.position - transform.position).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        if(_currentTarget == null)
        {
            _currentTarget = _targets[0];
        }
        
    }

    private void SelectNextTarget()
    {
        if (_targets.Count == 0)
        {
            _currentIndex = -1;
            _currentTarget = null;
            return;
        }
        int targetIndex = _targets.IndexOf(_currentTarget);

        _currentIndex = targetIndex >= 0? (targetIndex + 1) % _targets.Count:0;

        _currentTarget.GetComponent<SpriteRenderer>().material = _originMat;
        _currentTarget = _targets[_currentIndex];
        _currentTarget.GetComponent<SpriteRenderer>().material = _outLineMat;

        Debug.Log($"{_currentTarget.name}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position,_detectRange);
    }
}