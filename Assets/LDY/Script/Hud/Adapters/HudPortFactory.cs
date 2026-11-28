using UnityEngine;

namespace LDY.Hud.Adapters {
    // 플레이어 하나에 대한 실제 포트 묶음. 연결하지 못한 포트는 null로 두고 이유를 한 번 남긴다.
    public static class HudPortFactory {
        public static HudPorts Create(GameObject player, Object context) {
            var ability = new HtySkillAbilitySource();
            var absorb = JjbAbsorbTargetSource.TryCreate(player, out var absorbReason);
            if (absorb == null)
                Warn("absorb", absorbReason, context);

            var control = MovementControlTargetSource.TryCreate(player, out var controlReason);
            if (control == null)
                Warn("control", controlReason, context);

            var stopped = HtyStoppedObjectsSource.TryCreate();
            if (stopped == null)
                Warn("stopped", HtyStoppedObjectsSource.MissingReason, context);

            return new HudPorts(ability, absorb, control, new PlayerInputLockSource(player), stopped);
        }

        private static void Warn(string port, string reason, Object context) =>
            HudWarnings.Once("port:" + port + ":" + reason, $"{port} 포트 연결 안 됨 - {reason}. 이 HUD 요소는 꺼진다", context);
    }
}
