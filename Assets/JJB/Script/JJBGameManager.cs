using JJB.Script.Audio;
using JJB.Script.Level;
using UnityEngine;

namespace JJB.Script
{
    public class JJBGameManager : MonoSingleton<JJBGameManager>
    {
        [SerializeField] private JJBAudioManager jjbAudioManager;
        [SerializeField] private JJBLevelManager jjbLevelManager;

        public IAudioService JjbAudio => jjbAudioManager;
        public JJBLevelManager JjbLevel => jjbLevelManager;
    }
}