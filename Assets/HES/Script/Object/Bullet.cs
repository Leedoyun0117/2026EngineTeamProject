using UnityEngine;

namespace HES.Script.Object {
    public class Bullet : MonoBehaviour {
        [SerializeField] private float speed;
        private Rigidbody2D _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable() {
            _rb.linearVelocity = transform.right * speed;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Glass")) {
                Destroy(gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}