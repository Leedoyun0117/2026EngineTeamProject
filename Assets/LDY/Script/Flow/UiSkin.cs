using TMPro;
using UnityEngine;

namespace LDY.Flow {
    // Win95 느낌 UI에 쓰는 스프라이트·폰트 묶음. 탐색기 화면과 로딩 창이 같이 쓴다.
    [CreateAssetMenu(menuName = "LDY/Flow/UiSkin", fileName = "UiSkin")]
    public sealed class UiSkin : ScriptableObject {
        public TMP_FontAsset font;
        [Tooltip("폰트의 픽셀 격자 크기와 같아야 선명하다 (Galmuri9 = 10, Galmuri11 = 12)")] public int fontSize = 10;

        [Header("Window")] public Sprite panelRaised;
        public Sprite windowFrame;
        public Sprite titlebar;
        public Sprite fieldSunken;
        public Sprite button;
        public Sprite buttonPressed;
        public Sprite titleMin;
        public Sprite titleMax;
        public Sprite titleClose;
        public Sprite progressSegment;
        public Sprite taskbar;
        public Sprite startIcon;

        [Header("Explorer")] public Sprite tab;
        public Sprite tabActive;
        public Sprite thumbFrame;
        public Sprite focusDotted;
        public Sprite folder;
        public Sprite folderOpen;
        public Sprite folderLocked;
        public Sprite computer;
        public Sprite disk;
        public Sprite ini;
        public Sprite exe;
        public Sprite missing;
        public Sprite lockBadge;
        public Sprite checkBadge;

        [Header("Dialog")] public Sprite iconError;
        public Sprite iconWarn;
        public Sprite iconInfo;

        [Header("Colors")] public Color desktop = new Color32(0x00, 0x80, 0x80, 0xFF);
        public Color face = new Color32(0xC0, 0xC0, 0xC0, 0xFF);
        public Color paper = Color.white;
        public Color text = Color.black;
        public Color titleText = Color.white;
        public Color selection = new Color32(0x00, 0x00, 0x80, 0xFF);
        public Color selectionText = Color.white;
        public Color disabled = new Color32(0x80, 0x80, 0x80, 0xFF);
    }
}
