using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
        [Tooltip("포커스 테두리 설정. 비우면 art의 focusFrame 스프라이트와 기본값을 쓴다")]
        [SerializeField] private FocusFrameStyle focusStyle;
        [Tooltip("창 이동(행 이동, 내려가기, 바닥 보호, 벽) 복구 판정을 콘솔에 남긴다(에디터 전용)")]
        [FormerlySerializedAs("debugDropper")]
        [SerializeField] private bool debugWindowMovement;
        [Header("창 경계 벽")]
        [Tooltip("벽 레이어 이름. 비우면 발판과 같은 Ground 레이어. 벽에 붙은 채 땅 판정이 켜지면 땅 판정에서 제외된 레이어로 바꾼다(충돌 매트릭스에서 Player와 충돌해야 함)")]
        [SerializeField] private string wallLayerName = "";

        private readonly List<WindowWidget> _widgets = new List<WindowWidget>();
        private WindowServices _services;
        private WindowInteraction _interaction;
        private FocusFrameStyle _defaultFocusStyle;
        private WindowSideWalls _walls;
        private int _openedFrame = -1;
        private bool _jumpBlocked;

        protected WindowServices Services => _services;
        protected abstract string TitleText { get; }
        protected abstract string ControlHint { get; }
        protected abstract WindowLayout Layout { get; }
        // false면 행 이동 대신 물리 점프 + S 내려가기(크레딧)
        protected virtual bool UsesRowMovement => true;
        // 행 이동을 쓰지 않는 창(S 내려가기)만 쓰는 값. 크레딧이 인스펙터 값으로 재정의
        protected virtual float DropIgnoreSeconds => 1f;
        protected virtual float DropDownSpeed => -4f;

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
            _services.DropInput.Enable();
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
            _services?.DropInput.Disable();
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
            if (_defaultFocusStyle != null)
                Destroy(_defaultFocusStyle);
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

            float dt = UnscaledTween.Delta;
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
                WindowStyle.Width, Layout.Height), WindowStyle.Order.Frame);

            float innerWidth = WindowStyle.InnerRight - WindowStyle.InnerLeft;
            kit.Stretched("TitleBar", root, art.windowTitlebar, new Rect(WindowStyle.InnerLeft,
                Layout.InnerTop - WindowStyle.TitleBarHeight, innerWidth, WindowStyle.TitleBarHeight),
                WindowStyle.Order.TitleBar);
            kit.Text("Title", root, new Vector2(WindowStyle.InnerLeft + WindowStyle.TitleTextInset, Layout.TitleBarCenterY),
                TitleText, WindowStyle.TitleFont, WindowStyle.TitleText, WindowStyle.Order.Content,
                TextAlignmentOptions.Left, WindowStyle.TitleTextWidth);

            var closeButton = new Rect(WindowStyle.InnerRight - WindowStyle.CloseButtonMargin - CloseButtonWidth,
                Layout.TitleBarCenterY - CloseButtonHeight * 0.5f, CloseButtonWidth, CloseButtonHeight);
            kit.Stretched("CloseButton", root, art.buttonClose, closeButton, WindowStyle.Order.Content);

            kit.Text("ControlHint", root, new Vector2(0f, Layout.HintY), ControlHint, WindowStyle.ControlHintFont,
                WindowStyle.HintText, WindowStyle.Order.Content, TextAlignmentOptions.Center, WindowStyle.ControlHintWidth);
            kit.Text("CloseHint", root, new Vector2(0f, Layout.FooterTop + WindowStyle.CloseHintLift), WindowText.CloseHint,
                WindowStyle.HintFont, WindowStyle.HintText, WindowStyle.Order.Content, TextAlignmentOptions.Center,
                WindowStyle.HintWidth);

            kit.Platform("CloseLedge", root, WindowStyle.InnerLeft, WindowStyle.InnerRight, Layout.CloseTop, true);
            kit.Platform("FooterLedge", root, WindowStyle.InnerLeft, WindowStyle.InnerRight, Layout.FooterTop, true);

            // 드롭다운 목록이 ESC 닫기 줄과 겹칠 때 목록이 먼저 판정되게 내용 위젯부터 등록
            BuildBody(kit, root, _widgets);

            _widgets.Add(new WindowButton(closeButton, Expand(closeButton, WindowStyle.CloseFocusPad),
                new Rect(WindowStyle.CloseZoneLeft, Layout.CloseTop - WindowStyle.ZoneBelow,
                    WindowStyle.InnerRight - WindowStyle.CloseZoneLeft, Layout.ZoneHeight), _services.Close));

            var hint = new Rect(WindowStyle.FooterHintX, Layout.FooterTop + WindowStyle.FooterHintLift,
                WindowStyle.FooterHintWidth, WindowStyle.FooterHintHeight);
            _widgets.Add(new WindowButton(hint, Expand(hint, WindowStyle.FooterHintFocusPad),
                new Rect(WindowStyle.FooterZoneX, Layout.FooterTop - WindowStyle.ZoneBelow, WindowStyle.FooterZoneWidth,
                    Layout.ZoneHeight), _services.Close));

            PlayerRowMover rows = null;
            PlatformDropper dropper = null;
            if (UsesRowMovement)
            {
                rows = new PlayerRowMover(_services.PlayerBody, _services.PlayerCollider, _services.InputLock,
                    kit.Platforms, debugWindowMovement);
            }
            else
            {
                dropper = new PlatformDropper(kit.Platforms, _services.PlayerCollider, _services.PlayerBody,
                    DropIgnoreSeconds, DropDownSpeed, debugWindowMovement);
            }

            var floor = new WindowFloorGuard(_services.PlayerBody, _services.PlayerCollider, kit.Platforms, debugWindowMovement);
            _interaction = new WindowInteraction(_widgets, _services, root, FocusFrame.Create(root, ResolveFocusStyle()), rows,
                dropper, _services.DropInput, floor);
            BuildWalls(kit, root);
        }

        private FocusFrameStyle ResolveFocusStyle()
        {
            if (focusStyle != null)
                return focusStyle;

            _defaultFocusStyle = FocusFrameStyle.CreateDefault(art.focusFrame);
            return _defaultFocusStyle;
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
                float bodyHeight = _services.PlayerCollider.bounds.size.y / Mathf.Max(TitleTuning.MinScale, root.lossyScale.y);
                float frameTop = Layout.InnerTop + WindowStyle.FrameBorder;
                ceiling = Mathf.Max(frameTop, Layout.CloseTop + bodyHeight + CeilingMargin);
                yMax = ceiling.Value + WindowStyle.CeilingWallGap;
            }

            var go = new GameObject("WindowWalls");
            go.transform.SetParent(root, false);
            _walls = go.AddComponent<WindowSideWalls>();
            _walls.Configure(layer, WindowStyle.InnerLeft, WindowStyle.InnerRight, yMin, yMax, ceiling,
                _services.PlayerCollider, _services.PlayerBody, debugWindowMovement);
            _walls.SetActive(false);
        }

        private static Rect Expand(Rect rect, float amount)
        {
            return new Rect(rect.xMin - amount, rect.yMin - amount, rect.width + amount * 2f, rect.height + amount * 2f);
        }
    }
}
