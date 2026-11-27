using System.Collections.Generic;
using LDY.FlowEditor;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal sealed class ImportSelectedWindow : EditorWindow {
        private ImportSource _source;
        private Vector2 _scroll;

        public static void Open() {
            var window = GetWindow<ImportSelectedWindow>(true, "Stage Import");
            window.minSize = new Vector2(620f, 300f);
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

            EditorGUILayout.LabelField(_source.JsonPath, EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var job in _source.Jobs) DrawRow(job);
            EditorGUILayout.EndScrollView();
            if (_source.Assignments.Unslotted.Count > 0)
                EditorGUILayout.HelpBox("슬롯에 없는 맵: " + string.Join(", ", _source.Assignments.Unslotted), MessageType.None);
            foreach (var conflict in _source.Assignments.IdConflicts)
                EditorGUILayout.HelpBox($"id {conflict.StageId} 겹침, 가져오지 않음: {string.Join(", ", conflict.MapIds)}",
                    MessageType.Warning);
        }

        private void DrawRow(ImportJob job) {
            ImportJob selected = null;
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(job.Assignment.StageId, GUILayout.Width(40f));
                EditorGUILayout.LabelField(job.Assignment.Source == IdSource.Name ? "이름" : "슬롯 순서",
                    EditorStyles.miniLabel, GUILayout.Width(56f));
                EditorGUILayout.LabelField($"{job.Plan.Source.Name}  {job.Plan.Width}x{job.Plan.Height}");
                if (!job.SceneExists) {
                    if (GUILayout.Button("만들기", GUILayout.Width(140f))) selected = job;
                }
                else {
                    var next = job.AsNewVersion();
                    if (GUILayout.Button(new GUIContent($"새 이름으로 (v{next.Version})",
                            $"{job.SceneName}는 그대로 두고 {next.SceneName}를 만든다"), GUILayout.Width(140f)))
                        selected = next;
                }
            }

            if (selected == null) return;
            EditorApplication.delayCall += () => {
                StageImportMenu.Run(new List<ImportJob> { selected });
                Reload();
                Repaint();
            };
            GUIUtility.ExitGUI();
        }
    }
}
