using UnityEngine;

namespace LDY.Spider {
    public static class Vec2Convert {
        public static Vec2 ToVec2(this Vector2 v) => new Vec2(v.x, v.y);
        public static Vector2 ToUnity(this Vec2 v) => new Vector2(v.X, v.Y);
    }
}
