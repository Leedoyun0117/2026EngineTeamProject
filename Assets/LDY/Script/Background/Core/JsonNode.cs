using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.Background.Core {
    public sealed class BackgroundDataException : Exception {
        public BackgroundDataException(string message) : base(message) { }
    }

    public sealed class JsonNode {
        private readonly object _value;

        public JsonNode(object value, string path) {
            _value = value;
            Path = path;
        }

        public string Path { get; }

        public static JsonNode Root(string json, string sourceName) {
            try {
                return new JsonNode(JsonParser.Parse(json), sourceName);
            }
            catch (FormatException e) {
                throw new BackgroundDataException($"{sourceName}: {e.Message}");
            }
        }

        public bool Has(string key) => AsObject().ContainsKey(key);

        public IEnumerable<string> Keys => AsObject().Keys;

        public JsonNode Child(string key) {
            if (!AsObject().TryGetValue(key, out var value))
                throw new BackgroundDataException($"{Path}: 필수 키 '{key}'가 없다");
            return new JsonNode(value, Path + "." + key);
        }

        public IReadOnlyList<JsonNode> Items() =>
            As<List<object>>("배열").Select((v, i) => new JsonNode(v, $"{Path}[{i}]")).ToList();

        public string String(string key) => Child(key).As<string>("문자열");
        public string String(string key, string fallback) => Has(key) ? String(key) : fallback;

        public float Number(string key) => (float)Child(key).As<double>("숫자");
        public float Number(string key, float fallback) => Has(key) ? Number(key) : fallback;

        public bool Bool(string key, bool fallback) => Has(key) ? Child(key).As<bool>("true/false") : fallback;

        public float[] Numbers(string key, int expectedCount) {
            var items = Child(key).Items();
            if (items.Count != expectedCount)
                throw new BackgroundDataException($"{Path}.{key}: 숫자 {expectedCount}개가 필요한데 {items.Count}개다");
            return items.Select(n => (float)n.As<double>("숫자")).ToArray();
        }

        public PixelRect Rect(string key) {
            var v = Numbers(key, 4);
            return new PixelRect(v[0], v[1], v[2], v[3]);
        }

        public Rgba Color(string key) {
            var text = String(key);
            if (!HexColor.TryParse(text, out var color))
                throw new BackgroundDataException($"{Path}.{key}: 색 형식이 아니다 '{text}'");
            return color;
        }

        public Rgba Color(string key, Rgba fallback) => Has(key) ? Color(key) : fallback;

        public IReadOnlyList<Rgba> Colors(string key) =>
            Child(key).Items().Select(n => {
                var text = n.As<string>("색 문자열");
                if (!HexColor.TryParse(text, out var color))
                    throw new BackgroundDataException($"{n.Path}: 색 형식이 아니다 '{text}'");
                return color;
            }).ToList();

        private Dictionary<string, object> AsObject() => As<Dictionary<string, object>>("객체");

        private T As<T>(string expected) {
            if (_value is T typed) return typed;
            throw new BackgroundDataException($"{Path}: {expected}가 필요하다");
        }
    }
}
