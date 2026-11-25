using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.StageSelect {
    public sealed class StageSelectInput : MonoBehaviour {
        public event Action<int> Moved;
        public event Action Executed;
        public event Action Back;

        private void Update() {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.leftArrowKey.wasPressedThisFrame) Moved?.Invoke(-1);
            if (keyboard.rightArrowKey.wasPressedThisFrame) Moved?.Invoke(1);
            if (keyboard.upArrowKey.wasPressedThisFrame) Moved?.Invoke(-FileGridView.Columns);
            if (keyboard.downArrowKey.wasPressedThisFrame) Moved?.Invoke(FileGridView.Columns);
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Executed?.Invoke();
            if (keyboard.escapeKey.wasPressedThisFrame) Back?.Invoke();
        }
    }
}
