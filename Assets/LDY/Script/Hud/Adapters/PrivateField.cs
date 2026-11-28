using System;
using System.Reflection;

namespace LDY.Hud.Adapters {
    // 팀원 코드에 공개 API가 없을 때만 쓰는 읽기 전용 필드 접근. 이름·타입이 바뀌면 null을 돌려 포트를 만들지 않는다.
    internal sealed class PrivateField<TValue> {
        private readonly FieldInfo _field;

        private PrivateField(FieldInfo field) {
            _field = field;
        }

        public static PrivateField<TValue> Find(Type owner, string name) {
            var field = owner.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return field != null && typeof(TValue).IsAssignableFrom(field.FieldType) ? new PrivateField<TValue>(field) : null;
        }

        public TValue Read(object owner) => owner != null ? (TValue)_field.GetValue(owner) : default;

        public string Describe => $"{_field.DeclaringType?.Name}.{_field.Name}";
    }
}
