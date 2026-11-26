using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LDY.Background.Core {
    public sealed class JsonParser {
        private readonly string _text;
        private int _pos;

        private JsonParser(string text) {
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public static object Parse(string text) {
            var parser = new JsonParser(text);
            var value = parser.ReadValue();
            parser.SkipWhitespace();
            if (parser._pos != parser._text.Length) throw parser.Error("값 뒤에 남은 문자가 있다");
            return value;
        }

        private object ReadValue() {
            SkipWhitespace();
            if (_pos >= _text.Length) throw Error("값이 필요하다");
            switch (_text[_pos]) {
                case '{': return ReadObject();
                case '[': return ReadArray();
                case '"': return ReadString();
                case 't': return ReadLiteral("true", true);
                case 'f': return ReadLiteral("false", false);
                case 'n': return ReadLiteral("null", null);
                default: return ReadNumber();
            }
        }

        private Dictionary<string, object> ReadObject() {
            var result = new Dictionary<string, object>();
            _pos++;
            SkipWhitespace();
            if (TryConsume('}')) return result;
            do {
                SkipWhitespace();
                var key = ReadString();
                SkipWhitespace();
                Expect(':');
                result[key] = ReadValue();
                SkipWhitespace();
            } while (TryConsume(','));

            Expect('}');
            return result;
        }

        private List<object> ReadArray() {
            var result = new List<object>();
            _pos++;
            SkipWhitespace();
            if (TryConsume(']')) return result;
            do {
                result.Add(ReadValue());
                SkipWhitespace();
            } while (TryConsume(','));

            Expect(']');
            return result;
        }

        private string ReadString() {
            Expect('"');
            var sb = new StringBuilder();
            while (_pos < _text.Length) {
                var c = _text[_pos++];
                if (c == '"') return sb.ToString();
                if (c != '\\') {
                    sb.Append(c);
                    continue;
                }

                if (_pos >= _text.Length) break;
                var e = _text[_pos++];
                switch (e) {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (_pos + 4 > _text.Length) throw Error("\\u 이스케이프가 짧다");
                        sb.Append((char)int.Parse(_text.Substring(_pos, 4), NumberStyles.HexNumber));
                        _pos += 4;
                        break;
                    default: sb.Append(e); break;
                }
            }

            throw Error("문자열이 닫히지 않았다");
        }

        private double ReadNumber() {
            var start = _pos;
            while (_pos < _text.Length && "+-0123456789.eE".IndexOf(_text[_pos]) >= 0) _pos++;
            if (start == _pos) throw Error($"예상하지 못한 문자 '{_text[start]}'");
            var token = _text.Substring(start, _pos - start);
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw Error($"숫자 형식이 아니다: {token}");
            return value;
        }

        private object ReadLiteral(string literal, object value) {
            if (string.CompareOrdinal(_text, _pos, literal, 0, literal.Length) != 0) throw Error("알 수 없는 리터럴");
            _pos += literal.Length;
            return value;
        }

        private void SkipWhitespace() {
            while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos])) _pos++;
        }

        private bool TryConsume(char c) {
            if (_pos < _text.Length && _text[_pos] == c) {
                _pos++;
                return true;
            }

            return false;
        }

        private void Expect(char c) {
            if (!TryConsume(c)) throw Error($"'{c}'가 필요하다");
        }

        private FormatException Error(string message) => new FormatException($"JSON 위치 {_pos}: {message}");
    }
}
