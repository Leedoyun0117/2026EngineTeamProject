using LDY.PlayerMerge;
using UnityEngine;

namespace LDY.Script {
    internal static class SpriteFeetProbe {
        public static OpaqueRows Measure(Sprite sprite) {
            var rect = sprite.textureRect;
            var pixels = ReadPixels(sprite.texture, rect);
            var alpha = new byte[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
                alpha[i] = pixels[i].a;

            return FeetAlignment.FindOpaqueRows(alpha, (int)rect.width, (int)rect.height);
        }

        private static Color32[] ReadPixels(Texture2D source, Rect rect) {
            var buffer = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            var copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            try {
                Graphics.Blit(source, buffer);
                RenderTexture.active = buffer;
                copy.ReadPixels(rect, 0, 0);
                copy.Apply();
                return copy.GetPixels32();
            }
            finally {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(buffer);
                Object.DestroyImmediate(copy);
            }
        }
    }
}
