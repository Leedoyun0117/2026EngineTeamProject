using UnityEngine;

namespace JJB.Script.Audio
{
    public interface IAudioService
    {
        void PlayBGM(AudioClip clip);
        void StopBGM();
        void PlaySFX(AudioClip clip);
    }
}