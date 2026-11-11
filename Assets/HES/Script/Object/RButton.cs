using UnityEngine;

namespace HES.Script.Object {
    public class RButton : ButtonParent {
        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            var door = transform.root.GetComponent<LRDoor>();
            Debug.Assert(door != null, $"{door} is null or empty");
            door.Press(1);
            gameObject.SetActive(false);
        }
    }
}