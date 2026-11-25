using LDY.Death;
using UnityEngine;

namespace LDY.Script {
    // 파괴된 플레이어 자리에서 death만 재생하고 재시작을 요청하는 시각 전용 대역.
    public sealed class DeathPuppet : MonoBehaviour {
        private DeathClipRunner _runner;
        private IStageRestarter _restarter;

        internal static DeathPuppet Spawn(in PlayerSpriteSnapshot snapshot, PlayerDeathSettings settings,
            IStageRestarter restarter) {
            var renderer = snapshot.CreateRenderer(nameof(DeathPuppet));
            var puppet = renderer.gameObject.AddComponent<DeathPuppet>();
            puppet._runner = new DeathClipRunner(renderer, settings, DeathClipOrder.Puppet);
            puppet._restarter = restarter;
            return puppet;
        }

        private void Update() {
            if (_runner != null && _runner.Tick(Time.deltaTime))
                _restarter.RequestRestart();
        }
    }
}
