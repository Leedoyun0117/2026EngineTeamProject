using System;
using UnityEngine;

namespace LDY.Script {
    [CreateAssetMenu(menuName = "LDY/Rope Grip Profile", fileName = "RopeGripProfile")]
    public class RopeGripProfile : ScriptableObject {
        [Tooltip("캔버스 기준 픽셀, 왼쪽 위 원점. 스프라이트 피벗은 캔버스 아래 가운데")]
        [SerializeField] private Vector2 pivotPixel = new(32f, 64f);

        [SerializeField] private Vector2 handPixel = new(51f, 21f);
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

        public GripLayout LayoutFor(float pixelsPerUnit) {
            return new GripLayout(pixelsPerUnit, pivotPixel, handPixel);
        }

        public bool TryGetFoot(int frame, out Vector2 pixel) {
            return Table.TryGet(frame, out pixel);
        }

        private FootTable Table => _table ??= BuildTable();

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

        [Serializable]
        public struct FootEntry {
            public int frame;
            public Vector2 pixel;
        }
    }
}
