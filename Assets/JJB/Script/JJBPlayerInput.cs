using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JJB.Script
{
    public class JJBPlayerInput : MonoBehaviour
    {
        public Vector2 MoveDir { get; private set; }
        
        public event Action OnJumpKeyPressed;
        
        private void OnMove(InputValue value)
        {
            MoveDir = value.Get<Vector2>();
        }
        
        private void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                OnJumpKeyPressed?.Invoke();
            }
        }
        
        private void OnDisable()
        {
            MoveDir = Vector2.zero;
        }
    }
}