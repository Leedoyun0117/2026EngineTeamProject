namespace LDY.Background.Core {
    public readonly struct NotePose {
        public readonly float Rise;
        public readonly float Alpha;

        public NotePose(float rise, float alpha) {
            Rise = rise;
            Alpha = alpha;
        }
    }

    public static class NoteMotion {
        private const float FadeIn = 0.15f;
        private const float FadeOutStart = 0.6f;

        public static NotePose Evaluate(float age, float lifetime, float riseDistance) {
            if (lifetime <= 0f) return new NotePose(0f, 0f);
            var t = age / lifetime;
            if (t < 0f || t >= 1f) return new NotePose(0f, 0f);
            var alpha = t < FadeIn ? t / FadeIn : t > FadeOutStart ? (1f - t) / (1f - FadeOutStart) : 1f;
            return new NotePose(riseDistance * t, alpha);
        }
    }
}
