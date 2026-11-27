using System;
using System.Collections.Generic;
using LDY.Background.Core;

namespace LDY.StageImport.Core {
    public sealed class StageFileException : Exception {
        public StageFileException(string message) : base(message) { }
    }

    public static class CursorStageParser {
        public static CursorStageFile Parse(string json) {
            object root;
            try {
                root = JsonParser.Parse(json);
            }
            catch (Exception e) when (!(e is StageFileException)) {
                throw new StageFileException("JSON을 읽지 못했다: " + e.Message);
            }

            var obj = AsObject(root, "$");
            var format = OptionalString(obj, "format", null);
            if (format != CursorStageFile.Format)
                throw new StageFileException($"format이 '{CursorStageFile.Format}'가 아니다: '{format}'");
            var version = (int)RequiredNumber(obj, "version", "$");
            if (version != CursorStageFile.SupportedVersion)
                throw new StageFileException($"version {version}은 지원하지 않는다 (지원: {CursorStageFile.SupportedVersion})");

            return new CursorStageFile(version,
                ReadList(obj, "stages", ReadStage),
                ReadList(obj, "custom", ReadCustom),
                ReadList(obj, "slots", ReadSlot));
        }

        private static ToolStage ReadStage(Dictionary<string, object> obj, string path) {
            var id = RequiredString(obj, "id", path);
            var width = (int)RequiredNumber(obj, "w", path);
            var height = (int)RequiredNumber(obj, "h", path);
            if (width <= 0 || height <= 0) throw new StageFileException($"{path}: 크기가 0 이하다 ({width}x{height})");

            var count = width * height;
            var cells = ReadStrings(obj, "cells", path);
            if (cells.Count != count) throw new StageFileException($"{path}.cells: 길이 {cells.Count}, 기대 {count}");
            var dark = ReadInts(obj, "darkMask", path, count);

            return new ToolStage(id, OptionalString(obj, "name", id), width, height, cells, dark,
                ReadList(obj, "notes", ReadNote), OptionalString(obj, "memo", ""), OptionalBool(obj, "dark"),
                OptionalString(obj, "status", ""));
        }

        private static ToolNote ReadNote(Dictionary<string, object> obj, string path) =>
            new ToolNote((int)RequiredNumber(obj, "x", path), (int)RequiredNumber(obj, "y", path),
                OptionalString(obj, "text", ""));

        private static CustomBlock ReadCustom(Dictionary<string, object> obj, string path) =>
            new CustomBlock(RequiredString(obj, "id", path), OptionalString(obj, "name", ""),
                OptionalString(obj, "color", ""), OptionalString(obj, "col", ""));

        private static ToolSlot ReadSlot(Dictionary<string, object> obj, string path) =>
            new ToolSlot(ReadStrings(obj, "ids", path), OptionalString(obj, "theme", ""));

        private static List<T> ReadList<T>(Dictionary<string, object> obj, string key,
            Func<Dictionary<string, object>, string, T> read) {
            var result = new List<T>();
            if (!obj.TryGetValue(key, out var value) || value == null) return result;
            var list = AsArray(value, key);
            for (var i = 0; i < list.Count; i++) {
                var path = $"{key}[{i}]";
                result.Add(read(AsObject(list[i], path), path));
            }

            return result;
        }

        private static List<string> ReadStrings(Dictionary<string, object> obj, string key, string path) {
            var result = new List<string>();
            if (!obj.TryGetValue(key, out var value) || value == null) return result;
            foreach (var item in AsArray(value, path + "." + key)) result.Add(item as string ?? "");
            return result;
        }

        private static List<int> ReadInts(Dictionary<string, object> obj, string key, string path, int count) {
            var result = new List<int>(count);
            if (!obj.TryGetValue(key, out var value) || value == null) {
                for (var i = 0; i < count; i++) result.Add(0);
                return result;
            }

            var list = AsArray(value, path + "." + key);
            if (list.Count != count) throw new StageFileException($"{path}.{key}: 길이 {list.Count}, 기대 {count}");
            foreach (var item in list) result.Add(item is double d ? (int)d : 0);
            return result;
        }

        private static Dictionary<string, object> AsObject(object value, string path) =>
            value as Dictionary<string, object> ?? throw new StageFileException(path + ": 객체가 아니다");

        private static List<object> AsArray(object value, string path) =>
            value as List<object> ?? throw new StageFileException(path + ": 배열이 아니다");

        private static string RequiredString(Dictionary<string, object> obj, string key, string path) =>
            obj.TryGetValue(key, out var value) && value is string s
                ? s
                : throw new StageFileException($"{path}.{key}: 문자열이 필요하다");

        private static double RequiredNumber(Dictionary<string, object> obj, string key, string path) =>
            obj.TryGetValue(key, out var value) && value is double d
                ? d
                : throw new StageFileException($"{path}.{key}: 숫자가 필요하다");

        private static string OptionalString(Dictionary<string, object> obj, string key, string fallback) =>
            obj.TryGetValue(key, out var value) && value is string s ? s : fallback;

        private static bool OptionalBool(Dictionary<string, object> obj, string key) =>
            obj.TryGetValue(key, out var value) && value is bool b && b;
    }
}
