using UnityEngine;

namespace HES.Script.Object {
    public class ButtonBox : MonoBehaviour {
        private bool IsPressed;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Box")) IsPressed = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Box")) IsPressed = false;
        }
    }
}