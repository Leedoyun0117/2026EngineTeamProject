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
        public bool IsAbsorb { get; private set; }
        
        public event Action OnJumpKeyPressed;
        public event Action OnInteractPressed;
        public event Action OnAbsorbPressed;
        
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
        
        // F키
        private void OnInteract(InputValue value)
        {
            IsEating = value.isPressed;
            if (value.isPressed)
            {
                OnInteractPressed?.Invoke();
            }
        }
        
        // E키
        private void OnAbsorb(InputValue value)
        {
            IsAbsorb = value.isPressed;
            if (value.isPressed)
            {
                OnAbsorbPressed?.Invoke();
            }
        }
        
        private void OnDisable()
        {
            MoveDir = Vector2.zero;
            IsJumpHold = false;
        }
    }
}