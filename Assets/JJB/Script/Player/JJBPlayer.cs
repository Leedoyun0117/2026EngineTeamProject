using JJB.Script.Player;
using UnityEngine;

namespace JJB.Script
{
    [RequireComponent(typeof(JJBPlayerInput))]
    [RequireComponent(typeof(JJBPlayerMovement))]
    [RequireComponent(typeof(JJBPlayerJump))]
    public class JJBPlayer : MonoBehaviour
    {
        public JJBPlayerInput PlayerInput { get; private set; }
        public JJBPlayerMovement PlayerMovement { get; private set; }
        public JJBPlayerJump PlayerJump { get; private set; }
        public JJBPlayerInteract PlayerInteract { get; private set; }
        public JJBPlayerRenderer PlayerRenderer { get; private set; }

        private void Awake()
        {
            Initialize(GetComponent<JJBPlayerInput>(),
                GetComponent<JJBPlayerMovement>(),
                GetComponent<JJBPlayerJump>(),
                GetComponent<JJBPlayerInteract>(),
                GetComponent<JJBPlayerRenderer>());
        }

        private void Initialize(JJBPlayerInput playerInput, 
            JJBPlayerMovement playerMovement, 
            JJBPlayerJump playerJump, 
            JJBPlayerInteract playerInteract, 
            JJBPlayerRenderer playerRenderer)
        {
            PlayerInput = playerInput;
            PlayerMovement = playerMovement;
            PlayerJump = playerJump;
            PlayerInteract = playerInteract;
            PlayerRenderer = playerRenderer;
        }

        private void OnEnable()
        {
            PlayerInput.OnJumpKeyPressed += HandleJumpKeyPress;
            PlayerInput.OnInteractPressed += HandleInteractPressed;
        }

        private void OnDisable()
        {
            PlayerInput.OnJumpKeyPressed -= HandleJumpKeyPress;
            PlayerInput.OnInteractPressed -= HandleInteractPressed;
        }
        
        private void HandleJumpKeyPress()
        {
            PlayerJump.RequestJump();
        }

        private void HandleInteractPressed()
        {
            PlayerInteract.Trigger();
        }

        private void Update()
        {
            //PlayerRenderer.FilpX(PlayerMovement.rb.linearVelocityX);
        }
        
        private void FixedUpdate()
        {
            PlayerMovement.Move(PlayerInput.MoveDir.x);
            PlayerJump.Tick(PlayerInput.IsJumpHold);
        }
    }
}
