using UnityEngine;

namespace LDY.Script
{
    // 플레이어 방향을 정하는 유일한 곳. 상태는 SpriteRenderer.flipX 자체이므로 Alt 스냅샷 복원과 어긋나지 않는다.
    // 원본 아트가 오른쪽을 본다고 보고, 왼쪽(-1)일 때 뒤집는다.
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
