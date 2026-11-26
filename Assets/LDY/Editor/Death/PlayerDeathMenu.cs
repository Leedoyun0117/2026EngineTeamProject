using UnityEditor;

namespace LDY.Script {
    internal static class PlayerDeathMenu {
        [MenuItem("LDY/Death/1. Build Death Settings", priority = 0)]
        private static void BuildSettings() => PlayerDeathSettingsBuilder.Run();

        [MenuItem("LDY/Death/2. Replace KillHitbox In LDY Prefabs", priority = 1)]
        private static void ReplaceKillHitbox() => LdyKillHitboxReplacer.Run();
    }
}
