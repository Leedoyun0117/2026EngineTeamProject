using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerInteract : MonoBehaviour
    {
        [Header("Interact")]
        [SerializeField] private Transform interactPoint;
        [SerializeField] private LayerMask interactMask;
        [SerializeField] private float  interactSize;
        
        public void Trigger()
        {
            IInteract target = FindTarget();

            if (target == null)
                return;

            target.Interact(this);
        }

        private IInteract FindTarget()
        {
            if (interactPoint == null)
                return null;

            Collider2D[] hits = Physics2D.OverlapCircleAll(interactPoint.position, interactSize, interactMask);

            foreach (Collider2D hit in hits)
            {
                IInteract target = hit.GetComponentInParent<IInteract>();

                if (target != null)
                    return target;
                
                hit.gameObject.SetActive(false);
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactSize);
        }
    }
}