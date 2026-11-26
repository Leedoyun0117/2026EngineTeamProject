using System;
using UnityEngine;

namespace HES.Script.Object {
    public class Block : MonoBehaviour {
        private bool _isReal;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                if (_isReal) {
                }
            }
        }
    }
}