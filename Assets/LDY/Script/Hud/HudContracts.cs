namespace LDY.Hud {
    // 포트 묶음을 받아 자기 화면 요소를 그리는 쪽. 포트가 null이면 그 요소를 끈다.
    public interface IHudPresenter {
        void Bind(HudPorts ports);
    }

    // HudRoot가 LateUpdate마다 unscaledDeltaTime으로 부른다(Alt 중에도 동작).
    public interface IHudTicker {
        void Tick(float deltaTime);
    }

    // 이벤트가 없는 팀원 상태를 프레임마다 읽어 오는 어댑터.
    public interface IHudRefreshable {
        void Refresh();
    }
}
