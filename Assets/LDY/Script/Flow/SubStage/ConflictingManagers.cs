using System;
using System.Collections.Generic;
using System.Linq;
using LDY.Flow.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 같은 일(루트 생성·플레이어 배치)을 하는 팀원 매니저를 런타임에서만 끈다. 타입은 Assembly-CSharp라 이름으로 찾는다.
    // 끌 타입 이름은 FlowSettings에 있다. 못 찾으면(팀원이 이름을 바꿨을 수 있음) 경고를 한 번만 남긴다.
    public sealed class ConflictingManagers {
        private readonly string[] _typeNames;
        private readonly string _similarHint;
        private bool _warned;

        public ConflictingManagers(IEnumerable<string> typeNames, string similarHint) {
            _typeNames = (typeNames ?? Array.Empty<string>()).Where(n => !string.IsNullOrEmpty(n)).ToArray();
            _similarHint = similarHint;
        }

        public List<string> Disable(Scene scene, string stageId) {
            var disabled = new List<string>();
            if (_typeNames.Length == 0) return disabled;

            var seen = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true)) {
                if (behaviour == null) continue;
                var fullName = behaviour.GetType().FullName;
                seen.Add(fullName);
                if (!behaviour.enabled || Array.IndexOf(_typeNames, fullName) < 0) continue;
                behaviour.enabled = false;
                disabled.Add($"{behaviour.GetType().Name}({behaviour.name})");
            }

            var scan = ManagerScan.Evaluate(seen, _typeNames, _similarHint);
            if (!scan.FoundAny) WarnOnce(scene, stageId, scan);
            return disabled;
        }

        private void WarnOnce(Scene scene, string stageId, ManagerScan scan) {
            if (_warned) return;
            _warned = true;
            var similar = scan.Similar.Count > 0 ? $" 비슷한 컴포넌트: {string.Join(", ", scan.Similar)}." : "";
            Debug.LogWarning($"[Flow] {stageId}: 끌 팀원 매니저({string.Join(", ", _typeNames)})를 {scene.name}에서 찾지 못했다." +
                             similar + " 이름이 바뀌었으면 FlowSettings.conflictingManagerTypes를 고친다(이 경고는 한 번만 나온다)");
        }
    }
}
