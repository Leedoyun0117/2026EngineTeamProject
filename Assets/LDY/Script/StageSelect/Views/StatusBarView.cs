using LDY.Flow;
using LDY.Flow.Core;
using TMPro;
using UnityEngine;

namespace LDY.StageSelect {
    public sealed class StatusBarView : MonoBehaviour {
        private TextMeshProUGUI _text;

        public void Build(UiFactory ui) {
            _text = ui.NewText("Summary", transform, "");
            UiFactory.Stretch(_text.rectTransform, 4f, 0f, 4f, 0f);
        }

        public void Show(StageSummary summary) =>
            _text.text = StageSelectText.Summary(summary.Total, summary.Cleared, summary.Locked);
    }
}
