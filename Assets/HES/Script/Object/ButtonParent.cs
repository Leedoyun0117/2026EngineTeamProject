using UnityEngine;

namespace HES.Script.Object {
    public abstract class ButtonParent : MonoBehaviour {
        protected bool IsPressed;

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) IsPressed = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) IsPressed = false;
        }
    }
}