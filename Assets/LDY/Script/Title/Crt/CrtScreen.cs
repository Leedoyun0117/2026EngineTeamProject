using UnityEngine;

namespace LDY.Script
{
    // 편집 모드에서도 화면이 보이게 ExecuteAlways
    [ExecuteAlways]
    public class CrtScreen : MonoBehaviour
    {
        [SerializeField] private Camera contentCamera;
        [SerializeField] private Vector2Int resolution = new Vector2Int(960, 720);

        public RenderTexture Texture { get; private set; }
        public float Aspect => (float)resolution.x / resolution.y;

        private void OnEnable()
        {
            Texture = new RenderTexture(resolution.x, resolution.y, 24, RenderTextureFormat.ARGB32)
            {
                name = "CrtContent",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Texture.Create();
            contentCamera.targetTexture = Texture;
        }

        private void OnDisable()
        {
            if (contentCamera != null)
                contentCamera.targetTexture = null;
            if (Texture != null)
            {
                Texture.Release();
                DestroyImmediate(Texture);
                Texture = null;
            }
        }
    }
}
