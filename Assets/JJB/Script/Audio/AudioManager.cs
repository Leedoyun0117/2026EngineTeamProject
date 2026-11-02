using UnityEngine;

namespace JJB.Script.Audio
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null)
                return;
            
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
                return;

            sfxSource.PlayOneShot(clip);
        }
    }
}
