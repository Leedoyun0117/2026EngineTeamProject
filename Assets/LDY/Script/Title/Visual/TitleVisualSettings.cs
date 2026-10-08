using System;
using UnityEngine;

namespace LDY.Script
{
    public enum TitleLogoMode
    {
        Text,
        Sprite
    }

    [Serializable]
    public class TitleIconSlot
    {
        public string id;
        public MenuIconVisual visual;
        public string labelText;
        [Tooltip("화면 대비 위치(0~1, 왼쪽 아래 기준). 아이콘 타일의 중심")]
        public Vector2 anchor = new Vector2(0.5f, 0.2f);
        public Sprite tileSprite;
        public Color tileTint = Color.black;
        public Sprite symbolSprite;
        public Color symbolTint = Color.white;
    }

    // 시작 화면의 교체 가능한 이미지/텍스트/배치 값을 한 곳에 모은다. 값만 가지며 적용은 TitleVisualApplier가 한다.
    public class TitleVisualSettings : MonoBehaviour
    {
        [Header("Wallpaper")]
        public Sprite wallpaperSprite;
        public Color wallpaperTint = Color.white;

        [Header("Logo")]
        public TitleLogoMode logoMode = TitleLogoMode.Text;
        public string logoText = "Cursor";
        [Min(1f)] public float logoFontSize = 18f;
        public Color logoColor = Color.white;
        public Sprite logoSprite;
        [Tooltip("스프라이트 로고의 폭(화면 폭 대비)")]
        [Range(0.05f, 1f)] public float logoSpriteWidthRatio = 0.3f;
        public Vector2 logoAnchor = new Vector2(0.5f, 0.8f);
        public Color underlineColor = new Color(0.5f, 0.05f, 0.1f);
        [Range(0f, 1f)] public float underlineWidthRatio = 0.3f;
        [Range(0.001f, 0.05f)] public float underlineThicknessRatio = 0.008f;
        [Tooltip("로고 중심에서 밑줄까지의 거리(화면 높이 대비)")]
        [Range(0f, 0.3f)] public float underlineOffsetRatio = 0.07f;

        [Header("Icons")]
        [Tooltip("타일 한 변의 길이(화면 폭 대비). 판정 영역도 이 값을 따른다")]
        [Range(0.03f, 0.3f)] public float iconSizeRatio = 0.13f;
        [Tooltip("타일 안 심볼의 크기(타일 대비)")]
        [Range(0.1f, 1f)] public float symbolRatio = 0.62f;
        [Min(0.5f)] public float labelFontSize = 3.2f;
        [Min(0f)] public float labelGap = 0.12f;
        public TitleIconSlot[] icons = Array.Empty<TitleIconSlot>();

        [Header("Player")]
        public Sprite playerSprite;
        public Color playerColor = Color.white;
        [Tooltip("시작 위치(화면 대비). y=0이 바닥에 서 있는 위치")]
        public Vector2 playerStartRatio = new Vector2(0.08f, 0f);
    }
}
