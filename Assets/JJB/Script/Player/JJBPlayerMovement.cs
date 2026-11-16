using UnityEngine;

namespace JJB.Script.Player
{ 
    [RequireComponent(typeof(Rigidbody2D))]
    public class JJBPlayerMovement : MonoBehaviour 
    {
        [SerializeField] [Min(1)] private float moveSpeed;

        public Rigidbody2D Rb { get; private set; }

        private void Awake() 
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        public void Move(float direction) 
        {
            Rb.linearVelocityX = direction * moveSpeed;
        }
    }
}