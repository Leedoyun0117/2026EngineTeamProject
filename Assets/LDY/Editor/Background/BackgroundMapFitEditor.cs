using LDY.Background;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LDY.BackgroundEditor {
    [CustomEditor(typeof(BackgroundMapFit))]
    internal sealed class BackgroundMapFitEditor : Editor {
        private static readonly string[] Settings = { "mode", "fixedScale", "anchor" };
        private static readonly string[] Recorded = { "fittedBounds", "scale", "boundsSource" };

        public override void OnInspectorGUI() {
            serializedObject.Update();
            foreach (var name in Settings) EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            serializedObject.ApplyModifiedProperties();
            using (new EditorGUI.DisabledScope(true))
                foreach (var name in Recorded)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(name));

            var fit = (BackgroundMapFit)target;
            var background = fit.GetComponent<StageBackground>();
            var inScene = fit.gameObject.scene.IsValid() && !EditorUtility.IsPersistent(fit);
            using (new EditorGUI.DisabledScope(background == null || !inScene || Application.isPlaying))
                if (GUILayout.Button("맵에 다시 맞추기") && BackgroundMapFitter.Fit(background))
                    EditorSceneManager.MarkSceneDirty(fit.gameObject.scene);
        }
    }
}
