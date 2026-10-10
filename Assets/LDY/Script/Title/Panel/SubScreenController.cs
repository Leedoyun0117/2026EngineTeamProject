using UnityEngine;

namespace LDY.Script
{
    // 보조 화면은 한 번에 하나만 연다. 닫는 프레임에는 메뉴 입력을 막아 닫기와 실행이 겹치지 않게 한다.
    public class SubScreenController : MonoBehaviour, IMenuBlocker
    {
        [SerializeField] private SubScreenPanel[] panels;

        private IButtonInput _cancel;
        private ISubScreen _current;
        private int _closedFrame = -1;

        public bool Blocking => _current != null || _closedFrame == Time.frameCount;

        public void Initialize(IButtonInput cancel)
        {
            _cancel = cancel;
            foreach (SubScreenPanel panel in panels)
                panel.gameObject.SetActive(false);
        }

        public void Tick()
        {
            if (_current == null || !_cancel.Pressed)
                return;

            if (_current is ISubScreenCancelHandler handler && handler.HandleCancel())
                return;

            CloseCurrent();
        }

        public bool Open(ISubScreen screen)
        {
            if (_current != null)
                return false;

            // 열 수 없는 화면이면 _current를 잡지 않아 빈 창이 열린 채 ESC로만 닫히는 상태를 막는다.
            if (screen is SubScreenPanel panel && !panel.CanOpen)
                return false;

            _current = screen;
            screen.Open();
            return true;
        }

        public void CloseCurrent()
        {
            if (_current == null)
                return;

            ISubScreen closing = _current;
            _current = null;
            _closedFrame = Time.frameCount;
            closing.Close();
        }
    }
}
