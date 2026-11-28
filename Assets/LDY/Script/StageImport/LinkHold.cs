using System.Collections.Generic;
using UnityEngine;

namespace LDY.StageImport {
    // "연결 필요"로 임포터가 끈 스크립트·트리거 콜라이더 목록. 연결 이름표(EditorOnly)에 붙어 빌드에서 빠진다.
    public sealed class LinkHold : MonoBehaviour {
        [SerializeField] private List<Behaviour> scripts = new List<Behaviour>();
        [SerializeField] private List<Collider2D> triggers = new List<Collider2D>();

        public IReadOnlyList<Behaviour> Scripts => scripts;
        public IReadOnlyList<Collider2D> Triggers => triggers;

        public void HoldScript(Behaviour script) {
            if (script != null && !scripts.Contains(script)) scripts.Add(script);
        }

        public void HoldTrigger(Collider2D trigger) {
            if (trigger != null && !triggers.Contains(trigger)) triggers.Add(trigger);
        }
    }
}
