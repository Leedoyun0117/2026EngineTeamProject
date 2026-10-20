using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Script
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image hourglass;
        [SerializeField] private TextMeshProUGUI label;

        private HourglassAnimator _animator;
        private LoadingDots _dots;
        private Sprite _temporarySprite;

        public static LoadingOverlay Create(LoadingOverlay prefab, Transform parent)
        {
            return Instantiate(prefab, parent, false);
        }

        private void Awake()
        {
            hourglass.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _animator = new HourglassAnimator(hourglass);
            _dots = LoadingDots.Attach(label);
            canvas.enabled = false;
        }

        public void Show(string text, Sprite hourglassSprite, Sprite[] frames, float frameDuration,
            float flipDuration, AnimationCurve flipCurve, TMP_FontAsset font)
        {
            if (font != null)
                label.font = font;

            if (HasSprite(frames))
            {
                _animator.Play(frames, frameDuration, flipDuration, flipCurve);
            }
            else
            {
                if (hourglassSprite == null)
                    hourglassSprite = _temporarySprite != null ? _temporarySprite : (_temporarySprite = CreateTemporaryHourglass());
                _animator.Play(new[] { hourglassSprite }, frameDuration, flipDuration, flipCurve);
            }

            _dots.Play(text);
            canvas.enabled = true;
        }

        public void Hide()
        {
            if (canvas == null)
                return;

            _animator.Stop();
            _dots.Stop();
            canvas.enabled = false;
        }

        private void Update()
        {
            if (!canvas.enabled)
                return;

            _animator.Tick();
        }

        private static bool HasSprite(Sprite[] frames)
        {
            if (frames == null)
                return false;

            foreach (Sprite frame in frames)
            {
                if (frame != null)
                    return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            if (_temporarySprite != null)
            {
                Destroy(_temporarySprite.texture);
                Destroy(_temporarySprite);
            }
        }

        private static Sprite CreateTemporaryHourglass()
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float distance = Mathf.Abs(y - (size - 1) * 0.5f) / ((size - 1) * 0.5f);
                float halfWidth = distance * 6.5f + 0.5f;
                bool edge = y == 0 || y == size - 1;
                for (int x = 0; x < size; x++)
                {
                    bool filled = edge ? x >= 2 && x < size - 2 : Mathf.Abs(x - (size - 1) * 0.5f) <= halfWidth;
                    pixels[y * size + x] = filled ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
