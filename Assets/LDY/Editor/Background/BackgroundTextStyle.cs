using System;
using LDY.Background.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.BackgroundEditor {
    internal static class BackgroundTextStyle {
        public const string FontPath = "Assets/LDY/Fonts/Galmuri9 Pixel.asset";
        private const float TmpWorldScale = 0.1f;

        public static TMP_FontAsset LoadFont() => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        public static void Apply(TMP_Text text, TMP_FontAsset font, PixelSpace space) {
            if (font != null) text.font = font;
            text.enableAutoSizing = false;
            text.fontSize = space.ToUnits(TextFit.PixelSize) / TmpWorldScale;
        }
    }

    internal sealed class TextWidthMeter : IDisposable {
        private readonly GameObject _go;
        private readonly TextMeshPro _text;
        private readonly PixelSpace _space;

        public TextWidthMeter(TMP_FontAsset font, PixelSpace space) {
            _space = space;
            _go = new GameObject("TextWidthMeter") { hideFlags = HideFlags.HideAndDontSave };
            _text = _go.AddComponent<TextMeshPro>();
            _text.textWrappingMode = TextWrappingModes.NoWrap;
            BackgroundTextStyle.Apply(_text, font, space);
        }

        public float WidthPixels(string value) =>
            string.IsNullOrEmpty(value) ? 0f : _text.GetPreferredValues(value).x * _space.PixelsPerUnit;

        public float WrappedHeightPixels(string value, float widthPixels) =>
            string.IsNullOrEmpty(value)
                ? 0f
                : _text.GetPreferredValues(value, _space.ToUnits(widthPixels), float.PositiveInfinity).y * _space.PixelsPerUnit;

        public void Dispose() => Object.DestroyImmediate(_go);
    }
}
