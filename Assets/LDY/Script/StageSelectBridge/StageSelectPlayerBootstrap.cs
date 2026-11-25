using System.Collections.Generic;
using LDY.StageSelect;
using LDY.StageSelect.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // 선택 화면 안을 AltPlayer가 걸어 다니며 파일을 실행하게 조립한다. 조립과 Update 위임만 한다(TitleBootstrap과 같은 역할).
    // 타이틀과 같은 부품: MenuSelector(Normal) + StandingMenuFinder + PlayerInteractInput + MenuFocus + MenuGate + TitleAbilityGuard
    // 화면(StageSelectScreen.Start)이 뷰를 다 만든 뒤에 돌아야 해서 순서를 늦춘다.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class StageSelectPlayerBootstrap : MonoBehaviour {
        [SerializeField] private GameObject player;
        [SerializeField] private StageSelectScreen screen;
        [SerializeField] private PixelCameraFit cameraFit;

        private readonly Vector3[] _corners = new Vector3[4];
        private TitleAbilityGuard _abilityGuard;
        private IAltStatus _alt;
        private FileBoard _board;
        private Collider2D _body;
        private IFileBoardHost _host;
        private FileBoardInputLock _modalLock;
        private IPlayerBody _playerBody;
        private MenuSelector _selector;

        private void Start() {
            if (!ValidateReferences()) {
                enabled = false;
                return;
            }

            _host = screen;
            if (!_host.IsReady) {
                // 흐름 서비스가 없으면 화면이 이미 오류를 남겼다. 플레이어만 세워 둔다.
                enabled = false;
                return;
            }

            _alt = player.GetComponent<AltModeController>();
            _body = player.GetComponent<Collider2D>();
            _playerBody = new Rigidbody2DPlayerBody(player.GetComponent<Rigidbody2D>());
            var inputLock = player.GetComponent<PlayerInputLock>().Lock;

            _board = new FileBoard(GroundLayer());
            var gate = new MenuGate(new IMenuBlocker[] { new FileBoardBlocker(_host) });
            var interact = new PlayerInteractInput(player.GetComponent<PlayerInput>(), inputLock);
            _selector = new MenuSelector(AltPhase.Normal, _alt, new StandingMenuFinder(_board.Targets, _body, gate),
                interact, new MenuFocus());
            _modalLock = new FileBoardInputLock(_host, inputLock);
            _abilityGuard = new TitleAbilityGuard(player.GetComponent<PlayerAbilityGate>());
            _abilityGuard.Apply();

            _host.LayoutChanged += Rebuild;
            cameraFit.Changed += Rebuild;
            cameraFit.Apply();
            Rebuild();
        }

        private void Update() {
            _modalLock.Tick();
            _selector.Tick();
        }

        private void OnDestroy() {
            if (_host != null) _host.LayoutChanged -= Rebuild;
            if (cameraFit != null) cameraFit.Changed -= Rebuild;
            _modalLock?.Dispose();
            _abilityGuard?.Dispose();
        }

        // 그리드가 바뀌면(필터 탭·월드 전환·해상도) 발판을 다시 만들고 플레이어를 현재 선택 파일 위에 세운다.
        // Alt 중에는 Alt가 몸 위치를 쥐고 있어서 옮기지 않는다.
        private void Rebuild() {
            var plan = BoardPlanner.Plan(Measure(_host.FileArea), _host.VisibleIds, BoardMetrics.WallThickness);
            _board.Rebuild(plan, cameraFit.Space, _host);
            if (_alt.Phase == AltPhase.Normal) StandOn(_board.Find(_host.SelectedId));
        }

        private void StandOn(FilePlatformTarget target) {
            if (target == null) return;
            var pivotToFoot = _playerBody.Position.y - _body.bounds.min.y;
            _playerBody.Velocity = Vector2.zero;
            _playerBody.Position = target.StandPoint + new Vector2(0f, TitleTuning.StandGap + pivotToFoot);
            _playerBody.SyncTransforms();
        }

        // 파일 목록 패널을 캔버스 픽셀(왼쪽 위 원점)로 잰다.
        private PixelRect Measure(RectTransform area) {
            var canvas = cameraFit.Canvas;
            area.GetWorldCorners(_corners);
            Vector2 min = canvas.InverseTransformPoint(_corners[0]);
            Vector2 max = canvas.InverseTransformPoint(_corners[2]);
            var rect = canvas.rect;
            return new PixelRect(min.x - rect.xMin, rect.yMax - max.y, max.x - min.x, max.y - min.y);
        }

        private static int GroundLayer() {
            var layer = LayerMask.NameToLayer("Ground");
            if (layer >= 0) return layer;
            Debug.LogWarning("[StageSelect] Ground 레이어가 없어서 발판을 Default 레이어에 만듦");
            return 0;
        }

        private bool ValidateReferences() {
            var missing = new List<string>();
            if (player == null) missing.Add("player");
            if (screen == null) missing.Add("screen");
            if (cameraFit == null) missing.Add("cameraFit");
            if (player != null) {
                if (player.GetComponent<AltModeController>() == null) missing.Add("player.AltModeController");
                if (player.GetComponent<PlayerInputLock>() == null) missing.Add("player.PlayerInputLock");
                if (player.GetComponent<PlayerInput>() == null) missing.Add("player.PlayerInput");
                if (player.GetComponent<PlayerAbilityGate>() == null) missing.Add("player.PlayerAbilityGate");
                if (player.GetComponent<Rigidbody2D>() == null) missing.Add("player.Rigidbody2D");
                if (player.GetComponent<Collider2D>() == null) missing.Add("player.Collider2D");
            }

            if (missing.Count == 0) return true;
            Debug.LogError("[StageSelect] StageSelectPlayerBootstrap 연결 누락: " + string.Join(", ", missing)
                                                                           + " — LDY/Flow/Build Stage Select Scene 다시 실행", this);
            return false;
        }
    }
}
