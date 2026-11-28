using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 정지 스킬이 걸린 오브젝트 위 일시정지 배지. 포트가 없으면 이 요소만 끈다.
    public sealed class StoppedObjectsPresenter : MonoBehaviour, IHudPresenter, IHudTicker {
        [SerializeField] private UiPool badges;

        private IStoppedObjectsSource _source;

        public void Bind(HudPorts ports) {
            _source = ports.Stopped;
            if (badges != null)
                badges.HideAll();
        }

        public void Tick(float deltaTime) {
            if (_source != null && badges != null)
                badges.ShowAbove(_source.Stopped);
        }
    }
}
