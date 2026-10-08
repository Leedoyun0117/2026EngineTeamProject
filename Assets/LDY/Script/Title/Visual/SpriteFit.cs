using UnityEngine;

namespace LDY.Script
{
    // 스프라이트 원본 크기가 달라도 목표 크기에 맞도록 스케일을 정한다. 부모는 스케일이 없다고 가정한다.
    public static class SpriteFit
    {
        public static void Stretch(SpriteRenderer renderer, Vector2 target)
        {
            Vector2 size = SizeOf(renderer);
            if (size.x <= 0f || size.y <= 0f)
                return;

            renderer.transform.localScale = new Vector3(target.x / size.x, target.y / size.y, 1f);
        }

        public static void Contain(SpriteRenderer renderer, Vector2 target)
        {
            Vector2 size = SizeOf(renderer);
            if (size.x <= 0f || size.y <= 0f)
                return;

            float scale = Mathf.Min(target.x / size.x, target.y / size.y);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static Vector2 SizeOf(SpriteRenderer renderer)
        {
            return renderer.sprite != null ? (Vector2)renderer.sprite.bounds.size : Vector2.zero;
        }
    }
}
