using UnityEngine;

namespace LDY.Script
{
    // 클립 하나를 재생한다. 시간은 호출한 쪽이 넘긴 dt를 그대로 쓴다.
    public class SpriteFramePlayer
    {
        private const float MinFrameTime = 0.001f;

        private PlayerAnimClip _clip;
        private float _elapsed;

        public int Frame { get; private set; }
        public bool Finished { get; private set; }
        public Sprite Sprite => _clip != null ? _clip.frames[Frame] : null;

        public void Play(PlayerAnimClip clip, int startFrame = 0)
        {
            _clip = clip != null && clip.IsValid ? clip : null;
            Frame = _clip != null ? Mathf.Clamp(startFrame, 0, _clip.frames.Length - 1) : 0;
            _elapsed = 0f;
            Finished = false;
        }

        public void Tick(float deltaTime, float speedScale)
        {
            if (_clip == null || Finished)
                return;

            _elapsed += deltaTime * speedScale;
            while (_elapsed >= FrameTime(Frame))
            {
                _elapsed -= FrameTime(Frame);
                if (Frame + 1 < _clip.frames.Length)
                {
                    Frame++;
                }
                else if (_clip.loop)
                {
                    Frame = 0;
                }
                else
                {
                    Finished = true;
                    _elapsed = 0f;
                    return;
                }
            }
        }

        private float FrameTime(int frame)
        {
            return Mathf.Max(_clip.durations[frame], MinFrameTime);
        }
    }
}
