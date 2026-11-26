using System.Linq;
using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class NotesPartBuilder : IBackgroundPartBuilder {
        private const float SpawnBand = 0.25f;

        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Notes) BuildNotes(context, spec);
        }

        private static void BuildNotes(BackgroundBuildContext context, NotesSpec spec) {
            var root = BackgroundObjects.Child(context.Anim, spec.Id);
            var area = spec.Area;
            var bottomLeft = context.Local(area.X, area.Y + area.Height);
            var topRight = context.Local(area.X + area.Width, area.Y);
            var height = topRight.y - bottomLeft.y;
            var spawnMax = new Vector2(topRight.x, bottomLeft.y + height * SpawnBand);

            BackgroundObjects.Ensure<NoteSpawner>(root.gameObject).Configure(
                context.LoadSprite(spec.File),
                spec.Colors.Select(c => c.ToColor()).ToArray(),
                bottomLeft, spawnMax, height * (1f - SpawnBand), BackgroundSorting.Anim + 1);
            context.Log.Info($"{spec.Id}: 음표 스프라이트 {spec.File}");
        }
    }
}
