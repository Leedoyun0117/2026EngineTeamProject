using LDY.Background;
using LDY.Background.Core;

namespace LDY.BackgroundEditor {
    internal sealed class SheetAnimPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Sheets) BuildSheet(context, spec);
        }

        private static void BuildSheet(BackgroundBuildContext context, SheetAnimSpec spec) {
            if (!context.TryCreate(context.Anim, spec.Id, out var go)) return;
            var frames = context.LoadFrames(spec.File);
            if (frames.Length != spec.Frames)
                context.Log.Warn($"{spec.Id}: 프레임 {spec.Frames}개 기대, {frames.Length}개");

            go.transform.localPosition = context.Local(spec.CenterX, spec.CenterY);
            context.AddSprite(go, null, BackgroundSorting.Anim);
            go.AddComponent<SpriteSheetPlayer>().Configure(frames, spec.FrameMs);
        }
    }
}
