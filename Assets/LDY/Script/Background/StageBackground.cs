using System.Collections.Generic;
using UnityEngine;

namespace LDY.Background {
    public sealed class StageBackground : MonoBehaviour {
        [SerializeField] private StageBackgroundTextSet textSet;
        [SerializeField] private List<StageBackgroundTextSet.Entry> overrides = new List<StageBackgroundTextSet.Entry>();

        public void Configure(StageBackgroundTextSet set) {
            textSet = set;
            ApplyTexts();
        }

        public void ApplyTexts() {
            foreach (var label in GetComponentsInChildren<StageBackgroundText>(true))
                if (TryResolve(label.Id, out var text))
                    label.SetText(text);
        }

        private bool TryResolve(string id, out string text) {
            if (StageBackgroundTextSet.TryFind(overrides, id, out text)) return true;
            return textSet != null && textSet.TryGet(id, out text);
        }

        private void Awake() => ApplyTexts();

        private void OnValidate() => ApplyTexts();
    }
}
