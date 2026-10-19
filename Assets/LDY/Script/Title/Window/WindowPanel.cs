using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 창 오브젝트는 처음 열 때 코드로 만듦. 내용은 하위 클래스가 BuildBody에서 채움
    public abstract class WindowPanel : SubScreenPanel, ISubScreenCancelHandler
    {
        private const float CloseButtonWidth = 0.8f;
        private const float CloseButtonHeight = 0.7f;
        // 벽 세로 범위의 여유(맨 아래 발판 아래, 맨 위 발판 위)
        private const float WallBelow = 1.5f;
        private const float WallAbove = 2.5f;
        // 물리 점프 창 위쪽 경계의 여유
        private const float CeilingMargin = 0.1f;

        [SerializeField] private TitleArtSet art;
        [SerializeField] private TMP_FontAsset font;
        [Header("S 내려가기 (행 이동을 쓰지 않는 창만)")]
        [Tooltip("발판에서 겹침이 풀리기를 기다리는 시간(초). 지나면 아래 행에 착지해 멈춘 경우 복구한다. 하드 캡은 2초")]
        [SerializeField, Min(0.1f)] private float dropIgnoreSeconds = 1f;
        [Tooltip("내려가기 시작할 때 주는 아래 방향 속도")]
        [SerializeField, Range(-20f, 0f)] private float dropDownSpeed = -4f;
        [Tooltip("내려가기 복구 판정과 행 이동을 콘솔에 남긴다(에디터 전용)")]
        [SerializeField] private bool debugDropper;
        [Header("창 경계 벽")]
        [Tooltip("벽 레이어 이름. 비우면 발판과 같은 Ground 레이어. 벽에 붙은 채 땅 판정이 켜지면 땅 판정에서 제외된 레이어로 바꾼다(충돌 매트릭스에서 Player와 충돌해야 함)")]
        [SerializeField] private string wallLayerName = "";

        private readonly List<WindowWidget> _widgets = new List<WindowWidget>();
        private WindowServices _services;
        private WindowInteraction _interaction;
        private KeyboardButtonInput _dropInput;
        private WindowSideWalls _walls;
        private int _openedFrame = -1;
        private bool _jumpBlocked;

        protected WindowServices Services => _services;
        protected abstract string TitleText { get; }
        protected abstract string ControlHint { get; }
        protected abstract WindowLayout Layout { get; }
        // false면 행 이동 대신 물리 점프 + S 내려가기(크레딧)
        protected virtual bool UsesRowMovement => true;

        public void Bind(WindowServices services)
        {
            _services = services;
        }

        public bool HandleCancel()
        {
            return _interaction != null && _interaction.HandleCancel();
        }

        public override bool CanOpen
        {
            get
            {
                if (_services == null)
                {
                    Debug.LogError($"[Window] {name}: Bind가 호출 안 됨, TitleBootstrap 연결 확인");
                    return false;
                }

                if (art == null)
                {
                    Debug.LogError($"[Window] {name}: art(TitleArtSet)가 비어 있음, 연결하거나 Setup Title Scene 다시 실행");
                    return false;
                }

                return true;
            }
        }

        protected sealed override void OnOpened()
        {
            if (_interaction == null)
                Build();

            _openedFrame = Time.frameCount;
            _dropInput.Enable();
            _services.Platforms.SetEnabled(false);
            _interaction.Reset();
            if (UsesRowMovement)
            {
                _services.JumpBlock.SetJumpBlocked(true);
                _jumpBlocked = true;
            }

            _interaction.Opened();
            _walls.SetActive(true);
            OnWindowOpened();
        }

        protected sealed override void OnClosed()
        {
            _interaction?.Reset();
            _dropInput?.Disable();
            _services?.Platforms.SetEnabled(true);
            ReleaseRowControl();
        }

        private void OnDisable()
        {
            ReleaseRowControl();
        }

        private void OnDestroy()
        {
            ReleaseRowControl();
            _dropInput?.Dispose();
        }

        private void ReleaseRowControl()
        {
            _interaction?.Abort();
            if (_walls != null)
                _walls.SetActive(false);
            if (!_jumpBlocked)
                return;

            _jumpBlocked = false;
            _services?.JumpBlock.SetJumpBlocked(false);
        }

        // 아이콘을 눌러 연 프레임의 입력(E, 클릭)이 창에서 또 처리되지 않게 건너뜀
        private void Update()
        {
            if (_interaction == null || Time.frameCount == _openedFrame)
                return;

            float dt = Mathf.Min(Time.unscaledDeltaTime, TitleTuning.MaxDeltaTime);
            _interaction.Tick(dt);
            OnWindowTick(dt);
        }

        private void FixedUpdate()
        {
            _interaction?.FixedTick(Time.fixedDeltaTime);
        }

        protected abstract void BuildBody(WindowKit kit, Transform root, List<WindowWidget> widgets);

        protected virtual void OnWindowOpened() { }

        protected virtual void OnWindowTick(float dt) { }

        private void Build()
        {
            var kit = new WindowKit(art, font);
            Transform root = ContentRoot;

            kit.Sliced("Frame", root, art.windowFrame, new Rect(-WindowStyle.Width * 0.5f, -Layout.Height * 0.5f,
                WindowStyle.Width, Layout.Height), 10);

            float innerWidth = WindowStyle.InnerRight - WindowStyle.InnerLeft;
            kit.Stretched("TitleBar", root, art.windowTitlebar, new Rect(WindowStyle.InnerLeft,
                Layout.InnerTop - WindowStyle.TitleBarHeight, innerWidth, WindowStyle.TitleBarHeight), 11);
            kit.Text("Title", root, new Vector2(WindowStyle.InnerLeft + 0.3f, Layout.TitleBarCenterY), TitleText,
                WindowStyle.TitleFont, WindowStyle.TitleText, 12, TextAlignmentOptions.Left, 4f);

            var closeButton = new Rect(WindowStyle.InnerRight - 0.05f - CloseButtonWidth,
                Layout.TitleBarCenterY - CloseButtonHeight * 0.5f, CloseButtonWidth, CloseButtonHeight);
            kit.Stretched("CloseButton", root, art.buttonClose, closeButton, 12);

            kit.Text("ControlHint", root, new Vector2(0f, Layout.HintY), ControlHint, WindowStyle.ControlHintFont,
                WindowStyle.HintText, 12, TextAlignmentOptions.Center, 8f);
            kit.Text("CloseHint", root, new Vector2(0f, Layout.FooterTop + 0.3f), WindowText.CloseHint,
                WindowStyle.HintFont, WindowStyle.HintText, 12, TextAlignmentOptions.Center, 3f);

            kit.Platform("CloseLedge", root, WindowStyle.InnerLeft, WindowStyle.InnerRight, Layout.CloseTop, true);
            kit.Platform("FooterLedge", root, WindowStyle.InnerLeft, WindowStyle.InnerRight, Layout.FooterTop, true);

            // 드롭다운 목록이 ESC 닫기 줄과 겹칠 때 목록이 먼저 판정되게 내용 위젯부터 등록
            BuildBody(kit, root, _widgets);

            _widgets.Add(new WindowButton(closeButton, Expand(closeButton, 0.08f),
                new Rect(1.5f, Layout.CloseTop - WindowStyle.ZoneBelow, WindowStyle.InnerRight - 1.5f,
                    Layout.ZoneHeight), _services.Close));

            var hint = new Rect(-1.2f, Layout.FooterTop + 0.05f, 2.4f, 0.5f);
            _widgets.Add(new WindowButton(hint, Expand(hint, 0.1f),
                new Rect(-3.5f, Layout.FooterTop - WindowStyle.ZoneBelow, 7f, Layout.ZoneHeight),
                _services.Close));

            // S는 JJBInput 대신 LDY 쪽 InputAction으로 따로 읽음
            _dropInput = new KeyboardButtonInput("DropDown", "<Keyboard>/s");
            PlayerRowMover rows = null;
            PlatformDropper dropper = null;
            if (UsesRowMovement)
            {
                rows = new PlayerRowMover(_services.PlayerBody, _services.PlayerCollider, _services.InputLock,
                    kit.Platforms, debugDropper);
            }
            else
            {
                dropper = new PlatformDropper(kit.Platforms, _services.PlayerCollider, _services.PlayerBody,
                    dropIgnoreSeconds, dropDownSpeed, debugDropper);
            }

            var floor = new WindowFloorGuard(_services.PlayerBody, _services.PlayerCollider, kit.Platforms, debugDropper);
            _interaction = new WindowInteraction(_widgets, _services, root, new FocusFrame(kit, root), rows, dropper,
                _dropInput, floor);
            BuildWalls(kit, root);
        }

        // 위쪽 경계는 물리 점프 창에만 만듦. 열릴 때까지는 꺼 둠
        private void BuildWalls(WindowKit kit, Transform root)
        {
            int layer = string.IsNullOrEmpty(wallLayerName) ? -1 : LayerMask.NameToLayer(wallLayerName);
            if (layer < 0)
                layer = kit.GroundLayer;

            float yMin = Layout.FooterTop - WallBelow;
            float yMax = Layout.CloseTop + WallAbove;
            float? ceiling = null;
            if (!UsesRowMovement)
            {
                // 맨 위 발판에 선 몸 높이는 확보하되 프레임 위쪽보다 낮게 안 잡음
                float bodyHeight = _services.PlayerCollider.bounds.size.y / Mathf.Max(0.0001f, root.lossyScale.y);
                float frameTop = Layout.InnerTop + WindowStyle.FrameBorder;
                ceiling = Mathf.Max(frameTop, Layout.CloseTop + bodyHeight + CeilingMargin);
                yMax = ceiling.Value + 1f;
            }

            var go = new GameObject("WindowWalls");
            go.transform.SetParent(root, false);
            _walls = go.AddComponent<WindowSideWalls>();
            _walls.Configure(layer, WindowStyle.InnerLeft, WindowStyle.InnerRight, yMin, yMax, ceiling,
                _services.PlayerCollider, _services.PlayerBody, debugDropper);
            _walls.SetActive(false);
        }

        private static Rect Expand(Rect rect, float amount)
        {
            return new Rect(rect.xMin - amount, rect.yMin - amount, rect.width + amount * 2f, rect.height + amount * 2f);
        }
    }
}
