using JJB.Script.Audio;
using JJB.Script.Level;
using UnityEngine;

namespace JJB.Script
{
    public class JJBGameManager : MonoBehaviour
    {
        public static JJBGameManager Instance;
        
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private LevelManager levelManager;

        public IAudioService Audio => audioManager;
        public LevelManager Level => levelManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}