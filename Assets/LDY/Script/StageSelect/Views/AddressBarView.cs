using LDY.Flow;
using TMPro;
using UnityEngine;

namespace LDY.StageSelect {
    public sealed class AddressBarView : MonoBehaviour {
        private const float LabelWidth = 48f;

        private TextMeshProUGUI _address;

        public void Build(UiFactory ui) {
            var label = ui.NewText("Label", transform, StageSelectText.AddressLabel);
            UiFactory.LeftColumn(label.rectTransform, 4f, LabelWidth);

            var field = ui.Sunken("Field", transform);
            UiFactory.Stretch(field.rectTransform, LabelWidth + 4f, 1f, 1f, 1f);
            var icon = ui.NewImage("Icon", field.transform, ui.Skin.folderOpen);
            UiFactory.Place(icon.rectTransform, 3f, 1f, 16f, 14f);
            _address = ui.NewText("Path", field.transform, "");
            UiFactory.Stretch(_address.rectTransform, 22f, 0f, 4f, 0f);
        }

        public void Show(string address) => _address.text = address;
    }
}
