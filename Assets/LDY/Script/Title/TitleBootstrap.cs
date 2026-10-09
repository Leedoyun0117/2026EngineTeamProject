using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 조립과 Update 위임만 담당한다.
    [DisallowMultipleComponent]
    public class TitleBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private SubScreenController subScreens;
        [SerializeField] private GameStartTransition transition;
        [SerializeField] private MonitorSpace monitorSpace;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private CreditPanel creditPanel;
        [Tooltip("IAudioVolume을 구현한 컴포넌트 (MixerAudioVolume)")]
        [SerializeField] private MonoBehaviour audioVolume;

        private KeyboardCancelInput _cancel;
        private TitleAbilityGuard _abilityGuard;
        private MenuSelector _playerSelector;
        private MenuSelector _cursorSelector;

        private void Awake()
        {
            var altStatus = player.GetComponent<AltModeController>();
            IInputLock inputLock = player.GetComponent<PlayerInputLock>().Lock;
            var body = player.GetComponent<Collider2D>();
            MenuIcon[] icons = FindObjectsByType<MenuIcon>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            var gate = new MenuGate(new IMenuBlocker[] { subScreens, transition });
            var focus = new MenuFocus();
            _cancel = new KeyboardCancelInput();
            _abilityGuard = new TitleAbilityGuard(player.GetComponent<PlayerAbilityGate>());

            subScreens.Initialize(_cancel);
            transition.Initialize(inputLock);

            var interact = new PlayerInteractInput(player.GetComponent<PlayerInput>(), inputLock);
            _playerSelector = new MenuSelector(AltPhase.Normal, altStatus,
                new StandingMenuFinder(icons, body, gate), interact, focus);
            _cursorSelector = new MenuSelector(AltPhase.Alt, altStatus,
                new HoverMenuFinder(icons, altStatus, monitorSpace, gate), new MouseClickInput(), focus);

            BindWindows(altStatus, body, interact, inputLock, icons);
        }

        private void BindWindows(AltModeController altStatus, Collider2D body, IButtonInput interact,
            IInputLock inputLock, MenuIcon[] icons)
        {
            if (audioVolume is not IAudioVolume volume)
            {
                Debug.LogError("[Title] audioVolume에 IAudioVolume 구현체(MixerAudioVolume)를 연결하세요.");
                return;
            }

            var screenMode = new ScreenModeSetting();
            screenMode.ApplySaved();

            var services = new WindowServices(altStatus, monitorSpace, body,
                new Rigidbody2DPlayerBody(player.GetComponent<Rigidbody2D>()), player.GetComponent<JJBControlGate>(),
                new PlayerWindowInput(player.GetComponent<PlayerInput>(), inputLock, interact),
                subScreens.CloseCurrent, new MenuPlatformSwitch(icons));
            settingsPanel.Bind(services, volume, screenMode);
            creditPanel.Bind(services);
        }

        private void OnEnable()
        {
            _cancel.Enable();
            _abilityGuard.Apply();
        }

        private void OnDisable()
        {
            _abilityGuard.Dispose();
            _cancel.Disable();
        }

        private void OnDestroy()
        {
            _cancel?.Dispose();
        }

        private void Update()
        {
            subScreens.Tick();
            _playerSelector.Tick();
            _cursorSelector.Tick();
        }
    }
}
