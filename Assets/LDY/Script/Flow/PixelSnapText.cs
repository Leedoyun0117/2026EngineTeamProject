using TMPro;
using UnityEngine;

namespace LDY.Flow {
    // 글리프마다 왼쪽 아래 꼭짓점을 rect 모서리 기준 정수 좌표로 옮긴다.
    // 가운데 정렬에서 생기는 0.5px 어긋남으로 픽셀 폰트가 번지는 것을 막는다.
    // rect 모서리가 캔버스 정수 좌표이고 캔버스 배율이 정수(1280x720 = 2, 1920x1080 = 3)이면 화면 픽셀과 맞는다.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class PixelSnapText : MonoBehaviour {
        private TMP_Text _text;

        private void OnEnable() {
            _text = GetComponent<TMP_Text>();
            _text.OnPreRenderText += Snap;
            _text.SetVerticesDirty();
        }

        private void OnDisable() {
            if (_text != null) _text.OnPreRenderText -= Snap;
        }

        private void Snap(TMP_TextInfo info) {
            var origin = _text.rectTransform.rect.min;
            for (var i = 0; i < info.characterCount; i++) {
                var character = info.characterInfo[i];
                if (!character.isVisible) continue;
                var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                var first = character.vertexIndex;
                var corner = vertices[first];
                var delta = new Vector3(Offset(corner.x, origin.x), Offset(corner.y, origin.y), 0f);
                if (delta == Vector3.zero) continue;
                for (var v = 0; v < 4; v++) vertices[first + v] += delta;
            }
        }

        private static float Offset(float value, float origin) {
            var local = value - origin;
            return Mathf.Floor(local + 0.5f) - local;
        }
    }
}
