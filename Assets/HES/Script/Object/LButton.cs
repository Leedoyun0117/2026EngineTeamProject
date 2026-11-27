using System;
using UnityEngine;

namespace HES.Script.Object {
    public class LButton : ButtonParent {
        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            var door = transform.root.GetComponent<LRDoor>();
            try {
                door.Press(0);
            }
            catch (Exception e) {
                Debug.Log($"{e}");
            }
            gameObject.SetActive(false);
        }
    }
}