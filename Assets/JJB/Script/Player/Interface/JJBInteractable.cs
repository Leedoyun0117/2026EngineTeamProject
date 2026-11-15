using UnityEngine;

namespace JJB.Script.Player.Interface
{
    public abstract class JJBInteractable : MonoBehaviour, IInteract
    {
        public abstract bool Interact(JJBPlayerInteract target);
    }
}