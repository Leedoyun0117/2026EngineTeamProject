using UnityEngine;

namespace JJB.Script
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;
        private static bool _isApplicationQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] {typeof(T).Name}에 접근했지만 앱 종료 중입니다. null을 반환합니다.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();

                        var mono = _instance as MonoSingleton<T>;
                        if (mono != null && mono.isDontDestroy)
                        {
                            DontDestroyOnLoad(obj);
                        }
                    }
                    else
                    {
                        var mono = _instance as MonoSingleton<T>;
                        if (mono != null && mono.isDontDestroy)
                        {
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }

        [SerializeField] protected bool isDontDestroy = false;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (isDontDestroy)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}