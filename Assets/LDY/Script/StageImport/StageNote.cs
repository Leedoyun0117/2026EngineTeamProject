using UnityEngine;

namespace LDY.StageImport {
    // 툴 메모 칸. EditorOnly 태그 오브젝트에 붙어 빌드에서 빠진다.
    public sealed class StageNote : MonoBehaviour {
        [SerializeField, TextArea] private string text;

        public string Text => text;

        public void SetText(string value) => text = value;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, text);
        }
#endif
    }
}
