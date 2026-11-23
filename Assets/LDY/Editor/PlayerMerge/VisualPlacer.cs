using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
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
            var idle = FindIdleSprite();
            if (idle == null || !_root.TryGetComponent<CapsuleCollider2D>(out var collider)) {
                _log.Notes.Add("PinkVisual 위치 계산 건너뜀: Idle 스프라이트 또는 CapsuleCollider2D가 없음");
                return;
            }

            var rows = SpriteFeetProbe.Measure(idle);
            if (rows.IsEmpty) {
                _log.Notes.Add("PinkVisual 위치 계산 건너뜀: Idle 스프라이트가 비어 있음");
                return;
            }

            var bottom = FeetAlignment.ColliderBottom(collider.offset.y, collider.size.y);
            var y = FeetAlignment.VisualLocalY(bottom, idle.pivot.y, rows.Lowest, idle.pixelsPerUnit);
            Place(_root.transform.Find(PlayerVisualContext.PinkName), y);
            ReportHeights(idle, rows, collider.size.y);
        }

        private void Place(Transform visual, float y) {
            var target = new Vector3(visual.localPosition.x, y, visual.localPosition.z);
            if (Vector3.Distance(visual.localPosition, target) < 0.0001f)
                return;

            _log.Changed.Add($"PinkVisual.localPosition: {visual.localPosition.ToString("0.####")} -> {target.ToString("0.####")}");
            visual.localPosition = target;
        }

        private void ReportHeights(Sprite idle, OpaqueRows rows, float colliderHeight) {
            var content = FeetAlignment.PixelsToUnits(rows.Height, idle.pixelsPerUnit);
            _log.Notes.Add($"스프라이트 높이 {rows.Height}px / PPU {idle.pixelsPerUnit} = {content:0.###} 유닛, "
                           + $"콜라이더 높이 {colliderHeight:0.###} 유닛, 차이 {colliderHeight - content:0.###} 유닛 (수정하지 않음)");
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

        private static Sprite FindIdleSprite() {
            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(PlayerAnimSetBuilder.CursorSetPath);
            if (set == null)
                return null;

            foreach (var entry in set.Clips)
                if (entry.id == PlayerAnimId.Idle && entry.clip.frames.Length > 0)
                    return entry.clip.frames[0];

            return null;
        }
    }
}
