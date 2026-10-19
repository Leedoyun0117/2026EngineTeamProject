using System;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
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

        // 강조 직전 색을 저장해 뒀다가 되돌림(강조 전에 바뀐 색도 따라가게)
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
                Debug.LogError($"[MenuIcon] {name}: action 연결 안 됨", this);
                return;
            }

            action.Execute();
            Executed?.Invoke(this);
        }
    }
}
