using UnityEngine;

namespace HES.Script.Object {
    public class LongPressButton : ButtonParent {
        [SerializeField] private float pressTime;
        [SerializeField] private GameObject door;
        private bool _isActive;
        private float _pressedTime;

        private void Update() {
            if (!IsPressed && !_isActive) {
                _pressedTime = 0;
                return;
            }

            _pressedTime += Time.deltaTime;
            if (_pressedTime > pressTime) {
                door.SetActive(false);
                _isActive = true;
            }
        }
    }
}