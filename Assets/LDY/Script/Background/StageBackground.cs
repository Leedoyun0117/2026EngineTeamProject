using System.Collections.Generic;
using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class StageBackground : MonoBehaviour {
        [SerializeField] private StageBackgroundTextSet textSet;
        [SerializeField] private List<StageBackgroundTextSet.Entry> overrides = new List<StageBackgroundTextSet.Entry>();
        [SerializeField] private Vector2 canvasPixels = new Vector2(640f, 360f);
        [SerializeField, Min(1f)] private float pixelsPerUnit = 40f;
        [SerializeField, Range(0f, 1f)] private float dim = BackgroundDim.Default;

        public StageBackgroundTextSet TextSet => textSet;

        public void Configure(StageBackgroundTextSet set, Vector2 canvas, float ppu) {
            textSet = set;
            canvasPixels = canvas;
            pixelsPerUnit = ppu;
            Apply();
        }

        public void Apply() {
            ApplyTexts();
            ApplyDim();
        }

        private void ApplyTexts() {
            var space = new PixelSpace(canvasPixels.x, canvasPixels.y, pixelsPerUnit);
            foreach (var label in GetComponentsInChildren<StageBackgroundText>(true)) {
                if (TryResolve(label.Id, e => true, out var entry)) label.SetText(entry.text);
                if (TryResolve(label.Id, e => e.overrideRect, out entry))
                    label.SetRect(space, new PixelRect(entry.rect.x, entry.rect.y, entry.rect.width, entry.rect.height));
                if (TryResolve(label.Id, e => e.showDecided, out entry)) label.SetVisible(entry.show);
            }
        }

        private void ApplyDim() {
            foreach (var target in GetComponentsInChildren<IBackgroundDimmable>(true)) target.ApplyDim(dim);
        }

        private bool TryResolve(string id, System.Predicate<StageBackgroundTextSet.Entry> has,
            out StageBackgroundTextSet.Entry entry) {
            if (StageBackgroundTextSet.TryFind(overrides, id, out entry) && has(entry)) return true;
            return textSet != null && textSet.TryGet(id, out entry) && has(entry);
        }

        private void Awake() => Apply();

#if UNITY_EDITOR
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += () => {
            if (this != null) Apply();
        };
#endif
    }
}
