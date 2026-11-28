using LDY.Hud.Adapters;
using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Hud {
    // 조립 지점. 플레이어를 찾아 포트를 만들고 자식 프레젠터에 나눠 준다. 디버그 피더가 포트를 덮어쓸 수 있다.
    [DisallowMultipleComponent]
    public sealed class HudRoot : MonoBehaviour {
        [SerializeField] [Min(0.05f)] private float playerSearchInterval = 0.5f;

        private IHudPresenter[] _presenters;
        private IHudTicker[] _tickers;
        private HudPorts _ports = HudPorts.Empty;
        private GameObject _player;
        private bool _overridden;
        private float _searchTimer;

        private void Awake() {
            _presenters = GetComponentsInChildren<IHudPresenter>(true);
            _tickers = GetComponentsInChildren<IHudTicker>(true);
            Bind(HudPorts.Empty);
        }

        private void LateUpdate() {
            var deltaTime = Time.unscaledDeltaTime;
            if (!_overridden)
                TrackPlayer(deltaTime);

            _ports.Refresh();
            foreach (var ticker in _tickers)
                ticker.Tick(deltaTime);
        }

        private void OnDestroy() {
            _ports.Dispose();
        }

        // null을 주면 실제 플레이어 연결로 돌아간다.
        public void Override(HudPorts ports) {
            _overridden = ports != null;
            _player = null;
            _searchTimer = 0f;
            Bind(ports ?? HudPorts.Empty);
        }

        private void TrackPlayer(float deltaTime) {
            if (_player != null)
                return;

            if (!ReferenceEquals(_ports, HudPorts.Empty))
                Bind(HudPorts.Empty);

            _searchTimer -= deltaTime;
            if (_searchTimer > 0f)
                return;

            _searchTimer = playerSearchInterval;
            _player = PrimaryPlayerLocator.Find(this);
            if (_player != null)
                Bind(HudPortFactory.Create(_player, this));
        }

        private void Bind(HudPorts ports) {
            if (!ReferenceEquals(_ports, ports))
                _ports.Dispose();

            _ports = ports;
            foreach (var presenter in _presenters)
                presenter.Bind(ports);
        }
    }
}
