using UnityEngine;

namespace LDY.Script
{
    // 화면 곡률 왜곡. 셰이더(CrtScreen.shader의 CrtWarp)와 반드시 같은 식을 유지해야 보이는 위치와 판정 위치가 일치한다.
    public static class CrtCurvature
    {
        // 화면(표시) UV -> 콘텐츠 UV.
        public static Vector2 Warp(Vector2 uv, float curvature)
        {
            Vector2 p = uv * 2f - Vector2.one;
            p += new Vector2(p.x * p.y * p.y, p.y * p.x * p.x) * curvature;
            return p * 0.5f + new Vector2(0.5f, 0.5f);
        }
    }
}
