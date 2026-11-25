using System;
using UnityEngine;

namespace LDY.Script {
    public interface IMenuAction {
        void Execute();
    }

    // 보조 화면 열림, 시작 전환 진행 등 메뉴 실행을 막아야 하는 상태.
    public interface IMenuBlocker {
        bool Blocking { get; }
    }

    public interface IMenuGate {
        bool CanExecute { get; }
    }

    public interface IButtonInput {
        bool Pressed { get; }
    }

    // 쓰는 동안에만 켜 두는 입력
    public interface IToggleableButtonInput : IButtonInput {
        void Enable();
        void Disable();
    }

    // 강조를 받는 대상. 대상이 자기 강조 방법을 안다(docs/IHighlightable-proposal.md).
    public interface IHighlightable {
        void SetHighlighted(bool highlighted);
    }

    // 메뉴 선택기(MenuSelector)가 고르고 실행하는 대상. 타이틀 아이콘(MenuIcon)과 선택 화면 파일 발판이 구현한다.
    public interface IMenuTarget : IHighlightable {
        bool IsUsable(IMenuGate gate);
        void Execute();
    }

    // 플레이어가 올라서서 고르는 대상
    public interface IStandTarget : IMenuTarget {
        bool IsStoodOn(Collider2D body);
    }

    // 현재 선택 가능한 메뉴 항목을 찾는다. 플레이어 발판 방식과 커서 hover 방식이 각각 구현한다.
    public interface IMenuTargetFinder {
        IMenuTarget Find();
    }

    // 게임 시작 이후 화면으로 넘어가는 훅.
    public interface ISceneLoader {
        void LoadGameScene();
    }

    public interface ISubScreen {
        void Open();
        void Close();
    }

    // 보조 화면이 ESC를 먼저 처리하는 확장 지점. true를 돌려주면 ESC를 소비한 것이라 화면을 닫지 않는다.
    public interface ISubScreenCancelHandler {
        bool HandleCancel();
    }

    public interface IMonitorSpace {
        bool PointerInside { get; }

        // 화면 픽셀을 모니터 콘텐츠 월드 좌표로 변환한다. 화면 밖이면 가장자리로 clamp하고 inside만 false.
        Vector2 ScreenToContentWorld(Vector2 screenPosition, out bool inside);
    }

    // 플레이어의 물리 점프를 막는다. 창의 행 이동이 Jump 입력을 대신 쓰는 동안 쓴다.
    public interface IJumpBlock {
        void SetJumpBlocked(bool blocked);
    }

    // 손 오브젝트 등 후속 연출이 구독할 확장 지점.
    public interface IDeskMouse {
        Vector2 Position { get; }
        event Action<Vector2> Moved;
    }
}