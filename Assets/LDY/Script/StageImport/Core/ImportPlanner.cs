using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public static class ImportPlanner {
        public static ImportPlan Plan(ToolStage stage, CursorStageFile file, IKeyRuleSource mapped) {
            var plan = new ImportPlan(stage);
            var resolved = new StageGrid<Resolved>(stage.Width, stage.Height,
                (x, y) => Resolve(stage.Cells[y * stage.Width + x] ?? "", file, mapped, plan));

            for (var y = 0; y < resolved.Height; y++)
            for (var x = 0; x < resolved.Width; x++)
                PlaceCell(plan, resolved[x, y], new GridPoint(x, y));

            PlaceRuns(plan, resolved);
            PlaceGroups(plan, resolved);
            PlaceAreas(plan, resolved, stage);
            PlaceNotes(plan, stage);
            return plan;
        }

        private readonly struct Resolved {
            public Resolved(string key, KeyRule rule, bool valid) {
                Key = key;
                Rule = rule;
                Valid = valid;
            }

            public string Key { get; }
            public KeyRule Rule { get; }
            public bool Valid { get; }

            public bool Is(Placement placement) => Valid && Rule.Placement == placement;
        }

        private static Resolved Resolve(string raw, CursorStageFile file, IKeyRuleSource mapped, ImportPlan plan) {
            if (raw == ToolKeys.Empty) return default;
            Increment(plan.KeyCounts, raw);
            if (ToolKeys.IsCustom(raw)) return ResolveCustom(raw, file, mapped, plan);
            if (DefaultKeyRules.TryGet(raw, out var rule) || mapped.TryGet(raw, out rule))
                return new Resolved(raw, rule, true);
            Increment(plan.Unmapped, raw);
            return default;
        }

        private static Resolved ResolveCustom(string raw, CursorStageFile file, IKeyRuleSource mapped, ImportPlan plan) {
            var custom = file.FindCustom(raw);
            var use = plan.Customs.Find(c => c.Id == raw);
            if (use == null) {
                var resolution = CustomBlockResolver.Resolve(custom, mapped);
                use = new CustomUse(raw, custom?.Name ?? "", custom?.Col ?? "", resolution.Outcome, resolution.Key);
                plan.Customs.Add(use);
            }

            use.Count++;
            switch (use.Outcome) {
                case CustomOutcome.Skipped:
                    return default;
                case CustomOutcome.Unknown:
                    Increment(plan.Unmapped, raw);
                    return default;
                default:
                    var rule = DefaultKeyRules.TryGet(use.Key, out var known) ? known : RuleFromSource(mapped, use.Key);
                    return new Resolved(use.Key, rule, true);
            }
        }

        private static KeyRule RuleFromSource(IKeyRuleSource mapped, string key) =>
            mapped.TryGet(key, out var rule) ? rule : new KeyRule(Placement.Single);

        private static void PlaceCell(ImportPlan plan, Resolved cell, GridPoint p) {
            if (!cell.Valid) return;
            switch (cell.Rule.Placement) {
                case Placement.Tile:
                    plan.Tiles.Add(new KeyedCell(cell.Key, p));
                    break;
                case Placement.Decoration:
                    plan.Decorations.Add(new KeyedCell(cell.Key, p));
                    break;
                case Placement.Single:
                    plan.Prefabs.Add(new KeyedRect(cell.Key, GridRect.Cell(p)));
                    break;
                case Placement.Start:
                    plan.Starts.Add(p);
                    break;
                case Placement.Goal:
                    plan.Goals.Add(p);
                    break;
            }
        }

        private static void PlaceRuns(ImportPlan plan, StageGrid<Resolved> grid) {
            foreach (var key in DistinctKeys(grid, Placement.HorizontalRun))
            foreach (var run in RegionFinder.HorizontalRuns(grid.Width, grid.Height,
                         (x, y) => grid[x, y].Is(Placement.HorizontalRun) && grid[x, y].Key == key))
                plan.Prefabs.Add(new KeyedRect(key, run));
        }

        private static void PlaceGroups(ImportPlan plan, StageGrid<Resolved> grid) {
            foreach (var key in DistinctKeys(grid, Placement.Group4))
            foreach (var region in RegionFinder.Connected4(grid.Width, grid.Height,
                         (x, y) => grid[x, y].Is(Placement.Group4) && grid[x, y].Key == key))
                plan.Groups.Add(new KeyedGroup(key, region));
        }

        private static void PlaceAreas(ImportPlan plan, StageGrid<Resolved> grid, ToolStage stage) {
            var dark = StageGrids.Dark(stage);
            AddAreas(plan, AreaKind.Dark, grid.Width, grid.Height, (x, y) => dark[x, y]);
            AddAreas(plan, AreaKind.Tab, grid.Width, grid.Height,
                (x, y) => grid[x, y].Is(Placement.Area) && grid[x, y].Rule.Area == AreaKind.Tab);
        }

        private static void AddAreas(ImportPlan plan, AreaKind kind, int width, int height,
            System.Func<int, int, bool> match) {
            foreach (var region in RegionFinder.Connected4(width, height, match))
                plan.Areas.Add(new AreaRegion(kind, region, RectCover.Cover(region)));
        }

        private static void PlaceNotes(ImportPlan plan, ToolStage stage) {
            foreach (var note in stage.Notes) {
                if (note.X < 0 || note.Y < 0 || note.X >= stage.Width || note.Y >= stage.Height) continue;
                plan.Notes.Add(new PlacedNote(CellCoords.ToUnity(note.X, note.Y, stage.Height), note.Text));
            }
        }

        private static IEnumerable<string> DistinctKeys(StageGrid<Resolved> grid, Placement placement) {
            var keys = new SortedSet<string>();
            for (var y = 0; y < grid.Height; y++)
            for (var x = 0; x < grid.Width; x++)
                if (grid[x, y].Is(placement))
                    keys.Add(grid[x, y].Key);
            return keys;
        }

        private static void Increment(IDictionary<string, int> counts, string key) {
            counts.TryGetValue(key, out var count);
            counts[key] = count + 1;
        }
    }
}
