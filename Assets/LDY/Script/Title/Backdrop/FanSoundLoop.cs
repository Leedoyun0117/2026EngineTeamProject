using UnityEngine;

namespace LDY.Script
{
    // 은은한 팬 소리 루프. 클립이 비어 있으면 임시 허밍을 생성해 쓴다.
    [RequireComponent(typeof(AudioSource))]
    public class FanSoundLoop : MonoBehaviour
    {
        [SerializeField] private TitleClock clock;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.12f;

        private AudioSource _source;
        private AudioClip _generated;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.volume = volume;
            if (clip == null)
                _generated = CreateHum();
            _source.clip = clip != null ? clip : _generated;
        }

        private void OnEnable()
        {
            _source.Play();
        }

        private void OnDestroy()
        {
            if (_generated != null)
                Destroy(_generated);
        }

        // 오디오는 timeScale의 영향을 받지 않으므로, 배경 연출을 멈추는 설정일 때만 직접 멈춘다.
        private void Update()
        {
            bool shouldPlay = clock.UseUnscaledTime || Time.timeScale > 0f;
            if (shouldPlay && !_source.isPlaying)
                _source.UnPause();
            else if (!shouldPlay && _source.isPlaying)
                _source.Pause();
        }

        // 길이 1초 동안 정수 주기만 포함하는 사인 합성이라 루프 이음새가 없다.
        private static AudioClip CreateHum()
        {
            const int sampleRate = 22050;
            var samples = new float[sampleRate];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / sampleRate;
                float tone = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.5f
                             + Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.3f
                             + Mathf.Sin(2f * Mathf.PI * 181f * t) * 0.15f;
                float flutter = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 3f * t);
                samples[i] = tone * flutter * 0.5f;
            }

            AudioClip hum = AudioClip.Create("FanHum", samples.Length, 1, sampleRate, false);
            hum.SetData(samples, 0);
            return hum;
        }
    }
}
