using System.IO;
using LDY.Background;
using LDY.Background.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundTextSetBuilder {
        public StageBackgroundTextSet Build(StageBackgroundSource source, StageBackgroundLayout layout,
            BackgroundBuildLog log) {
            var set = AssetDatabase.LoadAssetAtPath<StageBackgroundTextSet>(source.TextSetPath);
            if (set == null) {
                BackgroundAssetFolders.Ensure(Path.GetDirectoryName(source.TextSetPath));
                set = ScriptableObject.CreateInstance<StageBackgroundTextSet>();
                AssetDatabase.CreateAsset(set, source.TextSetPath);
                log.Info("만듦 " + source.TextSetPath);
            }

            var added = 0;
            foreach (var slot in layout.Texts)
                if (set.AddMissing(slot.Id, slot.DefaultText))
                    added++;
            if (added > 0) log.Info($"{source.TextSetPath}: 문구 {added}개 추가 (기존 문구는 유지)");

            var decided = 0;
            foreach (var slot in layout.Texts)
                if (set.DecideShowIfMissing(slot.Id, BackgroundTextDefaults.ShownByDefault(slot.Id)))
                    decided++;
            if (decided > 0) log.Info($"{source.TextSetPath}: show 기본값 {decided}개 지정 (사람이 정한 값은 유지)");

            FitRects(source, layout, set, log);
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void FitRects(StageBackgroundSource source, StageBackgroundLayout layout,
            StageBackgroundTextSet set, BackgroundBuildLog log) {
            using (var meter = new TextWidthMeter(BackgroundTextStyle.LoadFont(), layout.Space)) {
                foreach (var slot in layout.Texts) {
                    set.TryGet(slot.Id, out var entry);
                    if (entry.overrideRect) continue;
                    var wraps = TextFit.Wraps(slot.Rect);
                    var needed = wraps || slot.Mask ? 0f : meter.WidthPixels(entry.text);
                    var fitted = TextFit.Expand(slot.Rect, slot.Align, needed);
                    if (TextFit.Same(fitted, slot.Rect)) continue;
                    set.SetRectIfMissing(slot.Id, new Rect(fitted.X, fitted.Y, fitted.Width, fitted.Height));
                    log.Info($"{source.name} rect 덮어쓰기 {slot.Id}: {Describe(slot.Rect)} -> {Describe(fitted)} (글자 {needed:0}px)");
                }
            }
        }

        public static string Describe(PixelRect r) => $"[{r.X:0.#}, {r.Y:0.#}, {r.Width:0.#}, {r.Height:0.#}]";
    }
}
