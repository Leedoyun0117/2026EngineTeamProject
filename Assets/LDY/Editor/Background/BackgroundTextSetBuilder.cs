using System.IO;
using LDY.Background;
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
            if (added > 0) {
                EditorUtility.SetDirty(set);
                log.Info($"{source.TextSetPath}: 문구 {added}개 추가 (기존 문구는 유지)");
            }

            return set;
        }
    }
}
