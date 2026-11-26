using LDY.Background;

namespace LDY.BackgroundEditor {
    internal sealed class SheetAnimPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Sheets) BuildSheet(context, spec);
        }

        private static void BuildSheet(BackgroundBuildContext context, SheetAnimSpec spec) {
            var frames = context.LoadFrames(spec.File);
            if (frames.Length != spec.Frames)
                context.Log.Warn($"{spec.Id}: 프레임 {spec.Frames}개 기대, {frames.Length}개 (Import Art를 먼저 실행)");

            var renderer = BackgroundObjects.SpriteChild(context.Anim, spec.Id, null, BackgroundSorting.Anim);
            renderer.transform.localPosition = context.Local(spec.CenterX, spec.CenterY);
            BackgroundObjects.Ensure<SpriteSheetPlayer>(renderer.gameObject).Configure(frames, spec.FrameMs);
        }
    }
}
