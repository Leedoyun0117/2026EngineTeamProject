using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetSelector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask targetLayer;

    private readonly List<Transform> _targets = new();

    private int _currentIndex = -1;
    public Transform _currentTarget;

    private void Update()
    {
        //나중에 어떤 스킬 상태를 만들어서 그때만 활성화 되게 그리고 활성화된다면 그때마다 가장 가까운 하나를 미리 지정해두기
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            DetectTargets();
            SelectNextTarget();
        }
    }

    private void DetectTargets()
    {
        _targets.Clear();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,detectionRadius,targetLayer);

        foreach (Collider2D col in colliders)
        {
            Transform target = col.transform;

            if (target == transform) continue;

            if (!_targets.Contains(target))//타겟이 리스트미리 있는지 확인
                _targets.Add(target);
        }
        _targets.Sort((a, b) =>//블로그 참고
        {
            float distanceA =
                (a.position - transform.position).sqrMagnitude;

            float distanceB =
                (b.position - transform.position).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });
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

        _currentTarget = _targets[_currentIndex];

        Debug.Log("{_currentTarget.name}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position,detectionRadius);
    }
}