using System;
using UnityEngine;

namespace HES.Script.Object {
    public class KillHitbox : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                Destroy(other.gameObject);
            }
        }
    }
}