using System.Collections.Generic;
using System.IO;
using LDY.Background;
using LDY.Background.Core;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundPrefabBuilder {
        private readonly IReadOnlyList<IBackgroundPartBuilder> _parts = new IBackgroundPartBuilder[] {
            new DesktopPartBuilder(),
            new StaticPartBuilder(),
            new TextPartBuilder(),
            new SelectionBarPartBuilder(),
            new ProgressPartBuilder(),
            new VisualizerPartBuilder(),
            new SheetAnimPartBuilder(),
            new NotesPartBuilder(),
            new CaretPartBuilder(),
            new DimPartBuilder()
        };

        public void Build(StageBackgroundSource source, StageBackgroundLayout layout, StageBackgroundTextSet textSet,
            BackgroundBuildLog log) {
            BackgroundAssetFolders.Ensure(Path.GetDirectoryName(source.PrefabPath));
            var created = AssetDatabase.LoadAssetAtPath<GameObject>(source.PrefabPath) == null;
            var root = created
                ? new GameObject(Path.GetFileNameWithoutExtension(source.PrefabPath))
                : PrefabUtility.LoadPrefabContents(source.PrefabPath);
            var font = BackgroundTextStyle.LoadFont();
            if (font == null) log.Warn($"{source.name}: 폰트 없음 {BackgroundTextStyle.FontPath} -> TMP 기본 폰트 사용");
            var meter = new TextWidthMeter(font, layout.Space);
            try {
                var context = new BackgroundBuildContext(root, layout, font, textSet, meter, log);
                foreach (var part in _parts) part.Build(context);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                BackgroundObjects.Ensure<BackgroundMapFit>(root);
                var background = root.GetComponent<StageBackground>();
                if (background == null) {
                    background = root.AddComponent<StageBackground>();
                    background.Configure(textSet, new Vector2(layout.Space.CanvasWidth, layout.Space.CanvasHeight),
                        layout.Space.PixelsPerUnit);
                }

                background.Apply();
                PrefabUtility.SaveAsPrefabAsset(root, source.PrefabPath);
                log.Info($"{(created ? "만듦" : "갱신")} {source.PrefabPath} (새 요소 {context.Created}, 유지 {context.Kept})");
            }
            finally {
                meter.Dispose();
                if (created) Object.DestroyImmediate(root);
                else PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }

    internal static class BackgroundAssetFolders {
        public static void Ensure(string folder) {
            folder = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
