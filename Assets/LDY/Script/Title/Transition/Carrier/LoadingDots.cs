using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 점 3개를 붙여 두고 maxVisibleCharacters로 가려서 가운데 정렬 문구가 안 흔들림
    public class LoadingDots : MonoBehaviour
    {
        private const int MaxDots = 3;

        [SerializeField] private float interval = 0.4f;

        private TMP_Text _label;
        private int _baseCharacters;
        private float _startTime;

        public static LoadingDots Attach(TMP_Text label)
        {
            var dots = label.gameObject.AddComponent<LoadingDots>();
            dots._label = label;
            dots.enabled = false;
            return dots;
        }

        public void Play(string text)
        {
            _label.text = text + new string('.', MaxDots);
            // 리치 텍스트 태그가 섞여도 보이는 글자 수로 세려고
            _label.ForceMeshUpdate();
            _baseCharacters = Mathf.Max(0, _label.textInfo.characterCount - MaxDots);
            _startTime = Time.unscaledTime;
            enabled = true;
            Apply();
        }

        public void Stop()
        {
            enabled = false;
            _label.maxVisibleCharacters = int.MaxValue;
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            int step = Mathf.FloorToInt((Time.unscaledTime - _startTime) / Mathf.Max(0.01f, interval));
            _label.maxVisibleCharacters = _baseCharacters + step % MaxDots + 1;
        }
    }
}
