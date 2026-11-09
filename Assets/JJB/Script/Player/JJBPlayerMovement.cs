using UnityEngine;
using UnityEngine.Serialization;

namespace JJB.Script
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JJBPlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(1)] private float moveSpeed;
        
        public Rigidbody2D rb;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Move(float direction)
        {
            rb.linearVelocityX = direction * moveSpeed;
        }
    }
}