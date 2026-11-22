using System;
using UnityEngine;

namespace HES.Script.Object {
    public class FallingBlock : MonoBehaviour {
        private Rigidbody2D _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D other) {
             if(other.gameObject.CompareTag("Player")) _rb.gravityScale = 1;
        }
    }
}