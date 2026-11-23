using UnityEngine;

namespace JJB.Script.Player
{ 
    [RequireComponent(typeof(Rigidbody2D))]
    public class JJBPlayerMovement : MonoBehaviour 
    {
        [SerializeField] [Min(1)] private float moveSpeed;
        public bool IsPlayObj { get; private set; } = true;

        public Rigidbody2D Rb { get; private set; }

        private void Awake() 
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        public void Move(float direction) 
        {
            if (!IsPlayObj) return;
            Rb.linearVelocityX = direction * moveSpeed;
        }

        public void SetPlay(bool play)
        {
            IsPlayObj = play;
        }
    }
}