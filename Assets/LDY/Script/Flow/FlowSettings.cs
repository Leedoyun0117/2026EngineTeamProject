using UnityEngine;

namespace LDY.Flow {
    // Resources/LDY/FlowSettings 에 하나만 둔다. 부트스트랩이 이걸 읽어 흐름 서비스를 만든다.
    [CreateAssetMenu(menuName = "LDY/Flow/FlowSettings", fileName = "FlowSettings")]
    public sealed class FlowSettings : ScriptableObject {
        public const string ResourcePath = "LDY/FlowSettings";

        public StageCatalog catalog;
        public UiSkin skin;
        public string titleScenePath = "Assets/Scenes/LDY_TitleScene.unity";
        public string selectScenePath = "Assets/LDY/Scenes/StageSelect.unity";
        [Min(0f)] public float loadingMinSeconds = 0.6f;
        [Min(0f)] public float restartDelaySeconds = 0.8f;

        [Header("다중 스테이지 씬")]
        [Tooltip("LDY 흐름과 같은 일(루트 생성·플레이어 배치)을 하는 팀원 매니저 타입의 전체 이름. 런타임에서만 끈다")]
        public string[] conflictingManagerTypes = { "JJB.Script.Level.JJBLevelManager" };

        [Tooltip("위 타입을 못 찾았을 때 경고에 함께 보여 줄 비슷한 컴포넌트의 이름 조각")]
        public string similarManagerHint = "LevelManager";
    }
}
