using UnityEngine;

namespace HES.Script.Object {
    public class Switch : ButtonParent {
        [SerializeField] private float pressTime;
        [SerializeField] private GameObject door;
        private bool _isActive;
        private float _pressedTime;
        
        private void Update() {
            if(door == null) return;
            Debug.Assert(door == null, "Door is not or empty");
            door.SetActive(!_isActive);
        }

        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            _isActive = !_isActive;
        }
    }
}