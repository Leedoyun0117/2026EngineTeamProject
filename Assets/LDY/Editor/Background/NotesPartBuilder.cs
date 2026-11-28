using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class NotesPartBuilder : IBackgroundPartBuilder {
        private const float SpawnBand = 0.25f;
        private const int PoolSize = 8;

        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Notes) BuildNotes(context, spec);
        }

        private static void BuildNotes(BackgroundBuildContext context, NotesSpec spec) {
            if (!context.TryCreate(context.Anim, spec.Id, out var root)) return;
            var area = spec.Area;
            var bottomLeft = context.Local(area.X, area.Y + area.Height);
            var topRight = context.Local(area.X + area.Width, area.Y);
            var height = topRight.y - bottomLeft.y;
            var spawnMax = new Vector2(topRight.x, bottomLeft.y + height * SpawnBand);

            var sprite = context.LoadSprite(spec.File);
            var pool = Enumerable.Range(0, PoolSize)
                .Select(i => context.NewSprite(root.transform, "Note_" + i, sprite, BackgroundSorting.Anim + 1))
                .ToArray();
            root.AddComponent<NoteSpawner>().Configure(pool, spec.Colors.Select(c => c.ToColor()).ToArray(),
                bottomLeft, spawnMax, height * (1f - SpawnBand));
        }
    }
}
