using System;
using UnityEngine;

namespace HES.Script.Object {
    public class Block : MonoBehaviour {
        private bool _isReal;
        public event Action OnPressReal;
        public event Action OnPressFake;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                if (_isReal) {
                    OnPressReal?.Invoke();
                }
                else {
                    OnPressFake?.Invoke();
                }
            }
        }
    }
}