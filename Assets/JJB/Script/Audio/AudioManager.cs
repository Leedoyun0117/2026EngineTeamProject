using UnityEngine;

namespace JJB.Script.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField]
        private AudioSource _bgmSource;

        [SerializeField]
        private AudioSource _sfxSource;

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

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null)
                return;
            
            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
                return;

            _bgmSource.Stop();
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
                return;

            _sfxSource.PlayOneShot(clip);
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
