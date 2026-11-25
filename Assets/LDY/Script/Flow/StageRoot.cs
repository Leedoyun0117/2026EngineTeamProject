using UnityEngine;

namespace LDY.Flow {
    // 템플릿 스테이지 씬마다 하나. 스테이지 id를 들고 카탈로그에 있는지 확인한다.
    [DisallowMultipleComponent]
    public sealed class StageRoot : MonoBehaviour {
        [SerializeField] private string stageId;

        public static StageRoot Current { get; private set; }
        public string StageId => stageId;

        private void Awake() {
            if (Current != null && Current != this)
                Debug.LogWarning($"[Flow] StageRoot가 둘 이상이다: {Current.name}, {name}", this);
            Current = this;
        }

        private void Start() {
            if (GameObject.FindGameObjectWithTag(PlayerMatch.DefaultTag) == null)
                Debug.LogWarning($"[Flow] {stageId}: '{PlayerMatch.DefaultTag}' 태그 플레이어가 없다", this);
            var catalog = SceneFlow.Instance != null ? SceneFlow.Instance.Catalog : null;
            if (catalog != null && catalog.Find(stageId) == null)
                Debug.LogWarning($"[Flow] StageRoot id '{stageId}'가 카탈로그에 없다", this);
        }

        private void OnDestroy() {
            if (Current == this) Current = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
