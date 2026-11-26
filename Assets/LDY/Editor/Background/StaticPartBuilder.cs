using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class StaticPartBuilder : IBackgroundPartBuilder {
        private const string ReferenceName = "Reference";

        public void Build(BackgroundBuildContext context) {
            var space = context.Space;
            var center = context.Local(space.CanvasWidth * 0.5f, space.CanvasHeight * 0.5f);
            var window = BackgroundObjects.SpriteChild(context.Static, "window_base", context.LoadSprite(context.Layout.StaticFile),
                BackgroundSorting.Static);
            window.transform.localPosition = center;

            if (context.Layout.ReferenceFile == null) {
                context.Log.Warn("검증용 레퍼런스 이미지 없음");
                return;
            }

            var reference = BackgroundObjects.SpriteChild(context.Static, ReferenceName,
                context.LoadSprite(context.Layout.ReferenceFile), BackgroundSorting.Text + 1);
            reference.transform.localPosition = center;
            reference.color = new Color(1f, 1f, 1f, 0.5f);
            reference.gameObject.SetActive(false);
            context.Log.Info("검증 기준 " + context.Layout.ReferenceFile + " (Reference 오브젝트, 기본 꺼짐)");
        }
    }
}
