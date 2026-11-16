using UnityEngine;

namespace HES.Script.Object {
    public class Switch : ButtonParent {
        [SerializeField] private float pressTime;
        [SerializeField] private GameObject door;
        private bool _isActive;
        private float _pressedTime;

        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            _isActive = !_isActive;
        }

        private void Update() {
            door.SetActive(!_isActive);
        }
    }
}