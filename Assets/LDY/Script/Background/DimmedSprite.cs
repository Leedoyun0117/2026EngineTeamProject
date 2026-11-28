using UnityEngine;

namespace LDY.Background {
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DimmedSprite : MonoBehaviour, IBackgroundDimmable {
        [SerializeField] private Color baseColor = Color.white;

        public void Configure(Color color) => baseColor = color;

        public void ApplyDim(float dim) => GetComponent<SpriteRenderer>().color = DimColor.Of(baseColor, dim);
    }
}
