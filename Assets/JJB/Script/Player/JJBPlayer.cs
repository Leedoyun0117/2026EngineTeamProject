using UnityEngine;

namespace JJB.Script.Player
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
        public JJBPlayerAbsorb PlayerAbsorb { get; private set; }
        public JJBPlayerRenderer PlayerRenderer { get; private set; }

        private void Awake()
        {
            Initialize(GetComponent<JJBPlayerInput>(),
                GetComponent<JJBPlayerMovement>(),
                GetComponent<JJBPlayerJump>(),
                GetComponent<JJBPlayerInteract>(),
                GetComponentInChildren<JJBPlayerRenderer>(),
                GetComponent<JJBPlayerAbsorb>());
        }

        private void Initialize(JJBPlayerInput playerInput, 
            JJBPlayerMovement playerMovement, 
            JJBPlayerJump playerJump, 
            JJBPlayerInteract playerInteract, 
            JJBPlayerRenderer playerRenderer,
            JJBPlayerAbsorb playerAbsorb)
        {
            PlayerInput = playerInput;
            PlayerMovement = playerMovement;
            PlayerJump = playerJump;
            PlayerInteract = playerInteract;
            PlayerRenderer = playerRenderer;
            PlayerAbsorb = playerAbsorb;
        }

        private void OnEnable()
        {
            PlayerInput.OnJumpKeyPressed += HandleJumpKeyPress;
            PlayerInput.OnInteractPressed += HandleInteractPressed;
            PlayerInput.OnAbsorbPressed += HandleAbsorbPressed;
        }

        private void OnDisable()
        {
            PlayerInput.OnJumpKeyPressed -= HandleJumpKeyPress;
            PlayerInput.OnInteractPressed -= HandleInteractPressed;
            PlayerInput.OnAbsorbPressed -= HandleAbsorbPressed;
        }
        
        private void HandleJumpKeyPress()
        {
            PlayerJump.RequestJump();
        }
        
        // F키
        private void HandleInteractPressed()
        {
            if (PlayerInteract != null)
                PlayerInteract.Trigger();
        }
        
        // E키
        private void HandleAbsorbPressed()
        {
            PlayerAbsorb.Trigger();
        }

        private void Update()
        {
            //PlayerRenderer.FilpX(PlayerMovement.Rb.linearVelocityX);
        }
        
        private void FixedUpdate()
        {
            PlayerMovement.Move(PlayerInput.MoveDir.x);
            PlayerJump.Tick(PlayerInput.IsJumpHold);
        }
    }
}
