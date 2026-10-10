using System;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 플레이어가 올라서는 발판(platform)과 커서 hover 영역(hitArea, 아이콘+이름)을 가진 메뉴 항목.
    // 모양과 크기는 MenuIconVisual이 정하고, 이 클래스는 판정과 실행만 담당한다.
    public class MenuIcon : MonoBehaviour
    {
        [SerializeField] private MenuActionBehaviour action;
        [SerializeField] private Collider2D platform;
        [SerializeField] private Collider2D hitArea;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color iconHighlightColor = new Color(0.22f, 0.38f, 0.7f);
        [SerializeField] private Color labelHighlightColor = new Color(1f, 0.92f, 0.45f);
        [Tooltip("보조 화면이 열려 있어도 사용할 수 있다 (닫기 버튼용)")]
        [SerializeField] private bool usableWhileBlocked;

        private Color _iconColor;
        private Color _labelColor;
        private bool _highlighted;

        public event Action<MenuIcon> Executed;

        public bool IsUsable(IMenuGate gate)
        {
            return isActiveAndEnabled && (usableWhileBlocked || gate.CanExecute);
        }

        public void SetPlatformEnabled(bool enabled)
        {
            if (platform != null)
                platform.enabled = enabled;
        }

        public bool ContainsPoint(Vector2 worldPoint)
        {
            return hitArea != null && hitArea.OverlapPoint(worldPoint);
        }

        public bool IsStoodOn(Collider2D body)
        {
            if (platform == null || !platform.IsTouching(body))
                return false;

            return body.bounds.min.y >= platform.bounds.max.y - TitleTuning.StandTolerance;
        }

        // 강조 직전의 색을 저장했다가 되돌리므로, 강조 중이 아닐 때 바뀐 색(이미지 교체 등)을 그대로 따른다.
        public void SetHighlighted(bool highlighted)
        {
            if (_highlighted == highlighted)
                return;

            _highlighted = highlighted;
            if (highlighted)
            {
                _iconColor = iconRenderer != null ? iconRenderer.color : Color.white;
                _labelColor = label != null ? label.color : Color.white;
            }

            if (iconRenderer != null)
                iconRenderer.color = highlighted ? iconHighlightColor : _iconColor;
            if (label != null)
                label.color = highlighted ? labelHighlightColor : _labelColor;
        }

        public void Execute()
        {
            if (action == null)
            {
                Debug.LogError($"[MenuIcon] {name}: action이 연결되어 있지 않습니다.", this);
                return;
            }

            action.Execute();
            Executed?.Invoke(this);
        }
    }
}
