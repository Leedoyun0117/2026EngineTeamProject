using UnityEngine;

namespace JJB.Script
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JJBPlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(1)] private float moveSpeed;
        
        private Rigidbody2D _rb;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Move(float direction)
        {
            _rb.linearVelocityX = direction * moveSpeed;
        }
    }
}