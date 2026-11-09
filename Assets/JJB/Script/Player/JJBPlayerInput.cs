using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JJB.Script.Player
{
    public class JJBPlayerInput : MonoBehaviour
    {
        public Vector2 MoveDir { get; private set; }
        public bool IsJumpHold { get; private set; }
        public bool IsEating { get; private set; }
        
        public event Action OnJumpKeyPressed;
        public event Action OnInteractPressed;
        
        private void OnMove(InputValue value)
        {
            MoveDir = value.Get<Vector2>();
        }
        
        private void OnJump(InputValue value)
        {
            IsJumpHold = value.isPressed;
            
            if (value.isPressed)
            {
                OnJumpKeyPressed?.Invoke();
            }
        }

        private void OnInteract(InputValue value)
        {
            IsEating = value.isPressed;
            if (value.isPressed)
            {
                OnInteractPressed?.Invoke();
            }
        }
        
        private void OnDisable()
        {
            MoveDir = Vector2.zero;
            IsJumpHold = false;
        }
    }
}