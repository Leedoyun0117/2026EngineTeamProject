using UnityEngine;

namespace LDY.Script
{
    public static class CrtCurvature
    {
        public static Vector2 Warp(Vector2 uv, float curvature)
        {
            Vector2 p = uv * 2f - Vector2.one;
            p += new Vector2(p.x * p.y * p.y, p.y * p.x * p.x) * curvature;
            return p * 0.5f + new Vector2(0.5f, 0.5f);
        }
    }
}
