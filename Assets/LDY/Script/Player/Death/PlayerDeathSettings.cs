using LDY.Death;
using UnityEngine;

namespace LDY.Script {
    // 런타임에 붙는 PlayerDeath가 읽는 설정. LDY/Death/Build Death Settings 메뉴로 만든다.
    public sealed class PlayerDeathSettings : ScriptableObject {
        public const string ResourcePath = "LDY/PlayerDeathSettings";
        public const float DefaultRestartDelay = 0.3f;

        [SerializeField] private PlayerAnimSet animSet;
        [SerializeField] [Min(0f)] private float restartDelay = DefaultRestartDelay;
        [SerializeField] [Min(0f)] private float pullTime = DeathPull.DefaultDuration;

        public PlayerAnimSet AnimSet => animSet;
        public float RestartDelay => restartDelay;
        public float PullTime => pullTime;

        public static PlayerDeathSettings Load() {
            var settings = Resources.Load<PlayerDeathSettings>(ResourcePath);
            if (settings == null)
                Debug.LogWarning("[PlayerDeath] Resources/" + ResourcePath + " 없음. LDY/Death/Build Death Settings 실행 필요");

            return settings;
        }
    }
}
