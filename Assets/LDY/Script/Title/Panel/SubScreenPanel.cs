using UnityEngine;

namespace LDY.Script
{
    // 빈 보조 화면. 내용은 ContentRoot 아래에 채우거나, 상속해서 OnOpened/OnClosed를 재정의한다.
    public class SubScreenPanel : MonoBehaviour, ISubScreen
    {
        [SerializeField] private Transform contentRoot;

        public Transform ContentRoot => contentRoot != null ? contentRoot : transform;

        public void Open()
        {
            gameObject.SetActive(true);
            OnOpened();
        }

        public void Close()
        {
            OnClosed();
            gameObject.SetActive(false);
        }

        protected virtual void OnOpened() { }

        protected virtual void OnClosed() { }
    }
}
