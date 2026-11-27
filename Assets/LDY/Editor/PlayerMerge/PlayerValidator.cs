using System.Collections.Generic;
using System.Linq;
using LDY.PlayerLocate;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // LDY/Player/Validate의 검사 항목. 값을 읽어 PlayerRules로 판정만 하고 아무것도 고치지 않는다.
    internal sealed class PlayerValidator {
        private readonly JibinReference _reference;
        private readonly SpriteRows _rows;
        private readonly Sprite _idle;
        private readonly List<CheckRow> _out = new();

        public PlayerValidator() {
            _rows = new SpriteRows();
            _reference = JibinReference.Load(_rows);
            _idle = IdleSprite.Load();
        }

        public IReadOnlyList<CheckRow> Rows => _out;

        public void Check(GameObject root, string target, bool isCopy) {
            var probe = new PlayerProbe(root);
            Add(target, "Missing Script 0개", probe.MissingScripts == 0, probe.MissingScripts + "개");
            Add(target, "루트 스케일 1", PlayerRules.UnitScale(probe.RootScale), probe.RootScale.ToString());
            Add(target, "태그 / 레이어", root.CompareTag(PlayerParts.PlayerTag) && root.layer == LayerMask.NameToLayer(PlayerParts.PlayerLayer),
                $"{root.tag} / {LayerMask.LayerToName(root.layer)}");
            Add(target, "회전 고정", PlayerRules.RotationFrozen(probe.Constraints), "Constraints " + probe.Constraints);
            CheckMasks(probe, target);
            CheckComponents(root, target, isCopy);
            CheckBody(probe, target);
            CheckVisual(probe, target);
            CheckSkills(probe, target, isCopy);
        }

        public void CheckVariant(GameObject copy, GameObject final) {
            const string target = "PlayerCopy (Variant)";
            Add(target, "최종본의 Variant", CopyVariantBuilder.IsVariantOf(copy, final),
                PrefabUtility.GetPrefabAssetType(copy) + " / 원본 " + AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(copy)));
            Add(target, "이름이 복제체 규칙(Copy/Clone)", PrimaryPlayerRule.LooksLikeCopy(copy.name), copy.name);
        }

        public void CheckName(GameObject body, string target) =>
            Add(target, "이름이 본체 규칙(Copy/Clone 없음)", !PrimaryPlayerRule.LooksLikeCopy(body.name), body.name);

        public void Add(string target, string label, bool passed, string detail) =>
            _out.Add(new CheckRow(target, label, passed, detail));

        private void CheckMasks(PlayerProbe probe, string target) {
            var masks = new[] {
                ("objectLayer", probe.Mask("JJBPlayerInteract", "objectLayer")),
                ("groundLayer", probe.Mask("JJBPlayerJump", "groundLayer")),
                ("absorbLayer", probe.Mask("JJBPlayerAbsorb", "absorbLayer"))
            };
            foreach (var (name, bits) in masks)
                Add(target, name + " 비어 있지 않음", PlayerRules.MaskFilled(bits), bits.ToString());
        }

        private void CheckComponents(GameObject root, string target, bool isCopy) {
            var onRoot = ComponentNames.On(root).ToList();
            var required = isCopy ? PlayerParts.CommonRootComponents : PlayerParts.CommonRootComponents.Concat(PlayerParts.BodyOnlyRootComponents);
            var missing = PlayerRules.Missing(onRoot, required);
            missing.AddRange(PlayerRules.Missing(ComponentNames.InChildren(root), PlayerParts.VisualComponents));
            Add(target, "필수 컴포넌트", missing.Count == 0, missing.Count == 0 ? "모두 있음" : "없음: " + string.Join(", ", missing));
            if (!isCopy)
                return;

            var leftover = PlayerRules.Forbidden(onRoot, PlayerParts.CopyRemovedComponents);
            Add(target, "복제체에 없어야 할 컴포넌트", leftover.Count == 0, leftover.Count == 0 ? "없음" : string.Join(", ", leftover));
        }

        private void CheckBody(PlayerProbe probe, string target) {
            if (!probe.TryCollider(out var offset, out var size)) {
                Add(target, "콜라이더", false, "CapsuleCollider2D 없음");
                return;
            }

            var bottom = FeetAlignment.ColliderBottom(offset.Y, size.Y);
            Compare(target, "콜라이더 월드 크기 = A", size, _reference?.ColliderSize);
            Compare(target, "콜라이더 월드 오프셋 = A", offset, _reference?.ColliderOffset);
            if (!probe.TryGroundCheck(out var point, out var box)) {
                Add(target, "GroundCheck", false, "JJBPlayerJump.groundCheck 없음");
                return;
            }

            Compare(target, "GroundCheck 월드 위치 = A(JJBLevel)", point, _reference?.GroundCheck);
            Compare(target, "GroundCheck 상자 크기 = A", box, _reference?.GroundCheckSize);
            Add(target, "GroundCheck가 콜라이더 바닥 ±0.1u", PlayerRules.GroundCheckReachesBottom(point.Y, box.Y, bottom),
                $"상자 중심 {point.Y:0.###}, 높이 {box.Y:0.###}, 콜라이더 바닥 {bottom:0.###}");
            Add(target, "GroundCheck 폭 ≤ 콜라이더 폭", PlayerRules.GroundCheckFitsWidth(box.X, size.X), $"{box.X:0.###} / {size.X:0.###}");
        }

        private void CheckVisual(PlayerProbe probe, string target) {
            var shown = probe.ShownRenderers;
            var renderer = probe.RendererTarget;
            Add(target, "비주얼 1개 (Animator 0개)", PlayerRules.SingleVisual(shown.Count, probe.Animators),
                $"보이는 SpriteRenderer {shown.Count}개 ({string.Join(", ", shown.Select(r => r.name))}), Animator {probe.Animators}개");
            Add(target, "보이는 렌더러 = PlayerRenderer.Target", renderer != null && shown.Count == 1 && shown[0] == renderer,
                renderer != null ? renderer.name : "Target 없음");
            if (probe.Root.GetComponent<AltModeController>() != null)
                Add(target, "Alt.playerSprite = PlayerRenderer.Target", probe.AltSprite == renderer,
                    probe.AltSprite != null ? probe.AltSprite.name : "비어 있음");

            if (renderer == null || _idle == null || !probe.TryCollider(out var offset, out var size))
                return;

            var rows = _rows.Of(_idle);
            var pixel = renderer.transform.lossyScale.y / _idle.pixelsPerUnit;
            var feet = probe.FeetY(renderer, _idle, rows);
            var bottom = FeetAlignment.ColliderBottom(offset.Y, size.Y);
            Add(target, "발과 콜라이더 바닥 차이 ≤ 1px", PlayerRules.FeetAligned(feet, bottom, pixel),
                $"발 {feet:0.####}, 바닥 {bottom:0.####}, 1px {pixel:0.####}u");

            var height = VisualScale.WorldHeight(rows.Height, _idle.pixelsPerUnit, renderer.transform.lossyScale.y);
            if (_reference == null)
                Add(target, "그림 높이 = A", true, $"{height:0.###}u (A 없음, 비교 생략)");
            else
                Add(target, "그림 높이 = A", Mathf.Abs(height - _reference.VisualHeight) < RootScaleConversion.WorldTolerance,
                    $"{height:0.####}u / A {_reference.VisualHeight:0.####}u");
        }

        private void CheckSkills(PlayerProbe probe, string target, bool isCopy) {
            var missing = PlayerParts.HtyChildren.Where(name => !probe.HasChild(name)).ToList();
            Add(target, "스킬 자식 (Skills, Player Light)", missing.Count == 0, missing.Count == 0 ? "있음" : "없음: " + string.Join(", ", missing));
            var active = PlayerParts.HtyChildren.Where(probe.ChildActive).ToList();
            if (isCopy)
                Add(target, "복제체의 스킬 자식 꺼짐", active.Count == 0, active.Count == 0 ? "꺼짐" : "켜짐: " + string.Join(", ", active));
            else
                Add(target, "본체의 스킬 자식 켜짐", active.Count == PlayerParts.HtyChildren.Length, string.Join(", ", active));
        }

        private void Compare(string target, string label, Float2 actual, Float2? expected) {
            if (expected == null) {
                Add(target, label, true, $"{actual} (A 없음, 비교 생략)");
                return;
            }

            Add(target, label, RootScaleConversion.SameWorld(actual, expected.Value), $"{actual} / A {expected.Value}");
        }
    }
}
