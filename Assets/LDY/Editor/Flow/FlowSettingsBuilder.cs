using LDY.Flow;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    internal static class FlowSettingsBuilder {
        public static FlowSettings Build(StageCatalog catalog, UiSkin skin, FlowLog log) {
            var settings = AssetDatabase.LoadAssetAtPath<FlowSettings>(FlowPaths.Settings);
            var created = settings == null;
            if (created) {
                FlowPaths.EnsureFolder(System.IO.Path.GetDirectoryName(FlowPaths.Settings).Replace('\\', '/'));
                settings = ScriptableObject.CreateInstance<FlowSettings>();
                AssetDatabase.CreateAsset(settings, FlowPaths.Settings);
            }

            settings.catalog = catalog;
            settings.skin = skin;
            settings.titleScenePath = FlowPaths.TitleScene;
            settings.selectScenePath = FlowPaths.SelectScene;
            EditorUtility.SetDirty(settings);
            log.Info((created ? "만듦 " : "갱신 ") + FlowPaths.Settings);
            return settings;
        }

        public static FlowSettings Load() => AssetDatabase.LoadAssetAtPath<FlowSettings>(FlowPaths.Settings);
    }
}
