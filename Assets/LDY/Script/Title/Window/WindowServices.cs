using System;
using UnityEngine;

namespace LDY.Script
{
    public interface IWindowInput
    {
        float Horizontal { get; }
        bool InteractPressed { get; }
        bool JumpPressed { get; }
        // 이번 프레임에 눌린 W(+1) / S(-1), 없으면 0
        int VerticalStep { get; }
        // 누르고 있는 W(+1) / S(-1)
        float VerticalHeld { get; }
        // 휠 한 칸당 +1(위) / -1(아래)
        float Scroll { get; }
        bool PointerPressed { get; }
        bool PointerHeld { get; }
    }

    public class WindowServices
    {
        public WindowServices(IAltStatus alt, IMonitorSpace monitor, Collider2D playerCollider, IPlayerBody playerBody,
            IPlayerControl playerControl, IWindowInput input, Action close, MenuPlatformSwitch platforms,
            IJumpBlock jumpBlock, IInputLock inputLock)
        {
            Alt = alt;
            Monitor = monitor;
            PlayerCollider = playerCollider;
            PlayerBody = playerBody;
            PlayerControl = playerControl;
            Input = input;
            Close = close;
            Platforms = platforms;
            JumpBlock = jumpBlock;
            InputLock = inputLock;
        }

        public IAltStatus Alt { get; }
        public IMonitorSpace Monitor { get; }
        public Collider2D PlayerCollider { get; }
        public IPlayerBody PlayerBody { get; }
        public IPlayerControl PlayerControl { get; }
        public IWindowInput Input { get; }
        public Action Close { get; }
        public MenuPlatformSwitch Platforms { get; }
        public IJumpBlock JumpBlock { get; }
        public IInputLock InputLock { get; }
    }

    // 한글 폰트 글리프 점검(셋업 도구)에도 씀
    public static class WindowText
    {
        public const string SettingTitle = "설정";
        public const string CreditTitle = "Credit";
        public const string CloseHint = "ESC 닫기";
        public const string SettingControlHint = "Space 위 · S 아래 · E 조작 · ESC 닫기";
        public const string CreditControlHint = "W/S 스크롤 · ESC 닫기";
        public const string MasterVolume = "전체 음량";
        public const string BgmVolume = "배경음";
        public const string SfxVolume = "효과음";
        public const string ScreenMode = "화면 모드";
        public const string FullScreen = "전체 화면";
        public const string Windowed = "창 모드";

        public static readonly string[] All =
        {
            SettingTitle, CreditTitle, CloseHint, SettingControlHint, CreditControlHint, MasterVolume, BgmVolume, SfxVolume, ScreenMode, FullScreen,
            Windowed, "0123456789%"
        };
    }
}
