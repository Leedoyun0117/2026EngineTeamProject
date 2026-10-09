using UnityEngine;
using UnityEngine.Audio;

namespace LDY.Script
{
    // 설정창 음량과 JJB 오디오가 닿는 유일한 지점. AudioManager 코드는 건드리지 않는다.
    // AudioManager에는 볼륨 API가 없으므로, 팀이 만든 AudioMixer(JJBAudioMixer)의 노출 파라미터로 BGM/SFX를 조절한다.
    // 이 값이 실제로 들리려면 AudioManager의 두 AudioSource 출력이 믹서의 BGM/SFX 그룹으로 연결되어 있어야 한다(에디터 설정).
    // 전체 음량은 AudioListener.volume에 적용하므로 연결 여부와 상관없이 항상 들린다.
    // AudioManager는 값을 저장하지 않으므로 저장은 이 어댑터가 PlayerPrefs로 한 번만 한다.
    public class MixerAudioVolume : MonoBehaviour, IAudioVolume
    {
        private const string KeyPrefix = "Settings.Volume.";
        private const float SaveDelay = 0.5f;
        private const float MinDecibel = -80f;

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string bgmParameter = "BGMVolume";
        [SerializeField] private string sfxParameter = "SFXVolume";
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.7f;

        private readonly float[] _values = new float[3];
        private bool _dirty;
        private float _lastChange;

        private void Awake()
        {
            for (int i = 0; i < _values.Length; i++)
            {
                var channel = (VolumeChannel)i;
                _values[i] = PlayerPrefs.GetFloat(KeyPrefix + channel, defaultVolume);
                Apply(channel);
            }
        }

        // 믹서 값이 있으면 믹서를 기준으로 읽는다. 다른 곳에서 바뀐 값도 슬라이더에 반영된다.
        public float Get(VolumeChannel channel)
        {
            if (channel == VolumeChannel.Master)
                return AudioListener.volume;

            if (mixer != null && mixer.GetFloat(ParameterOf(channel), out float decibel))
                return DecibelToLinear(decibel);

            return _values[(int)channel];
        }

        public void Set(VolumeChannel channel, float value)
        {
            _values[(int)channel] = Mathf.Clamp01(value);
            Apply(channel);
            PlayerPrefs.SetFloat(KeyPrefix + channel, _values[(int)channel]);
            _dirty = true;
            _lastChange = Time.unscaledTime;
        }

        // 드래그 중 매 프레임 디스크에 쓰지 않도록 변경이 멈춘 뒤에 저장한다.
        private void Update()
        {
            if (_dirty && Time.unscaledTime - _lastChange >= SaveDelay)
                Flush();
        }

        private void OnDestroy()
        {
            Flush();
        }

        private void OnApplicationQuit()
        {
            Flush();
        }

        private void Flush()
        {
            if (!_dirty)
                return;

            _dirty = false;
            PlayerPrefs.Save();
        }

        private void Apply(VolumeChannel channel)
        {
            float value = _values[(int)channel];
            if (channel == VolumeChannel.Master)
            {
                AudioListener.volume = value;
                return;
            }

            if (mixer != null)
                mixer.SetFloat(ParameterOf(channel), LinearToDecibel(value));
        }

        private string ParameterOf(VolumeChannel channel)
        {
            return channel == VolumeChannel.Bgm ? bgmParameter : sfxParameter;
        }

        private static float LinearToDecibel(float linear)
        {
            return linear <= 0.0001f ? MinDecibel : Mathf.Max(MinDecibel, 20f * Mathf.Log10(linear));
        }

        private static float DecibelToLinear(float decibel)
        {
            return decibel <= MinDecibel ? 0f : Mathf.Pow(10f, decibel / 20f);
        }
    }
}
