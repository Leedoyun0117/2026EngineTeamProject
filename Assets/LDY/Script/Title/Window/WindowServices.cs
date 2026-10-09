using System;
using UnityEngine;

namespace LDY.Script
{
    // 창이 쓰는 입력. 플레이어 조작(A/D, E, Space, W/S)과 Alt 커서의 마우스 입력을 한곳에서 읽는다.
    public interface IWindowInput
    {
        // A/D (-1~1)
        float Horizontal { get; }
        bool InteractPressed { get; }
        bool JumpPressed { get; }
        // 이번 프레임에 눌린 W(+1) / S(-1). 없으면 0
        int VerticalStep { get; }
        // 누르고 있는 W(+1) / S(-1)
        float VerticalHeld { get; }
        // 마우스 휠 한 칸당 +1(위) / -1(아래)
        float Scroll { get; }
        bool PointerPressed { get; }
        bool PointerHeld { get; }
    }

    // 창이 외부에서 받는 의존성 묶음. TitleBootstrap이 만들어 Bind로 넘긴다.
    public class WindowServices
    {
        public WindowServices(IAltStatus alt, IMonitorSpace monitor, Collider2D playerCollider, IPlayerBody playerBody,
            IPlayerControl playerControl, IWindowInput input, Action close, MenuPlatformSwitch platforms)
        {
            Alt = alt;
            Monitor = monitor;
            PlayerCollider = playerCollider;
            PlayerBody = playerBody;
            PlayerControl = playerControl;
            Input = input;
            Close = close;
            Platforms = platforms;
        }

        public IAltStatus Alt { get; }
        public IMonitorSpace Monitor { get; }
        public Collider2D PlayerCollider { get; }
        public IPlayerBody PlayerBody { get; }
        public IPlayerControl PlayerControl { get; }
        public IWindowInput Input { get; }
        public Action Close { get; }
        public MenuPlatformSwitch Platforms { get; }
    }

    // 창에 보이는 글자. 한글 폰트 글리프 점검(셋업 도구)에도 쓴다.
    public static class WindowText
    {
        public const string SettingTitle = "설정";
        public const string CreditTitle = "Credit";
        public const string CloseHint = "ESC 닫기";
        public const string SettingControlHint = "E 조작 · S 내려가기 · ESC 닫기";
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
