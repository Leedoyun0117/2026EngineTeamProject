using JJB.Script.Player.Interface;
using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerAbsorb : MonoBehaviour
    {
        [Header("Absorb")]
        [SerializeField] private Transform absorbPoint;
        [SerializeField] private LayerMask absorbLayer;
        [SerializeField] private float absorbSize = 0.5f;

        private JJBPlayerGimmick _playerGimmick;

        private void Awake()
        {
            _playerGimmick = GetComponent<JJBPlayerGimmick>();
        }

        public void Trigger()
        {
            if (_playerGimmick == null || _playerGimmick.HasGimmick)
                return;

            IAbsorbable target = FindTarget();

            if (target == null)
                return;

            if (!_playerGimmick.TryAbsorb(target))
                return;

            if (!target.Absorb(this))
                _playerGimmick.ReleaseGimmick();
        }

        private IAbsorbable FindTarget()
        {
            if (absorbPoint == null)
                return null;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                absorbPoint.position,
                absorbSize,
                absorbLayer
            );

            foreach (Collider2D hit in hits)
            {
                IAbsorbable target = hit.GetComponentInParent<IAbsorbable>();

                if (target != null)
                    return target;
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            if (absorbPoint == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(absorbPoint.position, absorbSize);
        }
    }
}