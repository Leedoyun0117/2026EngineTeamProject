using LDY.PlayerMerge;
using UnityEngine;

namespace LDY.Script {
    internal static class Float2Extensions {
        public static Float2 ToFloat2(this Vector2 value) => new Float2(value.x, value.y);

        public static Float2 ToFloat2(this Vector3 value) => new Float2(value.x, value.y);

        public static Vector2 ToVector2(this Float2 value) => new Vector2(value.X, value.Y);
    }
}
