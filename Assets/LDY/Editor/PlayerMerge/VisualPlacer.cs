using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // PinkVisual의 높이를 정해 Idle 그림의 발이 콜라이더 바닥에 닿게 한다. PinkVisual 배율을 반영한다.
    internal sealed class VisualPlacer {
        private readonly GameObject _root;
        private readonly SpriteRenderer _sprite;
        private readonly MergeLog _log;

        public VisualPlacer(GameObject root, SpriteRenderer sprite, MergeLog log) {
            _root = root;
            _sprite = sprite;
            _log = log;
        }

        public void Run() {
            ApplySorting();
            var idle = IdleSprite.Load();
            var visual = _root.transform.Find(PlayerParts.Visual);
            if (idle == null || visual == null || !_root.TryGetComponent<CapsuleCollider2D>(out var collider)) {
                _log.Notes.Add("PinkVisual 위치 계산 건너뜀: Idle 스프라이트, PinkVisual, CapsuleCollider2D 중 하나가 없음");
                return;
            }

            var rows = SpriteFeetProbe.Measure(idle);
            if (rows.IsEmpty) {
                _log.Notes.Add("PinkVisual 위치 계산 건너뜀: Idle 스프라이트가 비어 있음");
                return;
            }

            var pixelsPerUnit = VisualScale.EffectivePixelsPerUnit(idle.pixelsPerUnit, visual.localScale.y);
            var bottom = FeetAlignment.ColliderBottom(collider.offset.y, collider.size.y);
            var y = FeetAlignment.VisualLocalY(bottom, idle.pivot.y, rows.Lowest, pixelsPerUnit);
            Place(visual, y);
            _log.Notes.Add($"콜라이더 바닥 {bottom:0.####}, 발 행 {rows.Lowest}px, 실효 PPU {pixelsPerUnit:0.##} → PinkVisual.y {y:0.####}");
        }

        private void Place(Transform visual, float y) {
            var target = new Vector3(visual.localPosition.x, y, visual.localPosition.z);
            if (Vector3.Distance(visual.localPosition, target) < 0.0001f)
                return;

            _log.Changed.Add($"{PlayerParts.Visual}.localPosition: {visual.localPosition.ToString("0.####")} -> {target.ToString("0.####")}");
            visual.localPosition = target;
        }

        private void ApplySorting() {
            var so = new SerializedObject(_sprite);
            var property = so.FindProperty("m_SortingOrder");
            if (property.intValue == PlayerArt.StageSortingOrder)
                return;

            _log.Changed.Add($"Sprite.SortingOrder: {property.intValue} -> {PlayerArt.StageSortingOrder}");
            property.intValue = PlayerArt.StageSortingOrder;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
