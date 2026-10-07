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

        private void Awake()
        {
            Initialize(GetComponent<JJBPlayerInput>(),
                GetComponent<JJBPlayerMovement>(),
                GetComponent<JJBPlayerJump>());
        }

        private void Initialize(JJBPlayerInput playerInput, JJBPlayerMovement playerMovement, JJBPlayerJump playerJump)
        {
            PlayerInput = playerInput;
            PlayerMovement = playerMovement;
            PlayerJump = playerJump;
        }

        private void OnEnable()
        {
            PlayerInput.OnJumpKeyPressed += HandleJumpKeyPress;
        }

        private void OnDisable()
        {
            PlayerInput.OnJumpKeyPressed -= HandleJumpKeyPress;
        }
        
        private void HandleJumpKeyPress()
        {
            PlayerJump.RequestJump();
        }
        
        private void FixedUpdate()
        {
            PlayerMovement.Move(PlayerInput.MoveDir.x);
        }
    }
}
