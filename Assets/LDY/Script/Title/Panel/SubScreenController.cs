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
            if (_current != null && _cancel.Pressed)
                CloseCurrent();
        }

        public bool Open(ISubScreen screen)
        {
            if (_current != null)
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
