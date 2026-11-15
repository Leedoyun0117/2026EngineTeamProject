using UnityEngine;

namespace HES.Script.Object {
    public class Switch : ButtonParent {
        [SerializeField] private float pressTime;
        [SerializeField] private GameObject door;
        private bool _isActive;
        private float _pressedTime;

        private void Update() {
            if (IsPressed) {
                _isActive = !_isActive;
            }

            door.SetActive(!_isActive);
        }
    }
}