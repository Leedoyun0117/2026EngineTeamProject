using System;
using UnityEngine;

namespace LDY.Script
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputLock), typeof(JJBControlGate))]
    public class AltModeController : MonoBehaviour, IAltStatus
    {
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private MonoBehaviour pointerOverride;
        [SerializeField] private BehaviourCameraLock cameraLock = new BehaviourCameraLock();
        [SerializeField] private AltReturnSettings returnSettings = new AltReturnSettings();
        [SerializeField] private Color afterimageTint = new Color(0.78f, 0.62f, 0.38f, 0.75f);
        [SerializeField, Min(0.001f)] private float maxDeltaTime = TitleTuning.MaxDeltaTime;

        private AltStateMachine _machine;
        private AltKeyInput _altInput;
        private IPointerSource _pointer;
        private ITimeFreezer _freezer;

        public AltPhase Phase => _machine?.Current?.Phase ?? AltPhase.Normal;
        public event Action<AltPhase> PhaseChanged;
        public Vector2 CursorWorldPosition => _pointer?.WorldPosition ?? (Vector2)transform.position;

        private void Awake()
        {
            if (playerSprite == null)
                playerSprite = GetComponentInChildren<SpriteRenderer>();
            if (targetCamera == null)
                targetCamera = Camera.main;

            IInputLock inputLock = GetComponent<PlayerInputLock>().Lock;
            var body = new Rigidbody2DPlayerBody(GetComponent<Rigidbody2D>());
            var facing = new SpriteFacing(transform, playerSprite);
            var control = GetComponent<JJBControlGate>();
            var afterimage = new SpriteAfterimageSpawner(playerSprite, afterimageTint);
            var session = new AltSession();
            _pointer = pointerOverride as IPointerSource ?? new MouseWorldPointer(targetCamera, transform);
            _freezer = new TimeScaleFreezer();
            _altInput = new AltKeyInput(inputLock);
            _machine = new AltStateMachine();
            _machine.PhaseChanged += phase => PhaseChanged?.Invoke(phase);

            _machine.Register(new NormalState(_altInput, _machine));
            _machine.Register(new AltState(_altInput, _pointer, body, facing, control, afterimage,
                _freezer, cameraLock, session, _machine));
            _machine.Register(new ReturnState(body, facing, control, _freezer, cameraLock, inputLock,
                session, returnSettings, _machine));
            _machine.Switch<NormalState>();
        }

        private void OnEnable()
        {
            _altInput.Enable();
        }

        private void OnDisable()
        {
            Recover();
            _altInput.Disable();
        }

        private void OnDestroy()
        {
            Recover();
            _altInput?.Dispose();
        }

        private void Update()
        {
            // 포커스 없는 동안은 연출을 멈춤(timeScale은 그대로)
            float dt = Application.isFocused ? Mathf.Min(Time.unscaledDeltaTime, maxDeltaTime) : 0f;
            _machine.Tick(dt);
        }

        // Alt 도중 꺼져도 timeScale/물리/잔상/잠금 토큰은 복구해야 해서
        private void Recover()
        {
            if (_machine == null)
                return;

            if (_machine.Current is AltState)
                _machine.Switch<ReturnState>();
            if (_machine.Current is ReturnState)
                _machine.Switch<NormalState>();

            _freezer.Release();
        }
    }
}
