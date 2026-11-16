using UnityEditor;

namespace LDY.Script {
    public static class PlayerVisualMenu {
        [MenuItem("LDY/Player Visual/Diagnose")]
        private static void Diagnose() {
            PlayerVisualDiagnostics.Run();
        }

        [MenuItem("LDY/Player Visual/Fix")]
        private static void Fix() {
            PlayerVisualFixer.Run();
        }
    }
}
