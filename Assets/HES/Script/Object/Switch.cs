using UnityEngine;

namespace HES.Script.Object {
    public class Switch : ButtonParent {
        [SerializeField] private GameObject door;

        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            OpenDoor();
        }

        private void OpenDoor() {
            door.SetActive(false);
        }
    }
}