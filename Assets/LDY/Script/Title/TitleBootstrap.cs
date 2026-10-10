using System.Collections.Generic;
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
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

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

        // 연결 누락을 한 번에 모아 보여 준다. 필수 연결이 빠지면 조립하지 않고 이 컴포넌트를 끈다.
        // 창 관련(패널, audioVolume)만 빠진 경우는 시작 화면은 동작하고 해당 창만 열리지 않는다.
        private bool ValidateReferences()
        {
            var missing = new List<string>();
            bool fatal = false;

            void Require(bool ok, string message, bool isFatal)
            {
                if (ok)
                    return;

                missing.Add(message);
                fatal |= isFatal;
            }

            Require(player != null, "player", true);
            Require(subScreens != null, "subScreens", true);
            Require(transition != null, "transition", true);
            Require(monitorSpace != null, "monitorSpace", true);
            Require(settingsPanel != null, "settingsPanel", false);
            Require(creditPanel != null, "creditPanel", false);
            Require(audioVolume is IAudioVolume, "audioVolume (IAudioVolume 구현체, MixerAudioVolume)", false);

            if (missing.Count == 0)
                return true;

            Debug.LogError($"[Title] TitleBootstrap 연결 누락: {string.Join(", ", missing)}"
                + (fatal ? " — 필수 연결이 없어 시작 화면을 조립하지 않습니다." : " — 해당 창은 열리지 않습니다."), this);
            return !fatal;
        }

        private void BindWindows(AltModeController altStatus, Collider2D body, IButtonInput interact,
            IInputLock inputLock, MenuIcon[] icons)
        {
            if (audioVolume is not IAudioVolume volume || settingsPanel == null || creditPanel == null)
                return;

            var screenMode = new ScreenModeSetting();
            screenMode.ApplySaved();

            var services = new WindowServices(altStatus, monitorSpace, body,
                new Rigidbody2DPlayerBody(player.GetComponent<Rigidbody2D>()), player.GetComponent<JJBControlGate>(),
                new PlayerWindowInput(player.GetComponent<PlayerInput>(), inputLock, interact),
                subScreens.CloseCurrent, new MenuPlatformSwitch(icons),
                player.GetComponent<JJBControlGate>(), inputLock);
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
