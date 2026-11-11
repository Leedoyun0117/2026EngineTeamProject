using UnityEngine;

namespace HES.Script.Object {
    public class LButton : ButtonParent {
        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            var door = transform.root.GetComponent<LRDoor>();
            Debug.Assert(door != null, $"{door} is null or empty");
            door.Press(0);
            gameObject.SetActive(false);
        }
    }
}