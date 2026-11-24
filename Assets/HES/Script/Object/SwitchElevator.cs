using UnityEngine;

namespace HES.Script.Object {
    public class SwitchElevator : MonoBehaviour {
        [SerializeField] private float pressTime;
        private Elevator _elevator;
        private float _t;

        private void Awake() {
            _elevator = transform.root.GetComponent<Elevator>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") && _t + _elevator.Duration > Time.time) return;
            _t = Time.time;
            _elevator.MoveElevator();
        }
    }
}