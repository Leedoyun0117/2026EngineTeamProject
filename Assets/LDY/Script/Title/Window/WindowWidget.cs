using System;
using UnityEngine;

namespace LDY.Script
{
    // 창 안의 조작 요소. 좌표는 모두 창 로컬. 마우스(Alt 커서)와 플레이어(E 키) 두 가지 조작 방식을 같은 요소가 받는다.
    public abstract class WindowWidget
    {
        // 커서 hover/클릭 판정과, 포커스 프레임을 그리는 범위
        public Rect PointerRect { get; protected set; }
        public Rect FocusRect { get; protected set; }
        // 플레이어 발바닥 점이 이 안에 있으면 "근처"로 본다
        public Rect ProximityRect { get; protected set; }

        // 클릭 뒤에도 마우스 입력을 독점하는 상태(열린 드롭다운)
        public virtual bool PointerEngaged => false;

        // 누르면 true를 돌려주고 드래그가 끝날 때까지 이 요소가 입력을 받는다
        public virtual bool PointerPress(Vector2 local) => false;
        public virtual void PointerMove(Vector2 local) { }
        public virtual void PointerDrag(Vector2 local) { }
        public virtual void PointerRelease() { }
        public virtual void PointerCancel() { }

        // E를 눌렀을 때. true면 플레이어 조작을 이 요소가 잡는다(슬라이더, 열린 드롭다운). 버튼은 바로 실행하고 false.
        public virtual bool PlayerActivate() => false;
        // 잡고 있는 동안 매 프레임. false를 돌려주면 놓는다.
        public virtual bool PlayerTick(IWindowInput input, float dt) => false;
        public virtual void PlayerCancel() { }
        // 잡는 동안 플레이어가 따라갈 위치(슬라이더 핸들)
        public virtual bool TryGetPlayerAnchor(out Vector2 local)
        {
            local = default;
            return false;
        }
    }

    // 닫기 버튼(X), "ESC 닫기" 같은 한 번 누르는 요소.
    public class WindowButton : WindowWidget
    {
        private readonly Action _action;

        public WindowButton(Rect pointer, Rect focus, Rect proximity, Action action)
        {
            PointerRect = pointer;
            FocusRect = focus;
            ProximityRect = proximity;
            _action = action;
        }

        public override bool PointerPress(Vector2 local)
        {
            _action();
            return false;
        }

        public override bool PlayerActivate()
        {
            _action();
            return false;
        }
    }
}
