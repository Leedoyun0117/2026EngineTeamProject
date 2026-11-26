using System;
using System.IO;

namespace LDY.Flow.Core {
    // 읽을 수 없는 파일은 옆에 .corrupt-시각 이름으로 옮겨 두고 빈 진행도로 시작한다.
    public sealed class JsonProgressStore : IProgressStore {
        public const string FileName = "progress.json";

        private readonly string _path;
        private readonly Func<DateTime> _now;

        public JsonProgressStore(string path, Func<DateTime> now = null) {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _now = now ?? (() => DateTime.Now);
        }

        public string Path => _path;
        public string LastBackupPath { get; private set; }

        public ProgressModel Load() {
            if (!File.Exists(_path)) return new ProgressModel();
            try {
                return ProgressJson.Read(File.ReadAllText(_path));
            }
            catch (ProgressFormatException) {
                Backup();
                return new ProgressModel();
            }
        }

        // 임시 파일에 쓰고 바꿔치기해서 저장 도중 꺼져도 이전 파일이 남게 한다.
        public void Save(ProgressModel progress) {
            var directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var temp = _path + ".tmp";
            File.WriteAllText(temp, ProgressJson.Write(progress));
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(temp, _path);
        }

        private void Backup() {
            LastBackupPath = $"{_path}.corrupt-{_now():yyyyMMdd-HHmmss}";
            if (File.Exists(LastBackupPath)) File.Delete(LastBackupPath);
            File.Move(_path, LastBackupPath);
        }
    }
}
