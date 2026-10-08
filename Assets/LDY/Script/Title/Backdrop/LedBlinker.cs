using UnityEngine;

namespace LDY.Script
{
    public class LedBlinker : MonoBehaviour
    {
        [SerializeField] private TitleClock clock;
        [SerializeField] private SpriteRenderer led;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private Color offColor = new Color(0.1f, 0.2f, 0.1f);
        [Tooltip("true면 점멸하지 않고 켜진 채로 둔다 (전원 LED)")]
        [SerializeField] private bool steady;
        [SerializeField, Min(0.01f)] private float minInterval = 0.08f;
        [SerializeField, Min(0.01f)] private float maxInterval = 0.8f;

        private bool _lit = true;
        private float _nextToggle;

        private void OnEnable()
        {
            _lit = true;
            led.color = onColor;
            _nextToggle = clock.Now + Random.Range(minInterval, maxInterval);
        }

        private void Update()
        {
            if (steady || clock.Now < _nextToggle)
                return;

            _lit = !_lit;
            led.color = _lit ? onColor : offColor;
            _nextToggle = clock.Now + Random.Range(minInterval, Mathf.Max(minInterval, maxInterval));
        }
    }
}
