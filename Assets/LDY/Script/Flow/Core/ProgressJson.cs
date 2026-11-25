using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LDY.Background.Core;

namespace LDY.Flow.Core {
    public sealed class ProgressFormatException : Exception {
        public ProgressFormatException(string message) : base(message) { }
    }

    // {"version":1,"stages":[{"id":"1-T","cleared":true,"best":12.5}],"unlocked":["2-T"]}
    public static class ProgressJson {
        public const int Version = 1;

        public static string Write(ProgressModel progress) {
            var sb = new StringBuilder();
            sb.Append("{\n  \"version\": ").Append(Version).Append(",\n  \"stages\": [");
            var first = true;
            foreach (var pair in progress.Records.OrderBy(p => p.Key, StringComparer.Ordinal)) {
                sb.Append(first ? "\n    " : ",\n    ");
                first = false;
                sb.Append("{\"id\": ").Append(Quote(pair.Key))
                    .Append(", \"cleared\": ").Append(pair.Value.Cleared ? "true" : "false");
                if (pair.Value.BestSeconds.HasValue)
                    sb.Append(", \"best\": ").Append(pair.Value.BestSeconds.Value.ToString("R", CultureInfo.InvariantCulture));
                sb.Append('}');
            }

            sb.Append(first ? "]" : "\n  ]").Append(",\n  \"unlocked\": [");
            sb.Append(string.Join(", ", progress.ForcedUnlocks.OrderBy(id => id, StringComparer.Ordinal).Select(Quote)));
            sb.Append("]\n}\n");
            return sb.ToString();
        }

        public static ProgressModel Read(string json) {
            object root;
            try {
                root = JsonParser.Parse(json);
            }
            catch (FormatException e) {
                throw new ProgressFormatException(e.Message);
            }

            var obj = Expect<Dictionary<string, object>>(root, "최상위 객체");
            var version = (int)Expect<double>(Field(obj, "version"), "version 숫자");
            if (version != Version) throw new ProgressFormatException($"지원하지 않는 version {version}");

            var progress = new ProgressModel();
            foreach (var item in Expect<List<object>>(Field(obj, "stages"), "stages 배열")) {
                var stage = Expect<Dictionary<string, object>>(item, "stages 항목");
                var id = Expect<string>(Field(stage, "id"), "id 문자열");
                var cleared = Expect<bool>(Field(stage, "cleared"), "cleared true/false");
                double? best = stage.TryGetValue("best", out var raw) && raw != null
                    ? Expect<double>(raw, "best 숫자")
                    : (double?)null;
                progress.Restore(id, new StageRecord(cleared, best));
            }

            if (obj.TryGetValue("unlocked", out var unlocked) && unlocked != null)
                foreach (var id in Expect<List<object>>(unlocked, "unlocked 배열"))
                    progress.ForceUnlock(Expect<string>(id, "unlocked 문자열"));

            return progress;
        }

        private static object Field(Dictionary<string, object> obj, string key) {
            if (!obj.TryGetValue(key, out var value)) throw new ProgressFormatException($"'{key}' 키가 없다");
            return value;
        }

        private static T Expect<T>(object value, string expected) {
            if (value is T typed) return typed;
            throw new ProgressFormatException(expected + "가 필요하다");
        }

        private static string Quote(string text) =>
            "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
