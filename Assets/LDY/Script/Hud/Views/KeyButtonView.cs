using UnityEngine;

namespace LDY.Hud {
    // 활성/비활성 모양은 프리팹에 둘 다 있고 켜고 끄기만 한다.
    public sealed class KeyButtonView : MonoBehaviour {
        [SerializeField] private GameObject enabledFace;
        [SerializeField] private GameObject disabledFace;

        public void SetInteractable(bool interactable) {
            if (enabledFace != null)
                enabledFace.SetActive(interactable);
            if (disabledFace != null)
                disabledFace.SetActive(!interactable);
        }
    }
}
