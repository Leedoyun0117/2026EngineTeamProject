using System;
using JJB.Script.Player.Interface;
using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerInteract : MonoBehaviour
    {
        [Header("Interact")]
        [SerializeField] private Transform interactPoint;
        [SerializeField] private LayerMask objectLayer;
        [SerializeField] private float interactSize = 0.5f;

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

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                interactPoint.position,
                interactSize,
                objectLayer
            );

            foreach (Collider2D hit in hits)
            {
                IInteract target = hit.GetComponentInParent<IInteract>();

                if (target != null)
                    return target;
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            if (interactPoint == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactSize);
        }
    }
}