using System.Collections.Generic;
using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 프리팹 안의 비활성 템플릿을 복제해 쓰는 월드 표시 풀. 템플릿 자체는 쓰지 않는다.
    public sealed class UiPool : MonoBehaviour {
        [SerializeField] private UiWorldAnchor template;

        private readonly List<UiWorldAnchor> _items = new List<UiWorldAnchor>();

        public void ShowAbove(IReadOnlyList<HudRect> targets) {
            var count = targets?.Count ?? 0;
            for (var i = 0; i < count; i++) {
                var item = Get(i);
                if (item != null)
                    item.ShowAbove(targets[i]);
            }

            for (var i = count; i < _items.Count; i++)
                _items[i].Hide();
        }

        public void HideAll() => ShowAbove(null);

        private UiWorldAnchor Get(int index) {
            if (template == null)
                return null;

            while (_items.Count <= index) {
                var item = Instantiate(template, template.transform.parent);
                item.name = $"{template.name} {_items.Count}";
                _items.Add(item);
            }

            return _items[index];
        }
    }
}
