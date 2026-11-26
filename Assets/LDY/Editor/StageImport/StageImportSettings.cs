using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    [CreateAssetMenu(menuName = "LDY/Stage Import/Settings", fileName = "StageImportSettings")]
    internal sealed class StageImportSettings : ScriptableObject {
        public string jsonPath = StageImportPaths.DefaultJson;
        public string sceneFolder = StageImportPaths.DefaultSceneFolder;
        public bool firstIsTutorial = true;
        public string tutorialSuffix = "T";

        public SlotIdRule IdRule => new SlotIdRule(firstIsTutorial, tutorialSuffix);
    }
}
