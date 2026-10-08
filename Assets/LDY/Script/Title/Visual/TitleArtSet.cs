using UnityEngine;

namespace LDY.Script
{
    // 타이틀 씬에 쓰는 도트 에셋 슬롯을 한 곳에 모은다. 셋업 도구는 씬을 새로 만들지만 이 에셋은 그대로 남으므로
    // 한 번 연결한 스프라이트가 재실행 후에도 유지된다. 비어 있는 슬롯만 파일 이름으로 자동 채워진다.
    [CreateAssetMenu(menuName = "LDY/Title Art Set", fileName = "TitleArtSet")]
    public class TitleArtSet : ScriptableObject
    {
        [Header("Desk")]
        public Sprite roomBackground;
        public Sprite monitorFrame;
        public Sprite pcTower;
        public Sprite fanLarge;
        public Sprite fanSmall;
        public Sprite keyboard;
        public Sprite mouse;

        [Header("Screen")]
        public Sprite screenWallpaper;
        public Sprite iconTile;
        public Sprite symbolSetting;
        public Sprite symbolCredit;
        public Sprite symbolGameStart;
        public Sprite symbolExit;

        [Header("Player")]
        public Sprite playerCursor;
    }
}
