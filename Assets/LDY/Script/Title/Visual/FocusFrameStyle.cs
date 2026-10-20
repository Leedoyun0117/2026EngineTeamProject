using UnityEngine;

namespace LDY.Script
{
    // FocusFrame의 모양 설정. 점선 모양과 두께는 스프라이트(9-slice 테두리)가 정하므로 바꾸려면 스프라이트를 교체한다
    [CreateAssetMenu(menuName = "LDY/Focus Frame Style", fileName = "FocusFrameStyle")]
    public class FocusFrameStyle : ScriptableObject
    {
        public const int DefaultSortingOrder = 15;

        public Sprite sprite;
        public Color tint = Color.white;
        public int sortingOrder = DefaultSortingOrder;
        [Tooltip("SpriteRenderer 기본값과 같은 Continuous가 기본. 창과 전환 연출 모두 이 값을 쓴다")]
        public SpriteTileMode tileMode = SpriteTileMode.Continuous;
        [Tooltip("대상 사각형 바깥으로 더 키우는 길이(대상 좌표계 단위)")]
        [Min(0f)] public float padding;

        // 에셋 없이 스프라이트만 있을 때 쓰는 기본값 객체. 호출한 쪽이 Destroy한다
        public static FocusFrameStyle CreateDefault(Sprite sprite, int sortingOrder = DefaultSortingOrder)
        {
            var style = CreateInstance<FocusFrameStyle>();
            style.hideFlags = HideFlags.DontSave;
            style.sprite = sprite;
            style.sortingOrder = sortingOrder;
            return style;
        }
    }
}
