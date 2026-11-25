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
    }
}
