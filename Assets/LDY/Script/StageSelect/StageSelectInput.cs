using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.StageSelect {
    // 키보드 조작만 이벤트로 바꾼다. 대화상자·설정창이 열려 있을 때의 처리는 화면(StageSelectScreen)이 정한다.
    public sealed class StageSelectInput : MonoBehaviour {
        public event Action<int> Moved;
        public event Action Executed;
        public event Action Back;

        // 보기 탭: Tab = +1, Shift+Tab = -1
        public event Action<int> FilterCycled;

        // 월드 전환: PageDown = +1, PageUp = -1
        public event Action<int> WorldStepped;

        private void Update() {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.leftArrowKey.wasPressedThisFrame) Moved?.Invoke(-1);
            if (keyboard.rightArrowKey.wasPressedThisFrame) Moved?.Invoke(1);
            if (keyboard.upArrowKey.wasPressedThisFrame) Moved?.Invoke(-FileGridView.Columns);
            if (keyboard.downArrowKey.wasPressedThisFrame) Moved?.Invoke(FileGridView.Columns);
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Executed?.Invoke();
            if (keyboard.escapeKey.wasPressedThisFrame) Back?.Invoke();
            if (keyboard.tabKey.wasPressedThisFrame) FilterCycled?.Invoke(keyboard.shiftKey.isPressed ? -1 : 1);
            if (keyboard.pageUpKey.wasPressedThisFrame) WorldStepped?.Invoke(-1);
            if (keyboard.pageDownKey.wasPressedThisFrame) WorldStepped?.Invoke(1);
        }
    }
}
