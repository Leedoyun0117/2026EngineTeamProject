using System;
using UnityEngine;

namespace LDY.Script {
    // 플레이어 매달림 스프라이트의 손/발 위치 데이터(ScriptableObject). 픽셀 좌표를 월드 오프셋으로 바꾸는 데 쓴다.
    [CreateAssetMenu(menuName = "LDY/Rope Grip Profile", fileName = "RopeGripProfile")]
    public class RopeGripProfile : ScriptableObject {
        [Tooltip("캔버스 기준 픽셀, 왼쪽 위 원점. 스프라이트 피벗은 캔버스 아래 가운데")]
        [SerializeField] private Vector2 pivotPixel = new(32f, 64f);

        // 줄을 쥔 손의 픽셀 위치
        [SerializeField] private Vector2 handPixel = new(51f, 21f);
        // 프레임별 발 픽셀 위치 목록
        [SerializeField] private FootEntry[] feet = Array.Empty<FootEntry>();
        private FootTable _table;

        public Vector2 PivotPixel => pivotPixel;
        public Vector2 HandPixel => handPixel;

        public FootEntry[] Feet {
            get => feet;
            set {
                feet = value;
                _table = null;
            }
        }

        // 픽셀->월드 변환기(GripLayout) 생성
        public GripLayout LayoutFor(float pixelsPerUnit) {
            return new GripLayout(pixelsPerUnit, pivotPixel, handPixel);
        }

        // 애니메이션 프레임 번호로 발 픽셀 위치 조회
        public bool TryGetFoot(int frame, out Vector2 pixel) {
            return Table.TryGet(frame, out pixel);
        }

        private FootTable Table => _table ??= BuildTable();

        // 직렬화된 배열을 조회용 FootTable로 변환(지연 생성 후 캐시)
        private FootTable BuildTable() {
            var frames = new int[feet.Length];
            var pixels = new Vector2[feet.Length];
            for (var i = 0; i < feet.Length; i++) {
                frames[i] = feet[i].frame;
                pixels[i] = feet[i].pixel;
            }

            return new FootTable(frames, pixels);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            _table = null;
        }
#endif

        // (프레임 번호, 발 픽셀) 한 쌍
        [Serializable]
        public struct FootEntry {
            public int frame;
            public Vector2 pixel;
        }
    }
}
