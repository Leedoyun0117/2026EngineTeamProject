using UnityEngine;

namespace LDY.Script {
    // FocusFrame의 모양 설정. 점선 모양과 두께는 스프라이트(9-slice 테두리)가 정하므로 바꾸려면 스프라이트를 교체한다
    [CreateAssetMenu(menuName = "LDY/Focus Frame Style", fileName = "FocusFrameStyle")]
    public class FocusFrameStyle : ScriptableObject {
        public const int DefaultSortingOrder = 15;

        public Sprite sprite;
        public Color tint = Color.white;
        public int sortingOrder = DefaultSortingOrder;

        [Tooltip("테두리는 Tiled로 그린다(점이 늘어나지 않음). Continuous가 기본. 창과 전환 연출 모두 이 값을 쓴다. " +
                 "스프라이트는 Mesh Type Full Rect + 9-slice border가 있어야 한다")]
        public SpriteTileMode tileMode = SpriteTileMode.Continuous;

        [Tooltip("점 + 간격 한 주기(스프라이트 픽셀). 테두리 크기를 이 값의 정수배로 맞춰 끝 점이 잘리지 않게 한다. 0이면 맞추지 않음")]
        [Min(0f)]
        public float dotPeriodPixels = 16f;

        [Tooltip("대상 사각형 바깥으로 더 키우는 길이(대상 좌표계 단위)")] [Min(0f)]
        public float padding;

        // 에셋 없이 스프라이트만 있을 때 쓰는 기본값 객체. 호출한 쪽이 Destroy한다
        public static FocusFrameStyle CreateDefault(Sprite sprite, int sortingOrder = DefaultSortingOrder) {
            var style = CreateInstance<FocusFrameStyle>();
            style.hideFlags = HideFlags.DontSave;
            style.sprite = sprite;
            style.sortingOrder = sortingOrder;
            return style;
        }
    }
}