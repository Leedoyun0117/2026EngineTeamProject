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
            if (_playerGimmick == null)
                return;
            
            if (_playerGimmick.HasGimmick)
            {
                EjectGimmick();
                return;
            }

            TryAbsorbGimmick();
        }

        private void TryAbsorbGimmick()
        {
            if (absorbPoint == null)
                return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                absorbPoint.position,
                absorbSize,
                absorbLayer
            );

            foreach (Collider2D hit in hits)
            {
                MonoBehaviour[] components = hit.GetComponentsInParent<MonoBehaviour>();

                bool isAbsorb = false;

                foreach (MonoBehaviour component in components)
                {
                    if (component is IAbsorbable)
                    {
                        isAbsorb = true;
                        break;
                    }
                }

                if (!isAbsorb)
                    continue;

                GimmickSkillProvider provider = hit.GetComponentInParent<GimmickSkillProvider>();

                if (provider == null || provider.SkillData == null)
                    continue;

                SkillManager_HTY manager = SkillManager_HTY.Instance;

                if (manager == null)
                    return;
                
                if (!_playerGimmick.TryAbsorb(provider.SkillData))
                    return;
                
                manager.ChangeSkillState(provider.SkillData);
                Destroy(provider.gameObject);
                return;
            }
        }

        private void EjectGimmick()
        {
            SkillManager_HTY manager = SkillManager_HTY.Instance;

            if (manager == null)
                return;

            manager.SkillDrop();

            _playerGimmick.ReleaseGimmick();
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