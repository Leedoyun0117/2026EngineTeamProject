using System.Collections.Generic;
using System.IO;
using LDY.Background;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundPrefabBuilder {
        private readonly IReadOnlyList<IBackgroundPartBuilder> _parts = new IBackgroundPartBuilder[] {
            new StaticPartBuilder(),
            new TextPartBuilder(),
            new SelectionBarPartBuilder(),
            new ProgressPartBuilder(),
            new VisualizerPartBuilder(),
            new SheetAnimPartBuilder(),
            new NotesPartBuilder()
        };

        public void Build(StageBackgroundSource source, StageBackgroundLayout layout, StageBackgroundTextSet textSet,
            BackgroundBuildLog log) {
            BackgroundAssetFolders.Ensure(Path.GetDirectoryName(source.PrefabPath));
            var created = AssetDatabase.LoadAssetAtPath<GameObject>(source.PrefabPath) == null;
            var root = created
                ? new GameObject(Path.GetFileNameWithoutExtension(source.PrefabPath))
                : PrefabUtility.LoadPrefabContents(source.PrefabPath);
            try {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(source.FontPath);
                if (font == null) log.Warn($"{source.name}: 폰트 없음 {source.FontPath} -> TMP 기본 폰트 사용");
                var context = new BackgroundBuildContext(root, layout, font, source.FontPixelSize, log);
                foreach (var part in _parts) part.Build(context);
                BackgroundObjects.Ensure<BackgroundCameraFit>(root);
                BackgroundObjects.Ensure<StageBackground>(root).Configure(textSet);
                PrefabUtility.SaveAsPrefabAsset(root, source.PrefabPath);
                log.Info((created ? "만듦 " : "갱신 ") + source.PrefabPath);
            }
            finally {
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
