using UnityEngine;

namespace LDY.Script
{
    public class SpriteFacing : IFacing
    {
        private readonly Transform _transform;
        private readonly SpriteRenderer _sprite;

        public SpriteFacing(Transform transform, SpriteRenderer sprite)
        {
            _transform = transform;
            _sprite = sprite;
        }

        public FacingState Capture()
        {
            return new FacingState(_transform.localScale, _sprite.flipX);
        }

        public void Restore(FacingState state)
        {
            if (_transform == null)
                return;

            _transform.localScale = state.LocalScale;
            if (_sprite != null)
                _sprite.flipX = state.FlipX;
        }
    }
}
