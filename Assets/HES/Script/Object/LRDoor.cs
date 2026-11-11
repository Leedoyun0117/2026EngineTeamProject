using UnityEngine;

namespace HES.Script.Object {
    public class LRDoor : MonoBehaviour {
        private bool _LPress;
        private bool _RPress;

        private void Update() {
            if (_LPress && _RPress) gameObject.SetActive(false);
        }

        public void Press(int choose) {
            switch (choose) {
                case 0:
                    _LPress = true;
                    break;
                case 1:
                    _RPress = true;
                    break;
            }
        }
    }
}