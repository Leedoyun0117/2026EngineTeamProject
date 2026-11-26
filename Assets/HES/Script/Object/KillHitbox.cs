using System;
using LDY.Death;
using LDY.Script;
using UnityEngine;

namespace HES.Script.Object {
    public class KillHitbox : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            var pd = other.GetComponentInChildren<PlayerDeath>();
            try {
                pd.Die(DeathCause.Trap);
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        }
    }
}