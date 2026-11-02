using UnityEngine;

namespace JJB.Script.Player {
    public class JJBPlayerInteract : MonoBehaviour {
        [Header("Interact")] [SerializeField] private Transform interactPoint;

        [SerializeField] private LayerMask interactMask;
        [SerializeField] private float interactSize;

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactSize);
        }

        public void Trigger() {
            var target = FindTarget();

            if (target == null)
                return;

            target.Interact(this);
        }

        private IInteract FindTarget() {
            if (interactPoint == null)
                return null;

            var hits = Physics2D.OverlapCircleAll(interactPoint.position, interactSize, interactMask);

            foreach (var hit in hits) {
                var target = hit.GetComponentInParent<IInteract>();

                if (target != null)
                    return target;

                hit.gameObject.SetActive(false);
            }

            return null;
        }
    }
}