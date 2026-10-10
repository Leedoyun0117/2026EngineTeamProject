using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 설정/크레딧 창이 공유하는 틀. SubScreenPanel의 OnOpened/OnClosed 확장 지점 위에 창 프레임, 제목 줄, 닫기(X),
    // 하단 "ESC 닫기", 발판, 입력 처리를 얹는다. 내용은 하위 클래스가 BuildBody에서 채운다.
    // 창 오브젝트는 처음 열 때 코드로 만든다. 스프라이트는 TitleArtSet, 글꼴은 font 슬롯에서 바꾼다.
    public abstract class WindowPanel : SubScreenPanel, ISubScreenCancelHandler
    {
        private const float CloseButtonWidth = 0.8f;
        private const float CloseButtonHeight = 0.7f;

        [SerializeField] private TitleArtSet art;
        [SerializeField] private TMP_FontAsset font;
        [Header("S 내려가기 (행 이동을 쓰지 않는 창만)")]
        [Tooltip("발판에서 겹침이 풀리기를 기다리는 시간(초). 지나면 아래 행에 착지해 멈춘 경우 복구한다. 하드 캡은 2초")]
        [SerializeField, Min(0.1f)] private float dropIgnoreSeconds = 1f;
        [Tooltip("내려가기 시작할 때 주는 아래 방향 속도")]
        [SerializeField, Range(-20f, 0f)] private float dropDownSpeed = -4f;
        [Tooltip("내려가기 복구 판정과 행 이동을 콘솔에 남긴다(에디터 전용)")]
        [SerializeField] private bool debugDropper;

        private readonly List<WindowWidget> _widgets = new List<WindowWidget>();
        private WindowServices _services;
        private WindowInteraction _interaction;
        private KeyboardButtonInput _dropInput;
        private int _openedFrame = -1;
        private bool _jumpBlocked;

        protected WindowServices Services => _services;
        protected abstract string TitleText { get; }
        protected abstract string ControlHint { get; }
        // 창마다 행 간격이 달라 높이와 발판 높이를 따로 가진다.
        protected abstract WindowLayout Layout { get; }
        // true면 Jump/S로 한 칸씩 행 이동하고 물리 점프와 내려가기는 쓰지 않는다. 크레딧처럼 서 있기만 하는 창은 false.
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
                    Debug.LogError($"[Window] {name}: Bind가 호출되지 않았습니다. TitleBootstrap 연결을 확인하세요.");
                    return false;
                }

                if (art == null)
                {
                    Debug.LogError($"[Window] {name}: art(TitleArtSet)가 비어 있습니다. 인스펙터에서 TitleArtSet을 연결하거나 Setup Title Scene을 다시 실행하세요.");
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
            OnWindowOpened();
        }

        protected sealed override void OnClosed()
        {
            _interaction?.Reset();
            _dropInput?.Disable();
            _services?.Platforms.SetEnabled(true);
            ReleaseRowControl();
        }

        // 창이 꺼지거나 씬이 바뀌면 진행 중인 이동과 점프 차단을 정상으로 되돌린다.
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
            if (!_jumpBlocked)
                return;

            _jumpBlocked = false;
            _services?.JumpBlock.SetJumpBlocked(false);
        }

        // 아이콘을 눌러 연 프레임의 입력(E, 클릭)이 창 안에서 다시 처리되지 않게 한다.
        private void Update()
        {
            if (_interaction == null || Time.frameCount == _openedFrame)
                return;

            float dt = Mathf.Min(Time.unscaledDeltaTime, TitleTuning.MaxDeltaTime);
            _interaction.Tick(dt);
            OnWindowTick(dt);
        }

        // 내려가기 복구는 물리 판정이라 FixedUpdate에서 검사한다.
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

            // 내용 위젯을 먼저 등록한다. 드롭다운 목록이 "ESC 닫기" 줄과 겹칠 때 목록이 먼저 판정되도록.
            BuildBody(kit, root, _widgets);

            _widgets.Add(new WindowButton(closeButton, Expand(closeButton, 0.08f),
                new Rect(1.5f, Layout.CloseTop - WindowStyle.ZoneBelow, WindowStyle.InnerRight - 1.5f,
                    Layout.ZoneHeight), _services.Close));

            var hint = new Rect(-1.2f, Layout.FooterTop + 0.05f, 2.4f, 0.5f);
            _widgets.Add(new WindowButton(hint, Expand(hint, 0.1f),
                new Rect(-3.5f, Layout.FooterTop - WindowStyle.ZoneBelow, 7f, Layout.ZoneHeight),
                _services.Close));

            // S(아래 행 / 내려가기)는 JJBInput을 쓰지 않고 LDY 쪽 InputAction으로 따로 읽는다.
            _dropInput = new KeyboardButtonInput("DropDown", "<Keyboard>/s");
            PlayerRowMover rows = null;
            PlatformDropper dropper = null;
            if (UsesRowMovement)
            {
                rows = new PlayerRowMover(_services.PlayerBody, _services.PlayerCollider, _services.InputLock, root,
                    kit.Platforms, Layout.Height, debugDropper);
            }
            else
            {
                dropper = new PlatformDropper(kit.Platforms, _services.PlayerCollider, _services.PlayerBody,
                    dropIgnoreSeconds, dropDownSpeed, debugDropper);
            }

            _interaction = new WindowInteraction(_widgets, _services, root, new FocusFrame(kit, root), rows, dropper, _dropInput);
        }

        private static Rect Expand(Rect rect, float amount)
        {
            return new Rect(rect.xMin - amount, rect.yMin - amount, rect.width + amount * 2f, rect.height + amount * 2f);
        }
    }
}
