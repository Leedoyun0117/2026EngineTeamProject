using System.Collections.Generic;
using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal sealed class ImportSelectedWindow : EditorWindow {
        private ImportSource _source;
        private Vector2 _scroll;

        public static void Open() {
            var window = GetWindow<ImportSelectedWindow>(true, "Stage Import");
            window.minSize = new Vector2(520f, 300f);
            window.Reload();
        }

        private void Reload() {
            var log = new FlowLog("[Stage Import] Load");
            _source = ImportSource.Load(log);
            log.Print();
        }

        private void OnGUI() {
            if (GUILayout.Button("다시 읽기")) Reload();
            if (_source == null) {
                EditorGUILayout.HelpBox("파일을 읽지 못했다. Console을 확인.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(_source.Settings.jsonPath, EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var job in _source.Jobs) DrawRow(job);
            EditorGUILayout.EndScrollView();
            if (_source.Assignments.Unslotted.Count > 0)
                EditorGUILayout.HelpBox("슬롯에 없는 맵: " + string.Join(", ", _source.Assignments.Unslotted), MessageType.None);
        }

        private void DrawRow(ImportJob job) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(job.Assignment.StageId, GUILayout.Width(40f));
                EditorGUILayout.LabelField($"{job.Plan.Source.Name}  {job.Plan.Width}x{job.Plan.Height}");
                using (new EditorGUI.DisabledScope(job.SceneExists)) {
                    if (!GUILayout.Button(job.SceneExists ? "이미 있음" : "만들기", GUILayout.Width(80f))) return;
                }
            }

            var selected = job;
            EditorApplication.delayCall += () => {
                StageImportMenu.Run(new List<ImportJob> { selected });
                Reload();
                Repaint();
            };
            GUIUtility.ExitGUI();
        }
    }
}
