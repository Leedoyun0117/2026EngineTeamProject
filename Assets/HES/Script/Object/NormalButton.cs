using UnityEngine;

namespace HES.Script.Object {
    public class NormalButton : ButtonParent {
        [SerializeField] private GameObject door;

        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            if(door == null) return;
            door.SetActive(false);
        }

        protected override void OnTriggerExit2D(Collider2D other) {
            base.OnTriggerExit2D(other);
            if(door == null) return;
            door.SetActive(true);
        }
    }
}