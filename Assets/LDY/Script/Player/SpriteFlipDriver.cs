using UnityEngine;

namespace LDY.Script
{
    public class SpriteFlipDriver
    {
        private readonly SpriteRenderer _renderer;

        public SpriteFlipDriver(SpriteRenderer renderer)
        {
            _renderer = renderer;
        }

        public void Apply(int facingSign)
        {
            if (facingSign != 0)
                _renderer.flipX = facingSign < 0;
        }
    }
}
