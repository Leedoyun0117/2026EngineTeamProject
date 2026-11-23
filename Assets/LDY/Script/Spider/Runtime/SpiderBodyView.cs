using UnityEngine;

namespace LDY.Spider {
    public sealed class SpiderBodyView : MonoBehaviour {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite glitchSprite;
        [SerializeField] private Sprite hurtSpriteA;
        [SerializeField] private Sprite hurtSpriteB;

        private SpiderSettings _settings;
        private GlitchClock _idleGlitch;
        private GlitchClock _fleeGlitch;
        private Vector3 _basePosition;
        private float _time;

        public void Initialize(SpiderSettings settings) {
            _settings = settings;
            _idleGlitch = new GlitchClock(settings.GlitchInterval, settings.GlitchDuration);
            _fleeGlitch = new GlitchClock(settings.FleeGlitchInterval, settings.FleeGlitchDuration);
            _basePosition = transform.localPosition;
        }

        public void Tick(float deltaTime, Vector2 offset, float leanDegrees, SpiderState state, float stateElapsed) {
            _time += deltaTime;
            _idleGlitch.Tick(deltaTime);
            _fleeGlitch.Tick(deltaTime);
            var bob = BodyBob.Offset(_time, _settings.BobAmplitude, _settings.BobPeriod);
            transform.localPosition = _basePosition + new Vector3(offset.x, bob + offset.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, leanDegrees);
            bodyRenderer.sprite = SpriteFor(BodyFaceRule.Pick(state, stateElapsed, _idleGlitch.IsGlitching,
                _fleeGlitch.IsGlitching, _settings.HurtFrameTime));
        }

        private Sprite SpriteFor(BodyFace face) {
            switch (face) {
                case BodyFace.Glitch: return glitchSprite;
                case BodyFace.HurtA: return hurtSpriteA != null ? hurtSpriteA : idleSprite;
                case BodyFace.HurtB: return hurtSpriteB != null ? hurtSpriteB : idleSprite;
                default: return idleSprite;
            }
        }
    }
}
