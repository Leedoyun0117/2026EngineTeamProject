using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // 비주얼을 PinkVisual 하나로 정리하고, 그림이 A(JJBLevel)와 같은 월드 크기가 되도록 PinkVisual에만 배율을 준다.
    internal sealed class VisualStep {
        private readonly GameObject _jibin;
        private readonly GameObject _final;
        private readonly MergeLog _log;

        public VisualStep(GameObject jibin, GameObject final, MergeLog log) {
            _jibin = jibin;
            _final = final;
            _log = log;
        }

        public void Run() {
            RemoveLegacyRenderers();
            ApplyScale();
        }

        private void RemoveLegacyRenderers() {
            var legacy = _final.transform.Find(PlayerParts.LegacyVisual);
            if (legacy == null)
                return;

            foreach (var typeName in PlayerParts.LegacyVisualComponents) {
                var component = ComponentNames.Find(legacy.gameObject, typeName);
                if (component == null)
                    continue;

                Object.DestroyImmediate(component);
                _log.Changed.Add($"-{PlayerParts.LegacyVisual}.{typeName} (비주얼은 {PlayerParts.Visual} 하나만 씀)");
            }
        }

        private void ApplyScale() {
            var visual = _final.transform.Find(PlayerParts.Visual);
            var jibinSprite = _jibin.transform.Find(PlayerParts.LegacyVisual)?.GetComponent<SpriteRenderer>();
            var idle = IdleSprite.Load();
            if (visual == null || jibinSprite == null || jibinSprite.sprite == null || idle == null) {
                _log.Notes.Add("PinkVisual 배율 계산 건너뜀: PinkVisual, A의 Visual 스프라이트, Idle 스프라이트 중 하나가 없음");
                return;
            }

            if (!Mathf.Approximately(idle.pixelsPerUnit, PlayerArt.PixelsPerUnit))
                _log.Notes.Add($"Idle 스프라이트 PPU가 {idle.pixelsPerUnit} (기대 {PlayerArt.PixelsPerUnit}). PPU는 바꾸지 않음");

            var jibinScale = _jibin.transform.localScale.y * jibinSprite.transform.localScale.y;
            var jibinRows = SpriteFeetProbe.Measure(jibinSprite.sprite);
            var finalRows = SpriteFeetProbe.Measure(idle);
            var reference = VisualScale.WorldHeight(jibinRows.Height, jibinSprite.sprite.pixelsPerUnit, jibinScale);
            var choice = VisualScale.Choose(PlayerParts.DefaultVisualScale, reference, finalRows.Height, idle.pixelsPerUnit);
            var height = VisualScale.WorldHeight(finalRows.Height, idle.pixelsPerUnit, choice.Scale);

            _log.Notes.Add($"그림 높이: A {jibinSprite.sprite.name} {jibinRows.Height}px / PPU {jibinSprite.sprite.pixelsPerUnit} × {jibinScale} = {reference:0.####}u, "
                           + $"최종 {idle.name} {finalRows.Height}px / PPU {idle.pixelsPerUnit} × {choice.Scale:0.####} = {height:0.####}u "
                           + (choice.UsedDefault
                               ? $"(기본 배율 {PlayerParts.DefaultVisualScale} = 40/50로 일치)"
                               : $"(기본 배율 {PlayerParts.DefaultVisualScale}로는 달라 높이 비율로 다시 계산)"));

            var next = new Vector3(choice.Scale, choice.Scale, 1f);
            if (visual.localScale == next)
                return;

            _log.Changed.Add($"{PlayerParts.Visual}.localScale: {visual.localScale.ToString("0.####")} -> {next.ToString("0.####")}");
            visual.localScale = next;
        }
    }

    internal static class IdleSprite {
        public static Sprite Load() {
            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(PlayerAnimSetBuilder.CursorSetPath);
            return set != null ? set.FirstFrame(PlayerAnimId.Idle) : null;
        }
    }
}
