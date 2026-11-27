using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.PlayerLocate {
    // "Player" 태그 오브젝트 중 본체 하나를 찾는다. 태그로 플레이어를 찾는 LDY 런타임 코드는 모두 여기를 거친다.
    // 판별 규칙은 PrimaryPlayerRule에 있다. 비활성 오브젝트는 FindGameObjectsWithTag가 돌려주지 않는다.
    public static class PrimaryPlayerLocator {
        public const string Tag = "Player";

        public static GameObject Find(Object context = null) {
            var tagged = GameObject.FindGameObjectsWithTag(Tag);
            if (tagged.Length == 0) return null;

            var pick = PrimaryPlayerRule.Pick(Candidates(tagged));
            if (pick.IsAmbiguous)
                Debug.LogWarning($"[Player] '{Tag}' 태그 {tagged.Length}개 중 본체를 가리지 못해 첫 번째({tagged[pick.Index].name})를 쓴다: " +
                                 string.Join(", ", tagged.Select(go => go.name)), context);
            return tagged[pick.Index];
        }

        public static bool IsPrimary(GameObject player) {
            var tagged = GameObject.FindGameObjectsWithTag(Tag);
            return PrimaryPlayerRule.IsPrimary(Candidates(tagged), System.Array.IndexOf(tagged, player));
        }

        private static PlayerCandidate[] Candidates(GameObject[] tagged) {
            var candidates = new PlayerCandidate[tagged.Length];
            for (var i = 0; i < tagged.Length; i++)
                candidates[i] = new PlayerCandidate(tagged[i].name, HasActiveInput(tagged[i]));
            return candidates;
        }

        private static bool HasActiveInput(GameObject go) =>
            go.TryGetComponent<PlayerInput>(out var input) && input.isActiveAndEnabled;
    }
}
