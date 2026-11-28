using LDY.Hud.Core;
using TMPro;
using UnityEngine;

namespace LDY.Hud {
    // 생각.txt 창. 외부에서는 Show(text)만 부른다. 다 쓰고 나서 잠시 뒤 닫힌다.
    public sealed class ThoughtWindow : MonoBehaviour, IHudTicker {
        [SerializeField] private GameObject window;
        [SerializeField] private TMP_Text body;
        [SerializeField] [Min(1f)] private float charactersPerSecond = 18f;
        [SerializeField] [Min(0f)] private float holdSeconds = 2.5f;
        [SerializeField] private string caret = "_";
        [SerializeField] [Min(1)] private int caretBlinkMilliseconds = 400;

        private Typewriter _typer;
        private FrameClock _blink;
        private float _hold;
        private bool _open;

        public void Show(string text) {
            _typer ??= new Typewriter(charactersPerSecond);
            _blink ??= new FrameClock(2, caretBlinkMilliseconds / 1000f);
            _typer.Start(text);
            _blink.Reset();
            _hold = holdSeconds;
            _open = true;
            if (window != null)
                window.SetActive(true);
            Render();
        }

        public void Tick(float deltaTime) {
            if (!_open)
                return;

            var changed = _typer.Tick(deltaTime) | _blink.Tick(deltaTime);
            if (_typer.IsDone) {
                _hold -= deltaTime;
                if (_hold <= 0f) {
                    Close();
                    return;
                }
            }

            if (changed)
                Render();
        }

        private void Awake() {
            if (!_open && window != null)
                window.SetActive(false);
        }

        private void Close() {
            _open = false;
            if (window != null)
                window.SetActive(false);
        }

        private void Render() {
            if (body != null)
                body.text = _blink.Frame == 0 ? _typer.Visible + caret : _typer.Visible;
        }
    }
}
