#if UNITY_EDITOR
using UnityEngine;

namespace LDY.Script
{
    // 테스트용: Alt 중(timeScale 0) 커서 클릭이 메뉴를 실행하는지 로그와 화면 표시로 확인한다.
    public class TitleClickProbe : MonoBehaviour
    {
        [SerializeField] private AltModeController alt;

        private MenuIcon[] _icons;
        private string _lastExecuted = "-";

        private void Start()
        {
            _icons = FindObjectsByType<MenuIcon>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (MenuIcon icon in _icons)
                icon.Executed += HandleExecuted;
        }

        private void OnDestroy()
        {
            if (_icons == null)
                return;

            foreach (MenuIcon icon in _icons)
            {
                if (icon != null)
                    icon.Executed -= HandleExecuted;
            }
        }

        private void HandleExecuted(MenuIcon icon)
        {
            _lastExecuted = $"{icon.name} (phase {alt.Phase}, timeScale {Time.timeScale})";
            Debug.Log($"[TitleProbe] 실행: {_lastExecuted}");
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 520, 80),
                $"A/D 이동, Space 점프, E 실행, Alt 홀드+클릭 실행, ESC 닫기\n" +
                $"phase: {alt.Phase}  timeScale: {Time.timeScale}\n" +
                $"cursor: {alt.CursorWorldPosition}\n" +
                $"마지막 실행: {_lastExecuted}");
        }
    }
}
#endif
