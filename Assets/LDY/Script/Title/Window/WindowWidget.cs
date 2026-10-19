using System;
using UnityEngine;

namespace LDY.Script
{
    // 좌표는 모두 창 로컬. 마우스(Alt 커서)와 플레이어(E) 조작을 같은 요소가 받음
    public abstract class WindowWidget
    {
        public Rect PointerRect { get; protected set; }
        public Rect FocusRect { get; protected set; }
        public Rect ProximityRect { get; protected set; }

        // 클릭 뒤에도 마우스 입력을 독점하는 상태(열린 드롭다운)
        public virtual bool PointerEngaged => false;

        // true면 드래그가 끝날 때까지 이 요소가 입력을 받음
        public virtual bool PointerPress(Vector2 local) => false;
        public virtual void PointerMove(Vector2 local) { }
        public virtual void PointerDrag(Vector2 local) { }
        public virtual void PointerRelease() { }
        public virtual void PointerCancel() { }

        // true면 플레이어 조작을 이 요소가 잡음(슬라이더, 열린 드롭다운). 버튼은 바로 실행하고 false
        public virtual bool PlayerActivate() => false;
        // 잡는 동안 매 프레임 호출, false면 놓음
        public virtual bool PlayerTick(IWindowInput input, float dt) => false;
        public virtual void PlayerCancel() { }
        public virtual bool TryGetPlayerAnchor(out Vector2 local)
        {
            local = default;
            return false;
        }
    }

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
