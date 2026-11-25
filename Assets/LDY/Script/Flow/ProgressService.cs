using System;
using System.IO;
using LDY.Flow.Core;
using UnityEngine;

namespace LDY.Flow {
    public sealed class ProgressService {
        private readonly IProgressStore _store;

        public ProgressService(IProgressStore store) {
            _store = store;
            Model = Load();
        }

        public static string DefaultPath => Path.Combine(Application.persistentDataPath, JsonProgressStore.FileName);

        public ProgressModel Model { get; private set; }
        public event Action Changed;

        public bool RecordClear(string id, double seconds) {
            var improved = Model.RecordClear(id, seconds);
            Save();
            return improved;
        }

        public void Apply(Action<ProgressModel> change) {
            change(Model);
            Save();
        }

        public void Reload() {
            Model = Load();
            Changed?.Invoke();
        }

        private ProgressModel Load() {
            var model = _store.Load();
            if (_store is JsonProgressStore json && json.LastBackupPath != null)
                Debug.LogWarning("[Flow] 진행도 파일이 손상돼 초기화했다. 백업: " + json.LastBackupPath);
            return model;
        }

        private void Save() {
            try {
                _store.Save(Model);
            }
            catch (IOException e) {
                Debug.LogError("[Flow] 진행도 저장 실패: " + e.Message);
            }

            Changed?.Invoke();
        }
    }
}
