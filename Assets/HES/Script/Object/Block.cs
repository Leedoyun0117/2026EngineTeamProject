using System;
using UnityEngine;

namespace HES.Script.Object {
    public class Block : MonoBehaviour {
        [SerializeField] private bool isReal;
        private bool _isPressed;
        public event Action<bool> OnPressReal;
        public event Action<bool> OnPressFake;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                _isPressed = !_isPressed;
                if (isReal) {
                    OnPressReal?.Invoke(!_isPressed);
                }
                else {
                    OnPressFake?.Invoke(!_isPressed);
                }
            }
        }
    }
}