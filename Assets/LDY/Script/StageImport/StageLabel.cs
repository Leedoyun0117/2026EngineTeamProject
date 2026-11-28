using UnityEngine;

namespace LDY.StageImport {
    // 임시 블록·영역 이름표. Scene 뷰 기즈모로만 그려 Game 뷰에는 나오지 않고, EditorOnly 태그 오브젝트에 붙어 빌드에서 빠진다.
    public sealed class StageLabel : MonoBehaviour {
        [SerializeField] private string text;

        public string Text => text;

        public void SetText(string value) => text = value;

#if UNITY_EDITOR
        private static GUIStyle _style;

        private void OnDrawGizmos() {
            if (UnityEditor.SceneView.currentDrawingSceneView == null || string.IsNullOrEmpty(text)) return;
            _style ??= new GUIStyle(UnityEditor.EditorStyles.whiteBoldLabel) { alignment = TextAnchor.LowerCenter };
            UnityEditor.Handles.Label(transform.position, text, _style);
        }
#endif
    }
}
